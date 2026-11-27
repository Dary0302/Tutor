import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { expect, it } from 'vitest';
import { renderPage } from '@/test/render';

it.each([
  [0, '240 баллов'],
  [1, '580 баллов'],
] as const)('shows rewards for role %s', async (role, total) => {
  renderPage('/rewards', role);
  expect(await screen.findByRole('heading', { name: 'Награды' })).toBeVisible();
  expect(
    within(screen.getByRole('main')).getAllByText(total).length,
  ).toBeGreaterThan(0);
});

it('switches mentee goals to the supplied empty mentor state', async () => {
  renderPage('/goals', 0);
  const user = userEvent.setup();
  await user.selectOptions(await screen.findByLabelText('Наставник'), 'maria');
  expect(
    await screen.findByRole('heading', { name: 'Пока нет целей' }),
  ).toBeVisible();
  await user.selectOptions(screen.getByLabelText('Наставник'), 'alexander');
  expect(
    await screen.findByRole('heading', { name: 'Сверстать дашборд' }),
  ).toBeVisible();
});
