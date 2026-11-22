import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/react';
import { afterAll, afterEach, beforeAll } from 'vitest';
import { server } from './mocks/server';
import { resetMockState } from './mocks/handlers';
Object.defineProperty(navigator, 'locks', {
  configurable: true,
  value: {
    request: async (_name: string, callback: () => Promise<unknown>) =>
      callback(),
  },
});
beforeAll(() => server.listen({ onUnhandledRequest: 'error' }));
afterEach(() => {
  cleanup();
  server.resetHandlers();
  localStorage.clear();
  resetMockState();
});
afterAll(() => server.close());
