import { render } from '@testing-library/react';
import { createMemoryRouter, RouterProvider } from 'react-router';
import { AppProviders, createQueryClient } from '@/app/providers';
import { routeObjects } from '@/app/router';
import { profiles } from './fixtures/api';
export function renderPage(path: string, role?: number) {
  const client = createQueryClient();
  client.setDefaultOptions({
    queries: { retry: false },
    mutations: { retry: false },
  });
  if (role !== undefined)
    client.setQueryData(
      ['session'],
      profiles[role] ?? { ...profiles[0], role },
    );
  const router = createMemoryRouter(routeObjects, { initialEntries: [path] });
  return {
    ...render(
      <AppProviders client={client}>
        <RouterProvider router={router} />
      </AppProviders>,
    ),
    router,
    client,
  };
}
