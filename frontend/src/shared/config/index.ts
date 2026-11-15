import { z } from 'zod';

const schema = z.object({
  VITE_API_BASE_URL: z.union([z.literal(''), z.url()]).default(''),
  VITE_ENABLE_MOCKS: z.enum(['true', 'false']).default('false'),
});

export function parseConfig(input: unknown) {
  const env = schema.parse(input);
  return {
    apiBaseUrl: env.VITE_API_BASE_URL,
    enableMocks: env.VITE_ENABLE_MOCKS === 'true',
  };
}

export const config = parseConfig(import.meta.env);
