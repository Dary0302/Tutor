import { http, HttpResponse } from 'msw';
import {
  mentorFixture,
  mockSession,
  profiles,
  refreshTokens,
} from '../fixtures/api';
const registrations = new Map<string, number>();
const questionnaires = new Map<string, string>();
const persistentMockKey = (authorization: string) =>
  `kurator.mock.questionnaire:${authorization}`;
const browserMock = import.meta.env.DEV && !import.meta.env.TEST;
export function resetMockState() {
  registrations.clear();
  questionnaires.clear();
}
export const handlers = [
  http.get('*/api/mentors', ({ request }) => {
    if (!request.headers.has('Authorization'))
      return new HttpResponse(null, { status: 401 });
    const params = new URL(request.url).searchParams;
    const page = Number(params.get('Page') ?? '1');
    const pageSize = Number(params.get('PageSize') ?? '10');
    const places = params.get('HasAvailablePlaces');
    const mentors = Array.from({ length: 12 }, (_, index) => ({
      ...mentorFixture,
      userId: `22222222-2222-4222-8222-${String(index).padStart(12, '0')}`,
      questionnaireId: `33333333-3333-4333-8333-${String(index).padStart(12, '0')}`,
      firstName: index === 0 ? 'Мария' : 'Анна',
      lastName: index === 0 ? 'Петрова' : `Наставник ${index + 1}`,
      availablePlaces: index === 0 ? 0 : 2,
    })).filter(
      ({ availablePlaces }) =>
        places === null || availablePlaces > 0 === (places === 'true'),
    );
    return HttpResponse.json({
      items: mentors.slice((page - 1) * pageSize, page * pageSize),
      totalCount: mentors.length,
      page,
      pageSize,
      totalPages: Math.ceil(mentors.length / pageSize),
    });
  }),
  http.post('*/api/auth/register', async ({ request }) => {
    const body = (await request.json()) as { email: string; role: number };
    registrations.set(body.email, body.role);
    return HttpResponse.json(
      { message: 'User created', status: 201, data: null },
      { status: 201 },
    );
  }),
  http.post('*/api/auth/log-in', async ({ request }) => {
    const body = (await request.json()) as { email: string };
    const role =
      registrations.get(body.email) ?? (body.email.includes('mentor') ? 1 : 0);
    return HttpResponse.json({
      message: 'User authenticated',
      status: 200,
      data: mockSession(role),
    });
  }),
  http.put('*/api/auth/refresh', async ({ request }) => {
    const body = (await request.json()) as { refreshToken: string };
    const role = refreshTokens.indexOf(body.refreshToken);
    return role < 0
      ? HttpResponse.json({ message: 'Invalid refresh token' }, { status: 401 })
      : HttpResponse.json({
          message: 'Token refreshed',
          status: 200,
          data: mockSession(role),
        });
  }),
  http.get('*/api/auth/get-me', ({ request }) => {
    const role = request.headers.get('Authorization')?.endsWith('1') ? 1 : 0;
    return request.headers.has('Authorization')
      ? HttpResponse.json(profiles[role])
      : new HttpResponse(null, { status: 401 });
  }),
  http.get('*/api/questionnaires/me', ({ request }) => {
    const key = request.headers.get('Authorization') ?? '';
    const saved = browserMock
      ? sessionStorage.getItem(persistentMockKey(key))
      : questionnaires.get(key);
    return saved
      ? new HttpResponse(saved, {
          headers: { 'Content-Type': 'application/json' },
        })
      : HttpResponse.json({ detail: 'Анкета не найдена.' }, { status: 404 });
  }),
  http.put('*/api/questionnaires/me', async ({ request }) => {
    const key = request.headers.get('Authorization') ?? '';
    const saved = JSON.stringify(await request.json());
    if (browserMock) sessionStorage.setItem(persistentMockKey(key), saved);
    else questionnaires.set(key, saved);
    return new HttpResponse(null, { status: 204 });
  }),
];
