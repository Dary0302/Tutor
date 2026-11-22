import { http, HttpResponse } from 'msw';
import { expect, it } from 'vitest';
import { server } from '@/test/mocks/server';
import {
  questionnaireFixture,
  mockSession,
  profiles,
} from '@/test/fixtures/api';
import { createApiClient } from './client';
import { ApiError, decodeResponse, profileSchema } from './runtime';
const base = 'http://localhost:5003';
it('uses canonical auth routes, validates identity and preserves registration without session', async () => {
  const api = createApiClient(base);
  await expect(
    api.register({ email: 'test@example.test', password: 'password', role: 0 }),
  ).resolves.toBeUndefined();
  expect(await api.login('test@example.test', 'password')).toMatchObject(
    profiles[0]!,
  );
  expect(await api.refresh(mockSession().refreshToken!)).toMatchObject(
    profiles[0]!,
  );
  const authenticated = createApiClient(base, {
    token: () => 'test-token-0',
    renew: async () => {},
  });
  expect(await authenticated.profile()).toEqual(profiles[0]);
});
it('normalizes string enums and accepts 204 without parsing JSON', async () => {
  const api = createApiClient(base);
  server.use(
    http.get('*/api/questionnaires/me', () =>
      HttpResponse.json(questionnaireFixture),
    ),
  );
  const form = await api.questionnaire();
  expect(form.communicationFormat).toBe(0);
  expect(form.skills[0]?.skillLevel).toBe(0);
  expect(form.availability[0]?.dayOfWeek).toBe(1);
  await expect(
    api.saveQuestionnaire({
      firstName: 'A',
      lastName: 'B',
      city: 'C',
      education: 'D',
      goalDescription: 'E',
      skills: [],
    }),
  ).resolves.toBeUndefined();
});
it('decodes auth and field errors and rejects malformed success payloads', async () => {
  expect(() =>
    decodeResponse(
      new Response(null, { status: 400 }),
      JSON.stringify({
        errors: { email: ['Ошибка'] },
        detail: 'Проверьте поля',
      }),
    ),
  ).toThrow('Проверьте поля');
  expect(() =>
    decodeResponse(new Response(null, { status: 401 }), {
      message: 'Неверный пароль',
    }),
  ).toThrow('Неверный пароль');
  expect(() =>
    decodeResponse(new Response(null, { status: 500 }), '<html>'),
  ).toThrow('некорректный');
  expect(() =>
    decodeResponse(new Response(null, { status: 500 }), null),
  ).toThrow(ApiError);
  expect(() => decodeResponse(new Response(null, { status: 400 }), {})).toThrow(
    'Не удалось',
  );
  expect(() =>
    profileSchema.parse({ ...profiles[0], role: 'Student' }),
  ).not.toThrow();
  server.use(
    http.post('*/api/auth/log-in', () =>
      HttpResponse.json({ data: { token: '' } }),
    ),
  );
  await expect(
    createApiClient(base).login('test@example.test', 'password'),
  ).rejects.toThrow();
});
it('retries an authenticated request once and never loops on a second 401', async () => {
  let calls = 0;
  let renewals = 0;
  server.use(
    http.get('*/api/auth/get-me', () => {
      calls++;
      return new HttpResponse(null, { status: 401 });
    }),
  );
  const api = createApiClient(base, {
    token: () => 'token',
    renew: async () => {
      renewals++;
    },
  });
  await expect(api.profile()).rejects.toMatchObject({ status: 401 });
  expect(calls).toBe(2);
  expect(renewals).toBe(1);
});
