import { screen, within, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { expect, it } from 'vitest';
import { renderPage } from '@/test/render';
import { seedApplication } from '@/test/mocks/applications';
import { server } from '@/test/mocks/server';
import { mentorFixture, mentorQuestionnaireFixture } from '@/test/fixtures/api';

it('sends from the profile and cancels only after confirmation', async () => {
  const user = userEvent.setup();
  renderPage(`/mentors/${mentorFixture.userId}`, 0);
  const send = await screen.findByRole('button', { name: 'Отправить заявку' });
  expect(screen.getByLabelText('Сопроводительное сообщение')).toBeDisabled();
  await user.click(send);
  expect(await screen.findByRole('status')).toHaveTextContent(
    'Заявка отправлена.',
  );
  expect(
    screen.getByRole('button', { name: 'Заявка отправлена' }),
  ).toBeDisabled();
  await user.click(
    within(screen.getByRole('status')).getByRole('link', {
      name: 'Мои заявки',
    }),
  );
  const cancel = await screen.findByRole('button', { name: 'Отменить заявку' });
  expect(
    screen.getByRole('region', { name: 'Статус заявки' }),
  ).toHaveTextContent('Рассматривает');
  await user.click(cancel);
  let dialog = within(screen.getByRole('dialog', { name: 'Отменить заявку?' }));
  await user.click(dialog.getByRole('button', { name: 'Не отменять' }));
  expect(cancel).toHaveFocus();
  await user.click(cancel);
  dialog = within(screen.getByRole('dialog'));
  await user.click(dialog.getByRole('button', { name: 'Да, отменить заявку' }));
  expect(await screen.findByText('Заявок пока нет.')).toBeVisible();
});

it('moves an accepted application to the curator and approves it there', async () => {
  seedApplication();
  const user = userEvent.setup();
  const mentor = renderPage('/mentor', 1);
  expect(await screen.findByRole('button', { name: 'Принять' })).toBeEnabled();
  expect(
    screen.queryByRole('button', { name: 'Уточнить' }),
  ).not.toBeInTheDocument();
  await user.click(screen.getByRole('button', { name: 'Принять' }));
  expect(await screen.findByText('Заявок пока нет.')).toBeVisible();
  mentor.unmount();
  renderPage('/approval-queue', 2);
  const details = await screen.findAllByRole('button', { name: /Подробнее/ });
  expect(details).toHaveLength(2);
  for (const [index, name] of ['Анна Иванова', 'Мария Петрова'].entries()) {
    await user.click(details[index]!);
    const dialog = within(screen.getByRole('dialog'));
    expect(dialog.getByRole('heading', { name })).toBeVisible();
    expect(dialog.getByText('Москва, UTC+3')).toBeVisible();
    await user.click(dialog.getByRole('button', { name: 'Закрыть окно' }));
    expect(details[index]).toHaveFocus();
  }
  await user.click(screen.getByRole('button', { name: 'Утвердить' }));
  expect(await screen.findByText('Заявок пока нет.')).toBeVisible();
});

it.each(['mentor', 'tutor'] as const)(
  'refuses as %s with a required reason and shows the agreed student variant',
  async (role) => {
    seedApplication({ mentorApproved: role === 'tutor' ? true : null });
    const user = userEvent.setup();
    const page = renderPage(
      role === 'mentor' ? '/mentor' : '/approval-queue',
      role === 'mentor' ? 1 : 2,
    );
    await user.click(await screen.findByRole('button', { name: 'Отклонить' }));
    const dialog = within(
      screen.getByRole('dialog', {
        name: role === 'tutor' ? 'Отклонение заявки' : 'Отмена заявки',
      }),
    );
    const confirm = dialog.getByRole('button', {
      name: role === 'tutor' ? 'Отклонить заявку' : 'Отменить заявку',
    });
    await user.click(confirm);
    expect(dialog.getByText('Укажите причину.')).toBeVisible();
    await user.type(dialog.getByRole('textbox'), 'Нет свободного времени');
    expect(dialog.queryByText('Укажите причину.')).not.toBeInTheDocument();
    await user.click(confirm);
    expect(await screen.findByText('Заявок пока нет.')).toBeVisible();
    page.unmount();
    renderPage('/applications', 0);
    expect(
      await screen.findByRole('heading', { name: 'Причина отказа' }),
    ).toBeVisible();
    expect(screen.getByText('Нет свободного времени')).toBeVisible();
    expect(
      screen.getByRole('region', { name: 'Статус заявки' }),
    ).toHaveTextContent('Отклонено');
    expect(
      screen.queryByRole('button', { name: 'Отменить заявку' }),
    ).not.toBeInTheDocument();
  },
);

it('keeps the confirmation open on failure and prevents duplicate submission', async () => {
  seedApplication();
  let calls = 0;
  let release: () => void = () => {};
  server.use(
    http.delete('*/api/applications', async () => {
      calls++;
      await new Promise<void>((resolve) => {
        release = resolve;
      });
      return new HttpResponse(null, { status: 503 });
    }),
  );
  const user = userEvent.setup();
  renderPage('/applications', 0);
  await user.click(
    await screen.findByRole('button', { name: 'Отменить заявку' }),
  );
  await user.click(screen.getByRole('button', { name: 'Да, отменить заявку' }));
  await waitFor(() => expect(calls).toBe(1));
  expect(screen.getByRole('button', { name: 'Сохранение…' })).toBeDisabled();
  release();
  expect(await screen.findByRole('alert')).toHaveTextContent(
    'Не удалось выполнить действие.',
  );
  expect(screen.getByRole('dialog')).toBeVisible();
  expect(screen.getByRole('heading', { name: 'Мария Петрова' })).toBeVisible();
});

it('distinguishes list failure from a 404 empty list and retries', async () => {
  server.use(
    http.get(
      '*/api/applications',
      () => new HttpResponse(null, { status: 503 }),
    ),
  );
  renderPage('/applications', 0);
  const user = userEvent.setup();
  expect(await screen.findByRole('alert')).toHaveTextContent(
    'Не удалось загрузить заявки.',
  );
  server.use(
    http.get(
      '*/api/applications',
      () => new HttpResponse(null, { status: 404 }),
    ),
  );
  await user.click(screen.getByRole('button', { name: 'Повторить' }));
  expect(await screen.findByText('Заявок пока нет.')).toBeVisible();
});

it('accepts a raw array, nullable questionnaire and neutral missing values', async () => {
  server.use(
    http.get('*/api/applications', () =>
      HttpResponse.json([
        {
          id: crypto.randomUUID(),
          questionnaireForm: null,
          reasonForRefusal: null,
        },
        {
          id: crypto.randomUUID(),
          questionnaireForm: mentorQuestionnaireFixture,
          reasonForRefusal: '  ',
        },
      ]),
    ),
  );
  renderPage('/applications', 0);
  expect(
    await screen.findByRole('heading', { name: 'Анкета недоступна' }),
  ).toBeVisible();
  expect(
    screen.getAllByRole('button', { name: 'Отменить заявку' }),
  ).toHaveLength(2);
  expect(
    screen.queryByRole('heading', { name: 'Причина отказа' }),
  ).not.toBeInTheDocument();
});

it.each([
  ['/applications', undefined, 'Вход'],
  ['/applications', 1, 'Доступ запрещён'],
  ['/approval-queue', 0, 'Доступ запрещён'],
  ['/mentor', 2, 'Доступ запрещён'],
] as const)('restricts %s for role %s', async (path, role, heading) => {
  let requests = 0;
  server.use(
    http.get('*/api/applications', () => {
      requests++;
      return HttpResponse.json([]);
    }),
  );
  renderPage(path, role);
  expect(await screen.findByRole('heading', { name: heading })).toBeVisible();
  expect(requests).toBe(0);
});
