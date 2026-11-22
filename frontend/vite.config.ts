import { fileURLToPath, URL } from 'node:url';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import { defineConfig } from 'vitest/config';

export default defineConfig({
  server: {
    proxy: { '/api': 'http://localhost:5003' },
    watch: {
      ignored: [
        '**/.tools/**',
        '**/.pnpm-store/**',
        '**/coverage/**',
        '**/test-results/**',
      ],
    },
  },
  plugins: [react(), tailwindcss()],
  resolve: { alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) } },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    include: ['src/**/*.test.{ts,tsx}'],
    coverage: {
      provider: 'v8',
      include: ['src/**/*.{ts,tsx}'],
      exclude: ['src/**/*.test.{ts,tsx}', 'src/**/*.d.ts', 'src/test/**'],
      thresholds: { statements: 80, lines: 80, functions: 75, branches: 70 },
      reporter: ['text', 'html', 'lcov'],
    },
  },
});
