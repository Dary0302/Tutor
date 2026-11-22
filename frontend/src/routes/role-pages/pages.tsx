import { Navigate } from 'react-router';
import { useSession } from '@/entities/session';
import { rankings, rewards } from './fixtures';

export function RewardsRoute() {
  const { data } = useSession();
  if (!data) return <Navigate to="/login" replace />;
  if (data.role !== 0 && data.role !== 1) return <h1>Доступ запрещён</h1>;
  const reward = rewards[data.role];
  return (
    <section className="role-page">
      <header>
        <h1>Награды</h1>
        <p>Достижения и рейтинг участников «Куратора».</p>
      </header>
      <div className="role-page__columns">
        <section className="card role-page__tile role-page__tile--summary">
          <h2>Баланс</h2>
          <strong>{reward.points} баллов</strong>
          <p>Накоплено за активность</p>
        </section>
        <section className="card role-page__tile role-page__tile--summary">
          <h2>Место в рейтинге</h2>
          <strong>{reward.rank}</strong>
          <p>По баллам участников</p>
        </section>
      </div>
      <section className="role-page__badges">
        <h2>Мои бейджи</h2>
        <ul className="role-page__columns role-page__list">
          {reward.badges.map(([icon, title, text]) => (
            <li className="card role-page__badge" key={title}>
              <span aria-hidden="true">
                {icon === '★' ? (
                  <svg
                    width="20"
                    height="20"
                    viewBox="0 0 24 24"
                    fill="currentColor"
                  >
                    <path d="m12 2 3 6 7 1-5 5 1 8-6-4-6 4 1-8-5-5 7-1Z" />
                  </svg>
                ) : (
                  icon
                )}
              </span>
              <div>
                <h3>{title}</h3>
                <p>{text}</p>
              </div>
            </li>
          ))}
        </ul>
      </section>
      <div className="role-page__columns">
        {rankings.map(({ title, entries }) => (
          <section className="card role-page__tile" key={title}>
            <h2>{title}</h2>
            <ol className="role-page__ranking">
              {entries.map(([name, score]) => (
                <li key={name}>
                  <div>
                    <span>{name}</span>
                    <strong>{score} баллов</strong>
                  </div>
                </li>
              ))}
            </ol>
          </section>
        ))}
      </div>
    </section>
  );
}
