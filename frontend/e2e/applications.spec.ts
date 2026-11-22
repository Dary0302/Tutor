import { expect, test } from '@playwright/test';

test.use({ viewport: { width: 1920, height: 1080 } });

test('application lifecycle and confirmation dialogs across three roles', async ({
  page,
}) => {
  async function login(email: string) {
    await page.goto('/login');
    await page.getByLabel('Email').fill(email);
    await page.getByLabel('Пароль').fill('test-password');
    await page.getByRole('button', { name: 'Войти', exact: true }).click();
    await expect(
      page.getByRole('button', { name: 'Меню профиля' }),
    ).toBeVisible();
  }
  async function send() {
    await page.goto('/mentors/22222222-2222-4222-8222-222222222222');
    await expect(page.getByLabel('Сопроводительное сообщение')).toBeDisabled();
    await page.getByRole('button', { name: 'Отправить заявку' }).click();
    await expect(page.getByRole('status')).toContainText('Заявка отправлена.');
  }
  await login('menti@example.test');
  await send();
  await page
    .getByRole('navigation', { name: 'Основная навигация' })
    .getByRole('link', { name: 'Мои заявки' })
    .click();
  await expect(page.getByRole('heading', { name: 'Мои заявки' })).toBeVisible();
  const geometry = await page
    .locator('.site-header__inner')
    .evaluate((header) => {
      const rect = header.getBoundingClientRect();
      const account = header
        .querySelector('.site-header__account')!
        .getBoundingClientRect();
      const nav = header.querySelector('nav')!.getBoundingClientRect();
      return {
        headerHeight: rect.height,
        headerRight: rect.right,
        accountRight: account.right,
        navRight: nav.right,
        accountLeft: account.left,
        overflow: document.documentElement.scrollWidth > innerWidth,
      };
    });
  expect(geometry.headerHeight).toBe(92);
  expect(geometry.accountRight).toBeLessThanOrEqual(geometry.headerRight);
  expect(geometry.navRight).toBeLessThanOrEqual(geometry.accountLeft);
  expect(geometry.overflow).toBe(false);
  await page.getByRole('button', { name: 'Отменить заявку' }).click();
  const cancel = page.getByRole('dialog', { name: 'Отменить заявку?' });
  await expect(cancel).toBeVisible();
  expect((await cancel.boundingBox())?.width).toBe(467);
  await cancel.getByRole('button', { name: 'Не отменять' }).click();
  await page.getByRole('button', { name: 'Отменить заявку' }).click();
  await cancel.getByRole('button', { name: 'Да, отменить заявку' }).click();
  await expect(page.getByText('Заявок пока нет.')).toBeVisible();

  await send();
  await login('mentor@example.test');
  await page.goto('/mentor');
  await expect(page.getByRole('button', { name: 'Уточнить' })).toHaveCount(0);
  await page.getByRole('button', { name: 'Принять', exact: true }).click();
  await expect(page.getByText('Заявок пока нет.')).toBeVisible();
  await login('curator@example.test');
  await page
    .getByRole('link', { name: 'Очередь на утверждение', exact: true })
    .first()
    .click();
  await page.getByRole('button', { name: /Подробнее.*Анна Иванова/ }).click();
  await expect(
    page.getByRole('dialog', { name: 'Анкета менти' }),
  ).toContainText('Изучить frontend');
  await page.keyboard.press('Escape');
  await page.getByRole('button', { name: 'Отклонить', exact: true }).click();
  const refusal = page.getByRole('dialog', { name: 'Отклонение заявки' });
  await expect(refusal).toBeVisible();
  expect((await refusal.boundingBox())?.width).toBe(472);
  await refusal.getByRole('button', { name: 'Не отклонять' }).click();
  await page.getByRole('button', { name: 'Утвердить' }).click();
  await expect(page.getByText('Заявок пока нет.')).toBeVisible();
});
