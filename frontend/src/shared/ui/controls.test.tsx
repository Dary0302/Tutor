import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { expect, it, vi } from 'vitest';
import { Button, Card, Input, Select } from './index';

it('operates named controls with the keyboard and blocks disabled actions', async () => {
  const user = userEvent.setup();
  const click = vi.fn();
  render(
    <>
      <Input label="Имя" />
      <Select label="Формат">
        <option>Онлайн</option>
        <option>Очно</option>
      </Select>
      <Button onClick={click}>Сохранить</Button>
      <Button variant="secondary" disabled onClick={click}>
        Недоступно
      </Button>
    </>,
  );
  await user.tab();
  expect(screen.getByRole('textbox', { name: 'Имя' })).toHaveFocus();
  await user.keyboard('Анна');
  expect(screen.getByRole('textbox')).toHaveValue('Анна');
  await user.selectOptions(screen.getByRole('combobox'), 'Очно');
  expect(screen.getByRole('combobox')).toHaveValue('Очно');
  await user.click(screen.getByRole('button', { name: 'Сохранить' }));
  await user.click(screen.getByRole('button', { name: 'Недоступно' }));
  expect(click).toHaveBeenCalledTimes(1);
});

it('connects field errors to their labels and controls', () => {
  render(
    <>
      <Input id="email" label="Email" error="Некорректный email" />
      <Select id="duration" label="Срок" error="Укажите срок">
        <option>—</option>
      </Select>
    </>,
  );
  expect(
    screen.getByRole('textbox', { name: 'Email' }),
  ).toHaveAccessibleDescription('Некорректный email');
  expect(
    screen.getByRole('combobox', { name: 'Срок' }),
  ).toHaveAccessibleDescription('Укажите срок');
  expect(screen.getByRole('textbox')).toHaveAttribute('aria-invalid', 'true');
});

it('names a content card through its live heading', () => {
  render(
    <Card title="Гибридная модель">
      <p>Решение куратора</p>
    </Card>,
  );
  expect(
    screen.getByRole('article', { name: 'Гибридная модель' }),
  ).toHaveTextContent('Решение куратора');
});
