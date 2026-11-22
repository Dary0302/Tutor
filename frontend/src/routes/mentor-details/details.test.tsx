import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { expect, it } from 'vitest';
import { renderPage } from '@/test/render';
import { mentorFixture, mentorQuestionnaireFixture } from '@/test/fixtures/api';
import { server } from '@/test/mocks/server';

const id = mentorFixture.userId;

it('loads a direct profile with live questionnaire fields and no reviews', async () => {
  renderPage(`/mentors/${id}`, 0);
  expect(
    await screen.findByRole('heading', { name: 'Мария Петрова' }),
  ).toBeVisible();
  expect(
    screen.getByRole('heading', { name: 'К каким целям поможет прийти' }),
  ).toBeVisible();
  expect(screen.getByText('Изучить frontend')).toBeVisible();
  expect(screen.getByText('От 3 до 6 месяцев')).toBeVisible();
  expect(screen.getByText('Пн 18:00–19:00')).toBeVisible();
  expect(screen.queryByText(/Отзывы/)).not.toBeInTheDocument();
  expect(
    screen.getByRole('button', { name: 'Отправить заявку' }),
  ).toBeEnabled();
});

it('compares URL selections, removes a column and opens a profile', async () => {
  const user = userEvent.setup();
  const second = '22222222-2222-4222-8222-222222222223';
  renderPage(`/compare?mentor=${id}&mentor=${second}&mentor=${id}`, 0);
  expect(
    (await screen.findAllByRole('link', { name: 'Выбрать' }))[0],
  ).toBeVisible();
  const table = screen.getByRole('table');
  expect(await within(table).findAllByText('От 3 до 6 месяцев')).toHaveLength(
    2,
  );
  expect(within(table).queryByText('Рейтинг')).not.toBeInTheDocument();
  expect(within(table).queryByText('Совместимость')).not.toBeInTheDocument();
  await user.click(screen.getAllByRole('link', { name: 'Убрать' })[1]!);
  expect(screen.getAllByRole('link', { name: 'Убрать' })).toHaveLength(1);
  await user.click(screen.getByRole('link', { name: 'Выбрать' }));
  expect(
    await screen.findByRole('heading', { name: 'Мария Петрова', level: 1 }),
  ).toBeVisible();
});

it('offers an empty comparison and rejects an invalid mentor ID without a request', async () => {
  const page = renderPage('/compare', 0);
  expect(
    await screen.findByText(
      'Выберите наставников для сравнения в подборе и поиске.',
    ),
  ).toBeVisible();
  page.unmount();
  renderPage('/mentors/invalid', 0);
  expect(await screen.findByRole('alert')).toHaveTextContent(
    'Наставник не найден.',
  );
});

it('retries failed profile requests and handles a non-mentor questionnaire', async () => {
  let fails = true;
  server.use(
    http.get('*/api/comparison', () =>
      fails
        ? new HttpResponse(null, { status: 503 })
        : HttpResponse.json({
            ...mentorQuestionnaireFixture,
            teacherInfo: null,
          }),
    ),
  );
  renderPage(`/mentors/${id}`, 0);
  expect(
    await screen.findByRole('alert', {}, { timeout: 4000 }),
  ).toHaveTextContent('Не удалось загрузить');
  fails = false;
  await userEvent
    .setup()
    .click(screen.getByRole('button', { name: 'Повторить' }));
  expect(await screen.findByRole('alert')).toHaveTextContent(
    'Наставник не найден.',
  );
});

it.each([undefined, 2])(
  'restricts profile access for role %s',
  async (role) => {
    renderPage(`/mentors/${id}`, role);
    expect(
      await screen.findByRole('heading', {
        name: role === undefined ? 'Вход' : 'Доступ запрещён',
      }),
    ).toBeVisible();
  },
);
