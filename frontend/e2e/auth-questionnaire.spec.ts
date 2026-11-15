import { expect, test } from '@playwright/test';

test.use({ viewport: { width: 1920, height: 1080 } });

for (const role of ['Менти', 'Наставник']) {
  test(`регистрация, вход, анкета и восстановление сессии: ${role}`, async ({
    page,
  }) => {
    const email =
      role === 'Менти' ? 'menti@example.test' : 'mentor@example.test';
    await page.goto('/register');
    await page.getByLabel(role, { exact: true }).check();
    await page.getByRole('textbox', { name: 'Email' }).fill(email);
    await page.getByLabel('Пароль', { exact: true }).fill('test-password');
    await page.getByLabel('Принимаю пользовательское соглашение').check();
    await page.getByRole('button', { name: 'Создать аккаунт' }).click();
    await expect(page.getByRole('heading', { name: 'Вход' })).toBeVisible();
    await page.getByRole('textbox', { name: 'Email' }).fill(email);
    await page.getByLabel('Пароль', { exact: true }).fill('test-password');
    await page.getByRole('button', { name: 'Войти', exact: true }).click();
    await expect(
      page.getByRole('heading', {
        name: role === 'Менти' ? 'Анкета менти' : 'Анкета наставника',
      }),
    ).toBeVisible();
    const profile = page.getByRole('button', { name: 'Меню профиля' });
    await expect(profile.locator('svg')).toBeVisible();
    if (role === 'Менти') {
      const first = page.getByLabel('Имя *', { exact: true });
      const last = page.getByLabel('Фамилия *', { exact: true });
      await last.fill('Иванова');
      await page
        .getByRole('button', { name: 'Сохранить', exact: true })
        .click();
      await expect(first).toHaveAccessibleDescription('Обязательное поле');
      expect((await first.boundingBox())?.y).toBe(
        (await last.boundingBox())?.y,
      );
    }
    await page.getByLabel('Имя *', { exact: true }).fill('Анна');
    await page.getByLabel('Фамилия *', { exact: true }).fill('Иванова');
    await page.getByLabel('Город / часовой пояс *').fill('Москва, UTC+3');
    await page
      .getByLabel(
        role === 'Менти'
          ? 'Вуз, курс, специальность *'
          : 'Образование: вуз, специальность *',
      )
      .fill('МГУ, информатика');
    await page
      .getByLabel(
        role === 'Менти'
          ? 'Опишите цели (срок) *'
          : 'К каким целям могут прийти ваши ученики? *',
      )
      .fill('Изучить TypeScript');
    await page.getByPlaceholder('Введите название навыка').fill('TypeScript');
    await page.getByRole('button', { name: 'Добавить', exact: true }).click();
    await page.getByRole('button', { name: 'TypeScript · Начальный' }).click();
    const skillDialog = page.getByRole('dialog', { name: 'Уровень навыка' });
    await skillDialog.getByLabel('Уровень').selectOption('1');
    await skillDialog.getByRole('button', { name: 'Сохранить' }).click();
    const skillTag = page.getByRole('button', { name: 'TypeScript · Средний' });
    await expect(skillTag).toBeFocused();
    await page.keyboard.press('Enter');
    await skillDialog.getByLabel('Уровень').selectOption('3');
    await page.keyboard.press('Escape');
    await expect(skillDialog).not.toBeVisible();
    await expect(skillTag).toBeFocused();
    const duration = page.getByRole('button', { name: /Срок/ });
    await duration.click();
    const durationDialog = page.getByRole('dialog', { name: 'Длительность' });
    await durationDialog.getByLabel('Срок от (мес.)').fill('2');
    await durationDialog.getByLabel('Срок до (мес.)').fill('5');
    await durationDialog.getByRole('button', { name: 'Сохранить' }).click();
    await expect(duration).toHaveText(/От 2 до 5/);
    const monday = page.getByRole('button', { name: /^Пн/ });
    await monday.click();
    const timeDialog = page.getByRole('dialog', { name: 'Понедельник' });
    await timeDialog.getByRole('button', { name: /Добавить окно/ }).click();
    await timeDialog.getByRole('button', { name: 'Закрыть окно' }).click();
    await expect(monday).toHaveText(/\+/);
    await monday.click();
    await timeDialog.getByRole('button', { name: /Добавить окно/ }).click();
    await timeDialog.getByRole('button', { name: 'Сохранить' }).click();
    await expect(monday).toHaveText(/18:00–20:00/);
    if (role === 'Наставник') {
      await page.getByLabel('Место работы *').fill('Компания');
      await page.getByLabel('Должность *').fill('Разработчик');
    }
    await page.getByRole('button', { name: 'Сохранить', exact: true }).click();
    await expect(page.getByRole('status')).toHaveText('Анкета сохранена.');
    await expect(profile).toHaveText('А');
    await page.reload();
    await expect(page.getByRole('link', { name: 'Моя анкета' })).toBeVisible();
    await expect(profile).toHaveText('А');
    await expect(
      page.getByRole('button', { name: 'TypeScript · Средний' }),
    ).toBeVisible();
    await page.getByRole('link', { name: 'Награды' }).click();
    await expect(page.getByRole('heading', { name: 'Награды' })).toBeVisible();
    await expect(profile).toHaveText('А');
    await page.getByRole('button', { name: 'Меню профиля' }).click();
    await page.getByRole('button', { name: 'Выйти из аккаунта' }).click();
    await expect(page.getByRole('heading', { name: 'Вход' })).toBeVisible();
  });
}
