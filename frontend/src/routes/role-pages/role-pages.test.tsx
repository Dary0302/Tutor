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

it('shows the curator queue, opens every detail layer and preserves role restrictions', async () => {
  const user = userEvent.setup();
  renderPage('/approval-queue', 2);
  expect(
    await screen.findByRole('heading', { name: 'Очередь на утверждение' }),
  ).toBeVisible();
  const triggers = screen.getAllByRole('button', { name: /Подробнее/ });
  expect(triggers).toHaveLength(4);
  for (const [index, name] of [
    'Екатерина Смирнова',
    'Александр Иванов',
    'Павел Андреев',
    'Мария Петрова',
  ].entries()) {
    await user.click(triggers[index]!);
    const dialog = screen.getByRole('dialog');
    expect(within(dialog).getByRole('heading', { name })).toBeVisible();
    await user.click(
      within(dialog).getByRole('button', { name: 'Закрыть окно' }),
    );
    expect(triggers[index]).toHaveFocus();
  }
  expect(
    screen.getAllByRole('button', { name: 'Утвердить' })[0],
  ).toBeDisabled();
  expect(
    screen.queryByRole('link', { name: 'Моя анкета' }),
  ).not.toBeInTheDocument();
});

it('shows the empty approval state and prevents non-curator access', async () => {
  const page = renderPage('/approval-queue?state=empty', 2);
  expect(
    await screen.findByRole('heading', { name: 'Нет пар на утверждение' }),
  ).toBeVisible();
  page.unmount();
  renderPage('/approval-queue', 0);
  expect(
    await screen.findByRole('heading', { name: 'Доступ запрещён' }),
  ).toBeVisible();
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
