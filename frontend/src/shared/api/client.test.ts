import { http, HttpResponse } from 'msw';
import { expect, it } from 'vitest';
import createClient from 'openapi-fetch';
import { server } from '@/test/mocks/server';

// A test-only transport contract, never a product endpoint or production handler.
interface TransportFixture {
  '/__test__/transport': {
    get: {
      responses: {
        200: { content: { 'application/json': { status: string } } };
        503: { content: { 'application/json': { message: string } } };
      };
    };
  };
}
const createFixtureClient = () =>
  createClient<TransportFixture>({ baseUrl: 'http://localhost:5000' });
it('decodes successful responses at the HTTP boundary', async () => {
  server.use(
    http.get('http://localhost:5000/__test__/transport', () =>
      HttpResponse.json({ status: 'ready' }),
    ),
  );
  const result = await createFixtureClient().GET('/__test__/transport');
  expect(result.data).toEqual({ status: 'ready' });
  expect(result.error).toBeUndefined();
});
it('preserves HTTP error responses for the caller', async () => {
  server.use(
    http.get('http://localhost:5000/__test__/transport', () =>
      HttpResponse.json({ message: 'Unavailable' }, { status: 503 }),
    ),
  );
  const result = await createFixtureClient().GET('/__test__/transport');
  expect(result.response.status).toBe(503);
  expect(result.error).toEqual({ message: 'Unavailable' });
  expect(result.data).toBeUndefined();
});
