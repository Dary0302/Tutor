import { Link } from 'react-router';
import { Card } from '@/shared/ui';
import { useSession } from '@/entities/session';
const steps = [
  ['1. Анкета', 'Цели, интересы, доступность'],
  ['2. Рекомендации', '5–10 наставников с рейтингом совместимости'],
  ['3. Заявка', 'Наставник принимает, куратор утверждает пару'],
  ['4. Развитие', 'Сессии, цели, прогресс, бейджи и валюта'],
];
const capabilities = [
  [
    'Matching Engine',
    'Коллаборативная фильтрация и скрытые факторы совместимости.',
  ],
  ['Гибридная модель', 'Куратор-психолог снижает риск на финальном этапе.'],
  ['Геймификация', 'Рейтинг, бейджи, игровая валюта за наставничество.'],
];
export function HomeRoute() {
  const session = useSession();
  const role = session.data?.role;
  return (
    <div className="home">
      <section className="home__hero">
        <h1>Ищешь наставника или ученика?</h1>
        <p>
          Интеллектуальный подбор по 5+ критериям и финальное решение куратора.
          <br />
          Чат, календарь, трекер целей и награды — в одном месте.
        </p>
        {role === 1 ? (
          <span className="button button--secondary" aria-disabled="true">
            Мои подопечные
          </span>
        ) : (
          <Link
            className="button button--secondary"
            to={
              role === 2 ? '/approval-queue' : role === 0 ? '/search' : '/login'
            }
          >
            {role === 2
              ? 'Очередь на утверждение'
              : role === 0
                ? 'Подобрать наставника'
                : 'Войти'}
          </Link>
        )}
      </section>
      <section className="home__section">
        <h2>Как это работает</h2>
        <div className="home__steps">
          {steps.map(([title, text]) => (
            <Card key={title} title={title ?? ''}>
              <p>{text}</p>
            </Card>
          ))}
        </div>
      </section>
      <section className="home__section">
        <h2>Возможности</h2>
        <div className="home__columns">
          {capabilities.map(([title, text]) => (
            <Card key={title} title={title ?? ''}>
              <p>{text}</p>
            </Card>
          ))}
        </div>
      </section>
      <section className="home__section home__section--articles">
        <h2>Свежие статьи</h2>
        <div className="home__columns">
          {[
            [
              'Статья',
              'Как подготовиться к первой сессии',
              'Куратор-психолог · 5 мин',
            ],
            ['Статья', '5 ошибок при выборе ментора', 'Куратор · 4 мин'],
            [
              'Событие',
              'Хакатон по ML — 14 ноября',
              'Онлайн · регистрация открыта',
            ],
          ].map(([tag, title, text]) => (
            <article className="card" key={title}>
              <span className="tag">{tag}</span>
              <h3 className="card__title">{title}</h3>
              <p>{text}</p>
            </article>
          ))}
        </div>
      </section>
    </div>
  );
}
