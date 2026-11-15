import { http, HttpResponse, delay } from 'msw';
import { expect, it, vi } from 'vitest';
import { server } from '@/test/mocks/server';
import { profiles, refreshTokens } from '@/test/fixtures/api';
import { SessionService, SESSION_KEY } from './index';
const seed = () =>
  localStorage.setItem(
    SESSION_KEY,
    JSON.stringify({
      refreshToken: refreshTokens[0],
      sessionId: profiles[0]!.id,
    }),
  );
it('restores a guest or resets invalid browser storage', async () => {
  expect(await new SessionService().restore()).toBeNull();
  localStorage.setItem(SESSION_KEY, 'broken');
  expect(await new SessionService().restore()).toBeNull();
  localStorage.setItem(SESSION_KEY, '{}');
  expect(await new SessionService().restore()).toBeNull();
  expect(localStorage.getItem(SESSION_KEY)).toBeNull();
});
it('keeps only refresh credentials and a nonsecret session marker across restart', async () => {
  const service = new SessionService();
  expect(await service.login('mentor@example.test', 'password')).toEqual(
    profiles[1],
  );
  expect(JSON.parse(localStorage.getItem(SESSION_KEY)!)).toEqual({
    refreshToken: refreshTokens[1],
    sessionId: expect.any(String),
  });
  expect(await new SessionService().restore()).toEqual(profiles[1]);
  service.logout();
  expect(localStorage.getItem(SESSION_KEY)).toBeNull();
});
it('coordinates concurrent refresh in one tab', async () => {
  seed();
  const service = new SessionService();
  let count = 0;
  server.events.on('request:start', ({ request }) => {
    if (request.url.includes('/api/auth/refresh')) count++;
  });
  await Promise.all([service.renew(), service.renew()]);
  expect(count).toBe(1);
  server.events.removeAllListeners();
});
it('clears rejected sessions but preserves credentials on connection/server errors', async () => {
  seed();
  server.use(
    http.put('*/api/auth/refresh', () =>
      HttpResponse.json({ message: 'Expired' }, { status: 401 }),
    ),
  );
  expect(await new SessionService().restore()).toBeNull();
  expect(localStorage.getItem(SESSION_KEY)).toBeNull();
  seed();
  server.use(
    http.put('*/api/auth/refresh', () =>
      HttpResponse.json({ message: 'Unavailable' }, { status: 503 }),
    ),
  );
  await expect(new SessionService().restore()).rejects.toThrow('Unavailable');
  expect(localStorage.getItem(SESSION_KEY)).not.toBeNull();
});
it('does not bring back credentials after logout during renewal', async () => {
  seed();
  const service = new SessionService();
  server.use(
    http.put('*/api/auth/refresh', async () => {
      await delay(30);
      return HttpResponse.json({
        data: { ...profiles[0], token: 'new', refreshToken: refreshTokens[0] },
      });
    }),
  );
  const renewal = service.renew();
  service.logout();
  await expect(renewal).rejects.toThrow();
  expect(localStorage.getItem(SESSION_KEY)).toBeNull();
});
it('reports unavailable persistence and unavailable cross-tab locking', async () => {
  const set = vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
    throw new Error('Denied');
  });
  await expect(
    new SessionService().login('menti@example.test', 'password'),
  ).rejects.toThrow('хранилище');
  set.mockRestore();
  seed();
  const locks = navigator.locks;
  Object.defineProperty(navigator, 'locks', {
    configurable: true,
    value: undefined,
  });
  try {
    await expect(new SessionService().restore()).rejects.toThrow(
      'между вкладками',
    );
  } finally {
    Object.defineProperty(navigator, 'locks', {
      configurable: true,
      value: locks,
    });
  }
});
it('refuses renewal without credentials and invokes cleanup on sign out', async () => {
  const ended = vi.fn();
  const service = new SessionService(ended);
  await expect(service.renew()).rejects.toMatchObject({ status: 401 });
  service.externalChange();
  service.logout();
  expect(ended).toHaveBeenCalled();
});

it('serializes different tabs and rereads the rotated token after obtaining the lock', async () => {
  seed();
  const original = navigator.locks;
  let tail: Promise<unknown> = Promise.resolve();
  Object.defineProperty(navigator, 'locks', {
    configurable: true,
    value: {
      request: (_name: string, callback: () => Promise<unknown>) => {
        const result = tail.then(callback);
        tail = result.catch(() => {});
        return result;
      },
    },
  });
  let current = refreshTokens[0]!;
  let count = 0;
  server.use(
    http.put('*/api/auth/refresh', async ({ request }) => {
      const body = (await request.json()) as { refreshToken: string };
      if (body.refreshToken !== current)
        return new HttpResponse(null, { status: 401 });
      await delay(10);
      count++;
      current = count === 1 ? refreshTokens[1]! : refreshTokens[0]!;
      return HttpResponse.json({
        data: { ...profiles[0], token: 'new-token', refreshToken: current },
      });
    }),
  );
  try {
    await Promise.all([
      new SessionService().renew(),
      new SessionService().renew(),
    ]);
    expect(count).toBe(2);
  } finally {
    Object.defineProperty(navigator, 'locks', {
      configurable: true,
      value: original,
    });
  }
});
