import createClient from 'openapi-fetch';
import { config } from '@/shared/config';
import type { paths } from './generated/schema';
import type { components } from './generated/schema';
import {
  decodeResponse,
  profileSchema,
  sessionSchema,
  questionnaireSchema,
  mentorSearchResultSchema,
} from './runtime';
import { z } from 'zod';

export type MentorSearchQuery = NonNullable<
  paths['/api/mentors']['get']['parameters']['query']
>;

// Paths are generated from the imported Tutor.Api contract; never edit them manually.
export function createApiClient(
  baseUrl = config.apiBaseUrl || globalThis.location.origin,
  authorization?: { token: () => string | null; renew: () => Promise<void> },
) {
  const client = createClient<paths>({
    baseUrl,
    fetch: async (request: Request) => {
      const backup = request.clone();
      const token = authorization?.token();
      if (token) request.headers.set('Authorization', `Bearer ${token}`);
      const response = await globalThis.fetch(request);
      if (response.status !== 401 || !authorization) return response;
      await authorization.renew();
      const fresh = authorization.token();
      if (fresh) backup.headers.set('Authorization', `Bearer ${fresh}`);
      return globalThis.fetch(backup);
    },
  });
  return {
    async register(body: components['schemas']['RegisterRequest']) {
      const r = await client.POST('/api/auth/register', {
        body,
        parseAs: 'text',
      });
      decodeResponse(r.response, r.data ?? r.error);
    },
    async login(email: string, password: string) {
      const r = await client.POST('/api/auth/log-in', {
        body: { email, password },
        parseAs: 'text',
      });
      return z
        .object({ data: sessionSchema })
        .parse(decodeResponse(r.response, r.data ?? r.error)).data;
    },
    async refresh(refreshToken: string) {
      const r = await client.PUT('/api/auth/refresh', {
        body: { refreshToken },
        parseAs: 'text',
      });
      return z
        .object({ data: sessionSchema })
        .parse(decodeResponse(r.response, r.data ?? r.error)).data;
    },
    async profile() {
      const r = await client.GET('/api/auth/get-me', { parseAs: 'text' });
      return profileSchema.parse(decodeResponse(r.response, r.data ?? r.error));
    },
    async questionnaire() {
      const r = await client.GET('/api/questionnaires/me', { parseAs: 'text' });
      return questionnaireSchema.parse(
        decodeResponse(r.response, r.data ?? r.error),
      );
    },
    async searchMentors(query: MentorSearchQuery, signal?: AbortSignal) {
      const r = await client.GET('/api/mentors', {
        params: { query },
        querySerializer: { array: { style: 'form', explode: true } },
        ...(signal && { signal }),
        parseAs: 'text',
      });
      return mentorSearchResultSchema.parse(
        decodeResponse(r.response, r.data ?? r.error),
      );
    },
    async saveQuestionnaire(
      body: components['schemas']['QuestionnaireFormDto'],
    ) {
      const r = await client.PUT('/api/questionnaires/me', {
        body,
        parseAs: 'text',
      });
      decodeResponse(r.response, r.data ?? r.error);
    },
  };
}
export const api = createApiClient();
