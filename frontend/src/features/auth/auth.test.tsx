import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse, delay } from 'msw';
import { expect, it } from 'vitest';
import { renderPage } from '@/test/render';
import { server } from '@/test/mocks/server';
const fill = async () => {
  const user = userEvent.setup();
  await user.type(
    await screen.findByRole('textbox', { name: 'Email' }),
    'mentor@example.test',
  );
  await user.type(screen.getByLabelText('Пароль'), 'password');
  return user;
};
it('registers a mentor and returns to login without authenticating', async () => {
  renderPage('/register');
  const user = await fill();
  await user.click(screen.getByLabelText('Наставник'));
  await user.click(
    screen.getByLabelText('Принимаю пользовательское соглашение'),
  );
  await user.click(screen.getByRole('button', { name: 'Создать аккаунт' }));
  expect(
    await screen.findByText('Аккаунт создан. Войдите с вашим email и паролем.'),
  ).toBeVisible();
  expect(screen.getByRole('heading', { name: 'Вход' })).toBeVisible();
});
it('requires consent and shows registration conflicts without clearing input', async () => {
  renderPage('/register');
  const user = await fill();
  await user.click(screen.getByRole('button', { name: 'Создать аккаунт' }));
  expect(
    await screen.findByText('Подтвердите принятие соглашения.'),
  ).toBeVisible();
  server.use(
    http.post('*/api/auth/register', () =>
      HttpResponse.json({ message: 'Email already in use' }, { status: 409 }),
    ),
  );
  await user.click(
    screen.getByLabelText('Принимаю пользовательское соглашение'),
  );
  await user.click(screen.getByRole('button', { name: 'Создать аккаунт' }));
  expect(await screen.findByRole('alert')).toHaveTextContent(
    'Email already in use',
  );
  expect(screen.getByRole('textbox', { name: 'Email' })).toHaveValue(
    'mentor@example.test',
  );
});
it('validates input and reports invalid login', async () => {
  renderPage('/login');
  const user = userEvent.setup();
  await user.click(await screen.findByRole('button', { name: 'Войти' }));
  expect(await screen.findByText('Введите корректный email')).toBeVisible();
  await fill();
  server.use(
    http.post('*/api/auth/log-in', () =>
      HttpResponse.json({ message: 'Invalid credentials' }, { status: 401 }),
    ),
  );
  await user.click(screen.getByRole('button', { name: 'Войти' }));
  expect(await screen.findByRole('alert')).toHaveTextContent(
    'Invalid credentials',
  );
});
it('signs in and opens the current role questionnaire', async () => {
  renderPage('/login');
  const user = await fill();
  await user.click(screen.getByRole('button', { name: 'Войти' }));
  expect(
    await screen.findByRole('heading', { name: 'Анкета наставника' }),
  ).toBeVisible();
  expect(screen.getByRole('navigation')).not.toHaveTextContent('FAQ');
  expect(
    screen.queryByRole('link', { name: 'Регистрация' }),
  ).not.toBeInTheDocument();
  await user.click(screen.getByRole('button', { name: 'Меню профиля' }));
  await user.click(screen.getByRole('button', { name: 'Выйти из аккаунта' }));
  expect(await screen.findByRole('heading', { name: 'Вход' })).toBeVisible();
});

it('prevents duplicate login submissions while the backend is pending', async () => {
  server.use(
    http.post('*/api/auth/log-in', async () => {
      await delay(100);
      return HttpResponse.json({ message: 'Rejected' }, { status: 401 });
    }),
  );
  renderPage('/login');
  const user = await fill();
  await user.click(screen.getByRole('button', { name: 'Войти' }));
  expect(screen.getByRole('button', { name: 'Подождите…' })).toBeDisabled();
  expect(await screen.findByRole('alert')).toHaveTextContent('Rejected');
});
