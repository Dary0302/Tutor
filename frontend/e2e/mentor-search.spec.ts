import { expect, test } from '@playwright/test';

test.use({ viewport: { width: 1920, height: 1080 } });

test('панель фильтров заканчивается у кнопки применения', async ({ page }) => {
  await page.goto('/login');
  await page.getByLabel('Email').fill('menti@example.test');
  await page.getByLabel('Пароль').fill('test-password');
  await page.getByRole('button', { name: 'Войти', exact: true }).click();
  await page.getByRole('link', { name: 'Подбор и поиск' }).click();

  const filters = page.getByRole('form', { name: 'Фильтры' });
  const apply = filters.getByRole('button', { name: 'Применить' });
  const mentorWithPlaces = page.getByRole('heading', {
    name: 'Анна Наставник 2',
  });
  await expect(apply).toBeVisible();
  await expect(mentorWithPlaces).toBeVisible();
  await expect(filters.getByText(/Длительность/)).toHaveCount(0);
  const places = filters.getByLabel('Есть свободные места');
  await expect(places.getByRole('option')).toHaveText(['Неважно', 'Да', 'Нет']);

  const monday = await filters.getByLabel('Понедельник').boundingBox();
  const thursday = await filters.getByLabel('Четверг').boundingBox();
  const friday = await filters.getByLabel('Пятница').boundingBox();
  const range = filters.getByRole('group', { name: 'Часов в неделю' });
  expect(monday?.y).toBe(thursday?.y);
  expect(friday?.x).toBe(monday?.x);
  expect(friday!.y).toBeGreaterThan(monday!.y);
  expect((await range.getByLabel('От').boundingBox())?.y).toBe(
    (await range.getByLabel('До').boundingBox())?.y,
  );

  const panel = await filters.boundingBox();
  const button = await apply.boundingBox();
  const results = await page
    .getByRole('region', { name: 'Результаты подбора' })
    .boundingBox();
  expect(panel?.width).toBe(280);
  expect(button?.height).toBe(44);
  expect(panel!.y + panel!.height - button!.y - button!.height).toBe(21);
  expect(results!.height).toBeGreaterThan(panel!.height);

  await places.selectOption('false');
  await expect(mentorWithPlaces).toBeVisible();
  await apply.click();
  await expect(page).toHaveURL(/places=false/);
  const mentor = page.getByRole('heading', { name: 'Мария Петрова' });
  await expect(mentor).toBeVisible();
  await expect(mentorWithPlaces).toHaveCount(0);
  await page.reload();
  await expect(places).toHaveValue('false');
  await expect(mentor).toBeVisible();
});
