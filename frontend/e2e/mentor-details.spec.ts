import { expect, test } from '@playwright/test';

test('search, compare and direct profile survive reload', async ({ page }) => {
  await page.goto('/login');
  await page.getByLabel('Email').fill('menti@example.test');
  await page.getByLabel('Пароль').fill('test-password');
  await page.getByRole('button', { name: 'Войти', exact: true }).click();
  await page.getByRole('link', { name: 'Подбор и поиск' }).click();
  await page
    .getByRole('link', { name: 'Сравнить', exact: true })
    .first()
    .click();
  await expect(page.getByRole('link', { name: 'Выбрать' })).toBeVisible();
  await page.reload();
  await expect(page.getByRole('table')).toContainText('От 3 до 6 месяцев');
  await page.getByRole('link', { name: 'Добавить наставника' }).click();
  await page
    .getByRole('link', { name: 'Сравнить', exact: true })
    .nth(1)
    .click();
  await expect(page.getByRole('link', { name: 'Убрать' })).toHaveCount(2);
  await page.getByRole('link', { name: 'Выбрать' }).first().click();
  await expect(
    page.getByRole('heading', { name: 'К каким целям поможет прийти' }),
  ).toBeVisible();
  await page.reload();
  await expect(
    page.getByRole('heading', { name: 'Мария Петрова', level: 1 }),
  ).toBeVisible();
  await expect(
    page.getByRole('button', { name: 'Отправить заявку' }),
  ).toBeDisabled();
});
