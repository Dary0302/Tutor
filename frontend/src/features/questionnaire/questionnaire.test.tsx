import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { expect, it } from 'vitest';
import { server } from '@/test/mocks/server';
import { renderPage } from '@/test/render';
import { questionnaireFixture } from '@/test/fixtures/api';

it('treats 404 as a blank form, validates required fields and edits skills/windows', async () => {
  renderPage('/questionnaire', 0);
  const user = userEvent.setup();
  await user.click(await screen.findByRole('button', { name: 'Сохранить' }));
  expect(await screen.findAllByText('Обязательное поле')).not.toHaveLength(0);
  expect(screen.getByText('Навыки пока не добавлены')).toBeVisible();
  await user.type(
    screen.getByPlaceholderText('Введите название навыка'),
    'Python',
  );
  await user.click(screen.getByRole('button', { name: 'Добавить' }));
  const tag = screen.getByRole('button', { name: 'Python · Начальный' });
  await user.click(tag);
  const skillDialog = screen.getByRole('dialog', { name: 'Уровень навыка' });
  await user.selectOptions(within(skillDialog).getByLabelText('Уровень'), '2');
  await user.click(
    within(skillDialog).getByRole('button', { name: 'Сохранить' }),
  );
  expect(
    screen.getByRole('button', { name: 'Python · Продвинутый' }),
  ).toHaveFocus();
  await user.click(
    screen.getByRole('button', { name: 'Удалить навык Python' }),
  );
  expect(screen.getByText('Навыки пока не добавлены')).toBeVisible();
  await user.click(screen.getByRole('button', { name: /Пн/ }));
  await user.click(screen.getByRole('button', { name: /Добавить окно/ }));
  expect(screen.getByLabelText('Начало')).toHaveValue('18:00');
  await user.click(screen.getByRole('button', { name: 'Удалить окно' }));
  expect(screen.queryByLabelText('Начало')).not.toBeInTheDocument();
  await user.click(screen.getByRole('button', { name: 'Закрыть окно' }));
});
it.each([0, 1])(
  'loads and saves role %s while preserving edits on refetch',
  async (role) => {
    let saved: unknown;
    const fixture = {
      ...questionnaireFixture,
      teacherInfo: role
        ? {
            placeOfWork: 'Яндекс',
            position: 'Разработчик',
            yearsExperience: 5,
            maxStudents: 3,
          }
        : null,
    };
    server.use(
      http.get('*/api/questionnaires/me', () => HttpResponse.json(fixture)),
      http.put('*/api/questionnaires/me', async ({ request }) => {
        saved = await request.json();
        return new HttpResponse(null, { status: 204 });
      }),
    );
    const { client } = renderPage('/questionnaire', role);
    const user = userEvent.setup();
    const name = await screen.findByLabelText('Имя *');
    expect(name).toHaveValue('Анна');
    expect(
      screen.getByRole('button', { name: 'Меню профиля' }),
    ).toHaveTextContent('А');
    await user.clear(name);
    await user.type(name, 'Мария');
    expect(
      screen.getByRole('button', { name: 'Меню профиля' }),
    ).toHaveTextContent('А');
    await client.invalidateQueries({ queryKey: ['questionnaire'] });
    expect(name).toHaveValue('Мария');
    await user.click(screen.getByRole('button', { name: 'Сохранить' }));
    expect(await screen.findByRole('status')).toHaveTextContent(
      'Анкета сохранена.',
    );
    expect(saved).toMatchObject({
      firstName: 'Мария',
      skills: [{ name: 'TypeScript', skillLevel: 0 }],
    });
    expect(
      screen.getByRole('button', { name: 'Меню профиля' }),
    ).toHaveTextContent('М');
  },
);
it('preserves input on server field validation failure and network error', async () => {
  server.use(
    http.get('*/api/questionnaires/me', () =>
      HttpResponse.json(questionnaireFixture),
    ),
    http.put('*/api/questionnaires/me', () =>
      HttpResponse.json(
        {
          detail: 'Validation failed',
          errors: { firstName: ['Проверьте имя'] },
        },
        { status: 400 },
      ),
    ),
  );
  renderPage('/questionnaire', 0);
  const user = userEvent.setup();
  const firstName = await screen.findByLabelText('Имя *');
  await user.clear(firstName);
  await user.type(firstName, 'Мария');
  await user.click(screen.getByRole('button', { name: 'Сохранить' }));
  expect(await screen.findByText('Проверьте имя')).toBeVisible();
  expect(firstName).toHaveValue('Мария');
  const avatar = screen.getByRole('button', { name: 'Меню профиля' });
  expect(avatar).toHaveTextContent('А');
  server.use(http.put('*/api/questionnaires/me', () => HttpResponse.error()));
  await user.click(screen.getByRole('button', { name: 'Сохранить' }));
  expect(await screen.findByRole('status')).not.toHaveTextContent(
    'Анкета сохранена.',
  );
  expect(avatar).toHaveTextContent('А');
});
it('shows loading errors with retry and denies guests', async () => {
  server.use(
    http.get(
      '*/api/questionnaires/me',
      () => new HttpResponse(null, { status: 403 }),
    ),
  );
  renderPage('/questionnaire', 0);
  const user = userEvent.setup();
  expect(
    await screen.findByRole('heading', { name: 'Не удалось загрузить анкету' }),
  ).toBeVisible();
  server.use(
    http.get('*/api/questionnaires/me', () =>
      HttpResponse.json(questionnaireFixture),
    ),
  );
  await user.click(screen.getByRole('button', { name: 'Повторить' }));
  expect(await screen.findByLabelText('Имя *')).toHaveValue('Анна');
});
it('redirects an unauthenticated questionnaire visitor to login', async () => {
  renderPage('/questionnaire');
  expect(await screen.findByRole('heading', { name: 'Вход' })).toBeVisible();
  expect(
    within(screen.getByRole('main')).queryByLabelText('Имя *'),
  ).not.toBeInTheDocument();
});

it('denies the curator role rather than rendering mentor controls', async () => {
  renderPage('/questionnaire', 2);
  expect(
    await screen.findByRole('heading', { name: 'Доступ запрещён' }),
  ).toBeVisible();
  expect(
    screen.queryByRole('button', { name: 'Сохранить' }),
  ).not.toBeInTheDocument();
});
