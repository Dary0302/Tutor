import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { RouterProvider } from 'react-router';
import { config } from '@/shared/config';
import { AppProviders, createQueryClient } from './providers';
import { createAppRouter } from './router';

export async function bootstrap(element: HTMLElement) {
  if (import.meta.env.DEV && config.enableMocks) {
    const { worker } = await import('../test/mocks/browser');
    await worker.start({ onUnhandledRequest: 'error' });
  }
  const root = createRoot(element);
  root.render(
    <StrictMode>
      <AppProviders client={createQueryClient()}>
        <RouterProvider router={createAppRouter()} />
      </AppProviders>
    </StrictMode>,
  );
  return root;
}
