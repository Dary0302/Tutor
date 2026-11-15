import { expect, test } from '@playwright/test';
test('@smoke приложение открывается', async ({ page }) => {
  await page.goto('/');
  await expect(
    page.getByRole('heading', { name: 'Ищешь наставника или ученика?' }),
  ).toBeVisible();
  await expect(page.getByText('Свежие статьи')).toBeVisible();
});
test('@smoke неизвестный адрес и возврат домой', async ({ page }) => {
  await page.goto('/unknown');
  await expect(
    page.getByRole('heading', { name: 'Страница не найдена' }),
  ).toBeVisible();
  await page.getByRole('link', { name: 'На главную' }).click();
  await expect(page.getByRole('heading', { level: 1 })).toHaveText(
    'Ищешь наставника или ученика?',
  );
});
