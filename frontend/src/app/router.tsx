import { createBrowserRouter } from 'react-router';
import { HomeRoute } from '@/routes/home';
import { NotFoundRoute } from '@/routes/not-found';
import { ErrorState, LoadingState } from '@/shared/ui';
import { RootLayout } from './layout';
import { LoginRoute, RegisterRoute } from '@/routes/auth';
import { QuestionnaireRoute } from '@/routes/questionnaire';
export const routeObjects = [
  {
    element: <RootLayout />,
    hydrateFallbackElement: (
      <main className="page">
        <LoadingState />
      </main>
    ),
    errorElement: <ErrorState />,
    children: [
      { index: true, element: <HomeRoute /> },
      { path: 'login', element: <LoginRoute /> },
      { path: 'register', element: <RegisterRoute /> },
      { path: 'questionnaire', element: <QuestionnaireRoute /> },
      {
        path: 'search',
        lazy: async () => ({
          Component: (await import('@/routes/search')).SearchRoute,
        }),
      },
      {
        path: 'compare',
        lazy: async () => ({
          Component: (await import('@/routes/mentor-details')).CompareRoute,
        }),
      },
      {
        path: 'mentors/:id',
        lazy: async () => ({
          Component: (await import('@/routes/mentor-details'))
            .MentorProfileRoute,
        }),
      },
      {
        path: 'mentor',
        lazy: async () => ({
          Component: (await import('@/routes/demo')).MentorHomeRoute,
        }),
      },
      {
        path: 'calendar',
        lazy: async () => ({
          Component: (await import('@/routes/demo')).CalendarRoute,
        }),
      },
      {
        path: 'goals',
        lazy: async () => ({
          Component: (await import('@/routes/demo')).GoalsRoute,
        }),
      },
      {
        path: 'rewards',
        lazy: async () => ({
          Component: (await import('@/routes/role-pages')).RewardsRoute,
        }),
      },
      {
        path: 'approval-queue',
        lazy: async () => ({
          Component: (await import('@/routes/role-pages')).ApprovalQueueRoute,
        }),
      },
      { path: '*', element: <NotFoundRoute /> },
    ],
  },
];
export function createAppRouter() {
  return createBrowserRouter(routeObjects);
}
