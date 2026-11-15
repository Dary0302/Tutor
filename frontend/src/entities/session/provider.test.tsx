import { act, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { expect, it } from 'vitest';
import { renderPage } from '@/test/render';
import { profiles, refreshTokens } from '@/test/fixtures/api';
import { server } from '@/test/mocks/server';
import { SESSION_KEY } from './index';

it('propagates cross-tab logout and ignores routine rotation of the same session', async () => {
  const marker = profiles[0]!.id;
  const oldValue = JSON.stringify({
    refreshToken: refreshTokens[0],
    sessionId: marker,
  });
  localStorage.setItem(SESSION_KEY, oldValue);
  renderPage('/questionnaire', 0);
  expect(
    await screen.findByRole('heading', { name: 'Анкета менти' }),
  ).toBeVisible();
  act(() =>
    window.dispatchEvent(
      new StorageEvent('storage', {
        key: SESSION_KEY,
        oldValue,
        newValue: oldValue,
      }),
    ),
  );
  expect(screen.getByRole('link', { name: 'Моя анкета' })).toBeVisible();
  act(() =>
    window.dispatchEvent(
      new StorageEvent('storage', { key: 'unrelated', newValue: null }),
    ),
  );
  act(() => {
    localStorage.clear();
    window.dispatchEvent(
      new StorageEvent('storage', {
        key: SESSION_KEY,
        oldValue,
        newValue: null,
      }),
    );
  });
  expect(await screen.findByRole('heading', { name: 'Вход' })).toBeVisible();
});
it('switches role and clears private data after another tab logs into a different account', async () => {
  const { client } = renderPage('/questionnaire', 0);
  expect(
    await screen.findByRole('heading', { name: 'Анкета менти' }),
  ).toBeVisible();
  const value = JSON.stringify({
    refreshToken: refreshTokens[1],
    sessionId: profiles[1]!.id,
  });
  act(() => {
    localStorage.setItem(SESSION_KEY, value);
    window.dispatchEvent(
      new StorageEvent('storage', {
        key: SESSION_KEY,
        oldValue: null,
        newValue: value,
      }),
    );
  });
  expect(
    await screen.findByRole('heading', { name: 'Анкета наставника' }),
  ).toBeVisible();
  expect(
    client.getQueryData(['questionnaire', profiles[0]!.id]),
  ).toBeUndefined();
});
it('offers retry for failed session restoration without deleting valid credentials', async () => {
  localStorage.setItem(
    SESSION_KEY,
    JSON.stringify({
      refreshToken: refreshTokens[0],
      sessionId: profiles[0]!.id,
    }),
  );
  server.use(
    http.put('*/api/auth/refresh', () =>
      HttpResponse.json({ message: 'Temporary outage' }, { status: 503 }),
    ),
  );
  renderPage('/');
  const user = userEvent.setup();
  expect(
    await screen.findByRole('heading', {
      name: 'Не удалось восстановить вход',
    }),
  ).toBeVisible();
  expect(localStorage.getItem(SESSION_KEY)).not.toBeNull();
  await user.click(screen.getByRole('button', { name: 'Повторить' }));
  expect(await screen.findByRole('alert')).toHaveTextContent(
    'Temporary outage',
  );
  await user.click(screen.getByRole('button', { name: 'Войти заново' }));
  expect(
    await screen.findByRole('link', { name: 'Регистрация' }),
  ).toBeVisible();
});
