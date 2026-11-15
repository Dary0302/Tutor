import { act, screen } from '@testing-library/react';
import { expect, it, vi } from 'vitest';

it('fails explicitly if the HTML root is missing', async () => {
  await expect(import('./main')).rejects.toThrow('Root element is missing');
});
it('starts from the HTML entry point', async () => {
  vi.resetModules();
  const root = document.createElement('div');
  root.id = 'root';
  document.body.append(root);
  await act(async () => {
    await import('./main');
  });
  expect(await screen.findByRole('heading', { level: 1 })).toHaveTextContent(
    'Ищешь наставника или ученика?',
  );
  root.remove();
});
