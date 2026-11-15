import { act, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { createMemoryRouter, RouterProvider } from 'react-router';
import { describe, expect, it, vi } from 'vitest';
import { routeObjects } from './router';
import { AppProviders, createQueryClient } from './providers';
import { bootstrap } from './bootstrap';

vi.mock('../test/mocks/browser', () => ({ worker: { start: vi.fn() } }));

describe('application shell', () => {
  it('offers guest entry points and live home content', async () => {
    const client = createQueryClient();
    render(
      <AppProviders client={client}>
        <RouterProvider router={createMemoryRouter(routeObjects)} />
      </AppProviders>,
    );
    expect(await screen.findByRole('heading', { level: 1 })).toHaveTextContent(
      'Ищешь наставника или ученика?',
    );
    expect(
      screen.getByRole('heading', { name: 'Как это работает' }),
    ).toBeVisible();
    expect(screen.getByRole('link', { name: 'Регистрация' })).toHaveAttribute(
      'href',
      '/register',
    );
  });

  it('recovers from an unknown address through the home link', async () => {
    const user = userEvent.setup();
    render(
      <AppProviders client={createQueryClient()}>
        <RouterProvider
          router={createMemoryRouter(routeObjects, {
            initialEntries: ['/missing'],
          })}
        />
      </AppProviders>,
    );
    expect(
      await screen.findByRole('heading', { name: 'Страница не найдена' }),
    ).toBeVisible();
    await user.click(screen.getByRole('link', { name: 'На главную' }));
    expect(await screen.findByRole('heading', { level: 1 })).toHaveTextContent(
      'Ищешь наставника или ученика?',
    );
  });

  it('shows the route fallback when route loading fails', async () => {
    const router = createMemoryRouter([
      {
        ...routeObjects[0],
        loader: () => {
          throw new Error('Unavailable');
        },
      },
    ]);
    render(<RouterProvider router={router} />);
    expect(
      await screen.findByRole('heading', {
        name: 'Не удалось открыть страницу',
      }),
    ).toBeVisible();
    expect(screen.getByRole('link', { name: 'На главную' })).toHaveAttribute(
      'href',
      '/',
    );
  });

  it('mounts the real application in the browser root', async () => {
    const element = document.createElement('div');
    document.body.append(element);
    let root: Awaited<ReturnType<typeof bootstrap>>;
    await act(async () => {
      root = await bootstrap(element);
    });
    expect(await screen.findByRole('heading', { level: 1 })).toHaveTextContent(
      'Ищешь наставника или ученика?',
    );
    act(() => root.unmount());
    element.remove();
  });

  it('starts the development worker before rendering when mocks are enabled', async () => {
    const { config } = await import('@/shared/config');
    const { worker } = await import('../test/mocks/browser');
    const start = vi.spyOn(worker, 'start').mockResolvedValue(undefined);
    config.enableMocks = true;
    const element = document.createElement('div');
    document.body.append(element);
    try {
      let root: Awaited<ReturnType<typeof bootstrap>>;
      await act(async () => {
        root = await bootstrap(element);
      });
      expect(start).toHaveBeenCalledWith({ onUnhandledRequest: 'error' });
      expect(await screen.findByRole('heading', { level: 1 })).toBeVisible();
      act(() => root.unmount());
    } finally {
      config.enableMocks = false;
      start.mockRestore();
      element.remove();
    }
  });
});
