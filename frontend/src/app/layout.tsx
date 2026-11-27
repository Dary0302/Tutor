import { Outlet, useLocation } from 'react-router';
import { SiteHeader, SiteFooter, LoadingState, Button } from '@/shared/ui';
import { useSession } from '@/entities/session';
import { useSavedQuestionnaire } from '@/entities/questionnaire';
export function RootLayout() {
  const session = useSession();
  const location = useLocation();
  const auth = ['/login', '/register'].includes(location.pathname);
  const saved = useSavedQuestionnaire(
    session.data?.id,
    session.service.api.questionnaire,
    !auth && (session.data?.role ?? 2) < 2,
  );
  const wide =
    /^\/(mentors\/|mentor(?:$|\/)|calendar|goals|rewards|approval-queue|applications)/.test(
      location.pathname,
    );
  if (session.isPending)
    return (
      <main className="page">
        <LoadingState />
      </main>
    );
  if (session.isError)
    return (
      <main className="page">
        <h1>Не удалось восстановить вход</h1>
        <p role="alert">{session.error.message}</p>
        <Button onClick={() => void session.refetch()}>Повторить</Button>
        <Button variant="secondary" onClick={session.logout}>
          Войти заново
        </Button>
      </main>
    );
  if (session.data && session.data.role > 2)
    return (
      <main className="page">
        <h1>Доступ запрещён</h1>
        <p>Кабинет куратора пока не реализован.</p>
        <Button onClick={session.logout}>Выйти из аккаунта</Button>
      </main>
    );
  return (
    <>
      {!auth && (
        <SiteHeader
          role={session.data?.role}
          initial={Array.from(
            saved.data?.firstName.trim() ?? '',
          )[0]?.toLocaleUpperCase('ru')}
          onLogout={session.logout}
        />
      )}
      <main className={auth ? 'auth-page' : `page${wide ? ' page--wide' : ''}`}>
        <Outlet />
      </main>
      {!auth && <SiteFooter />}
    </>
  );
}
