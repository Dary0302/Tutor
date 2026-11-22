import { screen, within } from '@testing-library/react';
import { expect, it } from 'vitest';
import { renderPage } from '@/test/render';

it('shows the existing mentor profile preview by its direct address', async () => {
  renderPage('/mentors/alexey', 0);
  expect(
    await screen.findByRole('heading', { name: 'Алексей Морозов', level: 1 }),
  ).toBeVisible();
  expect(
    screen.getByRole('button', { name: 'Отправить заявку' }),
  ).toBeDisabled();
});
it('shows comparison criteria without claiming a live comparison operation', async () => {
  renderPage('/compare', 0);
  expect(
    await screen.findByRole('heading', { name: 'Сравнение наставников' }),
  ).toBeVisible();
  expect(screen.getByRole('table')).toHaveTextContent('Свободные слоты');
  expect(screen.getAllByRole('button', { name: 'Убрать' })).toHaveLength(3);
});
it.each(['/mentor', '/mentor?session=none'])(
  'shows mentor dashboard variant %s with unavailable mutations',
  async (path) => {
    renderPage(path, 1);
    expect(
      await screen.findByRole('heading', { name: 'Главная наставника' }),
    ).toBeVisible();
    expect(
      screen.getAllByRole('button', { name: 'Принять' })[0],
    ).toBeDisabled();
    expect(
      !!screen.queryByRole('heading', { name: 'Предстоящая сессия' }),
    ).toBe(!path.includes('none'));
  },
);
it.each([0, 1])('shows role %s calendar with fixture events', async (role) => {
  renderPage('/calendar', role);
  expect(
    await screen.findByRole('heading', { name: 'Календарь сессий' }),
  ).toBeVisible();
  expect(screen.getByRole('table')).toHaveTextContent(
    role ? 'Екатерина Смирнова' : 'Александр Иванов',
  );
  expect(!!screen.queryByRole('button', { name: 'Запланировать' })).toBe(
    role === 1,
  );
});
it.each([0, 1])('shows role %s goals without task mutations', async (role) => {
  renderPage('/goals', role);
  expect(
    await screen.findByRole('heading', { name: 'Цели и прогресс' }),
  ).toBeVisible();
  expect(screen.getAllByRole('progressbar')).toHaveLength(2);
  expect(
    screen.getByRole('checkbox', { name: 'Создать структуру проекта' }),
  ).toBeDisabled();
  if (role)
    expect(screen.getByRole('button', { name: 'Создать цель' })).toBeDisabled();
  const nav = screen.getByRole('navigation');
  expect(
    within(nav).queryByRole('link', { name: 'Награды' }),
  ).toBeInTheDocument();
});
it('redirects unauthenticated preview access', async () => {
  renderPage('/compare');
  expect(await screen.findByRole('heading', { name: 'Вход' })).toBeVisible();
});
