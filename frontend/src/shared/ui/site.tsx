import { Link, NavLink, useSearchParams } from 'react-router';
import { comparisonAddress, comparisonIds } from '@/shared/lib';
import { Button } from './controls';
import { useState } from 'react';

export function Brand() {
  return (
    <Link className="brand" to="/">
      <span className="brand__mark">К</span>Куратор
    </Link>
  );
}
const menti = [
  ['Мои наставники', null],
  ['Подбор и поиск', '/search'],
  ['Сравнение', '/compare'],
  ['Календарь', '/calendar'],
  ['Цели и прогресс', '/goals'],
  ['Награды', '/rewards'],
];
const mentor = [
  ['Главная', '/mentor'],
  ['Мои подопечные', null],
  ['Заявки', null],
  ['Календарь', '/calendar'],
  ['Награды', '/rewards'],
];
const curator = [
  ['Очередь на утверждение', '/approval-queue'],
  ['Обращения', null],
  ['Контент', null],
];
export function SiteHeader({
  role,
  initial,
  onLogout,
}: {
  role?: number | undefined;
  initial?: string | undefined;
  onLogout: () => void;
}) {
  const [open, setOpen] = useState(false);
  const [params] = useSearchParams();
  const selection = comparisonIds(params);
  return (
    <header
      className={
        role === undefined ? 'site-header site-header--guest' : 'site-header'
      }
    >
      <div className="site-header__inner">
        <Brand />
        {role !== undefined && (
          <nav aria-label="Основная навигация">
            <ul className="site-header__nav">
              {(role === 0 ? menti : role === 1 ? mentor : curator).map(
                ([label, path]) => (
                  <li key={label}>
                    {path ? (
                      <NavLink
                        className="site-header__link"
                        to={
                          path === '/compare' || path === '/search'
                            ? comparisonAddress(path, selection)
                            : path
                        }
                      >
                        {label}
                      </NavLink>
                    ) : (
                      <span
                        className="site-header__inactive"
                        aria-disabled="true"
                      >
                        {label}
                      </span>
                    )}
                  </li>
                ),
              )}
            </ul>
          </nav>
        )}
        <div className="site-header__account">
          {role === undefined ? (
            <>
              <Link className="button button--secondary" to="/login">
                Войти
              </Link>
              <Link className="button button--primary" to="/register">
                Регистрация
              </Link>
            </>
          ) : (
            <>
              {role < 2 && (
                <Link className="button button--primary" to="/questionnaire">
                  Моя анкета
                </Link>
              )}
              <div className="profile-menu">
                <Button
                  variant="secondary"
                  className="profile-menu__trigger"
                  aria-label="Меню профиля"
                  aria-expanded={open}
                  onClick={() => setOpen(!open)}
                >
                  {initial || role === 2 ? (
                    <span aria-hidden="true">{initial ?? 'К'}</span>
                  ) : (
                    <svg
                      width="24"
                      height="24"
                      viewBox="0 0 24 24"
                      fill="none"
                      aria-hidden="true"
                    >
                      <circle cx="12" cy="8" r="4" stroke="currentColor" />
                      <path
                        d="M4 22v-3a8 8 0 0 1 16 0v3"
                        stroke="currentColor"
                      />
                    </svg>
                  )}
                </Button>
                {open && (
                  <Button
                    variant="secondary"
                    onClick={() => {
                      setOpen(false);
                      onLogout();
                    }}
                  >
                    Выйти из аккаунта
                  </Button>
                )}
              </div>
            </>
          )}
        </div>
      </div>
    </header>
  );
}
export function SiteFooter() {
  return (
    <footer className="site-footer">
      <div className="site-footer__inner">
        <section>
          <Brand />
          <p>
            Платформа наставничества: интеллектуальный подбор +<br />
            решение человека-куратора.
          </p>
          <p>© 2026 Куратор</p>
        </section>
        <section>
          <h2>Платформа</h2>
          <ul>
            <li>
              <span aria-disabled="true">Новости</span>
            </li>
            <li>
              <span aria-disabled="true">FAQ</span>
            </li>
          </ul>
        </section>
        <section>
          <h2>Документы</h2>
          <span aria-disabled="true">Пользовательское соглашение</span>
        </section>
        <section>
          <h2>Контакты</h2>
          <span>info@kurator.example</span>
        </section>
      </div>
    </footer>
  );
}
