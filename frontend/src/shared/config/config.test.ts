import { describe, expect, it } from 'vitest';
import { parseConfig } from './index';
describe('environment configuration', () => {
  it('defaults to an explicit local API and disabled mocks', () => {
    expect(parseConfig({})).toEqual({
      apiBaseUrl: '',
      enableMocks: false,
    });
  });
  it('accepts a configured backend and explicit mock opt-in', () => {
    expect(
      parseConfig({
        VITE_API_BASE_URL: 'https://api.example.com',
        VITE_ENABLE_MOCKS: 'true',
      }),
    ).toEqual({ apiBaseUrl: 'https://api.example.com', enableMocks: true });
  });
  it('rejects invalid URLs and accidental truthy mock strings', () => {
    expect(() => parseConfig({ VITE_API_BASE_URL: 'invalid' })).toThrow();
    expect(() => parseConfig({ VITE_ENABLE_MOCKS: 'yes' })).toThrow();
  });
});
