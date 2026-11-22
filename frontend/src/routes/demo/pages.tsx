import { Link, Navigate, useParams, useSearchParams } from 'react-router';
import type { ReactNode } from 'react';
import { useSession } from '@/entities/session';
import { Button, Input, Select } from '@/shared/ui';
import { mentors, htmlTasks, cssTasks } from './fixtures';

function Preview({ children }: { children: ReactNode }) {
  const session = useSession();
  if (!session.data) return <Navigate to="/login" replace />;
  if (session.data.role > 1)
    return (
      <section>
        <h1>Доступ запрещён</h1>
      </section>
    );
  return <>{children}</>;
}
function Tags({ values }: { values: string[] }) {
  return (
    <ul className="tags">
      {values.map((value) => (
        <li className="tag" key={value}>
          {value}
        </li>
      ))}
    </ul>
  );
}
function Star() {
  return (
    <svg
      width="16"
      height="16"
      viewBox="0 0 24 24"
      fill="currentColor"
      aria-hidden="true"
    >
      <path d="m12 2 3 6 7 1-5 5 1 8-6-4-6 4 1-8-5-5 7-1Z" />
    </svg>
  );
}
export function CompareRoute() {
  const rows: [string, (m: (typeof mentors)[number]) => ReactNode][] = [
    ['Совместимость', (m) => <span className="match">{m.score}%</span>],
    [
      'Рейтинг',
      (m) => (
        <>
          <Star /> {m.rating}
        </>
      ),
    ],
    ['Опыт', (m) => `${m.years} лет`],
    [
      'Навыки',
      (m) => <Tags values={m.skills.filter((s) => s !== 'Нейросети')} />,
    ],
    ['Доступность', (m) => `${m.hours} ч/нед`],
    ['Длительность', (m) => <Tags values={m.months} />],
    ['Свободные слоты', (m) => m.days],
  ];
  return (
    <Preview>
      <h1 className="page-title">Сравнение наставников</h1>
      <div className="card">
        <table className="comparison">
          <thead>
            <tr>
              <th scope="col">
                <span className="visually-hidden">Критерий</span>
              </th>
              {mentors.map((m) => (
                <th key={m.id} scope="col">
                  {m.name}
                  <br />
                  <Button variant="secondary" disabled>
                    Убрать
                  </Button>
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {rows.map(([label, render]) => (
              <tr key={label}>
                <th scope="row">{label}</th>
                {mentors.map((m) => (
                  <td key={m.id}>{render(m)}</td>
                ))}
              </tr>
            ))}
          </tbody>
          <tfoot>
            <tr>
              <td />
              {mentors.map((m) => (
                <td key={m.id}>
                  {m.id === 'alexey' ? (
                    <Link
                      className="button button--primary"
                      to={`/mentors/${m.id}`}
                    >
                      Выбрать
                    </Link>
                  ) : (
                    <Button disabled>Выбрать</Button>
                  )}
                </td>
              ))}
            </tr>
          </tfoot>
        </table>
      </div>
    </Preview>
  );
}
export function MentorProfileRoute() {
  const { id } = useParams();
  if (id !== 'alexey') return <Navigate to="/not-found" replace />;
  return (
    <Preview>
      <Link to="/search">‹ К результатам поиска</Link>
      <div className="two-columns profile-layout">
        <section className="preview-stack">
          <article className="card">
            <div className="profile-heading">
              <span className="avatar">А</span>
              <div>
                <h1>Алексей Морозов</h1>
                <p className="muted">
                  Senior Data Scientist · Яндекс · 7 лет опыта
                </p>
                <Tags values={['Python', 'Нейросети', 'ML']} />
              </div>
            </div>
            <h2>О наставнике</h2>
            <p>
              Рекомендательные системы, NLP, проекты в поиске
              <br />и голосовых ассистентах. Помогу выстроить путь в Data
              Science.
            </p>
          </article>
          <h2>
            Отзывы{' '}
            <span className="tag">
              <Star /> 4.9 · 12 отзывов
            </span>
          </h2>
          {[
            ['Дмитрий К.', 'Помог собрать портфолио и выйти на стажировку.'],
            ['Анна П.', 'Чёткие задачи и честная обратная связь.'],
          ].map(([name, text]) => (
            <article className="card" key={name}>
              <div className="row">
                <h3>{name}</h3>
                <span>
                  <Star /> 5.0
                </span>
              </div>
              <p className="muted">
                Полезность 5 · Коммуникация 5 · Пунктуальность 5
              </p>
              <p>{text}</p>
            </article>
          ))}
        </section>
        <aside className="card preview-stack">
          <strong className="match match--large">94%</strong>
          <p>совместимость с вами</p>
          <Tags values={['1,5 ч/нед']} />
          <h2>Длительность наставничества</h2>
          <Tags values={['1 мес.', '3 мес.', '6 мес.']} />
          <h2>Формат урока</h2>
          <Tags values={['Очно', 'Онлайн']} />
          <h2>Доступные слоты</h2>
          <Tags values={['Пн 18:00', 'Ср 19:00', 'Сб 11:00']} />
          <label className="field">
            Сопроводительное сообщение
            <textarea
              className="field__control"
              rows={5}
              disabled
              placeholder="Расскажите, почему хотите заниматься и каких целей хотите достичь…"
            />
          </label>
          <Button disabled>Отправить заявку</Button>
        </aside>
      </div>
    </Preview>
  );
}
const applicationData = [
  [
    'Екатерина Смирнова',
    ['Data Science', 'Python'],
    'найти стажировку за 6 месяцев',
  ],
  [
    'Павел Андреев',
    ['Backend', 'Архитектура'],
    'спроектировать собственный сервис',
  ],
] as const;
export function MentorHomeRoute() {
  const [params] = useSearchParams();
  const upcoming = params.get('session') !== 'none';
  return (
    <Preview>
      <h1>Главная наставника</h1>
      <p className="muted">
        Здравствуйте, Алексей! Вот что происходит с вашим наставничеством.
      </p>
      <div className="stats">
        {[
          ['Проведено сессий', '32', 'За всё время'],
          ['Подопечных', '3', 'Сейчас занимаются с вами'],
          ['Новых заявок', '2', 'Ожидают вашего решения'],
        ].map(([label, value, caption]) => (
          <article className="stats__card" key={label}>
            <h2>{label}</h2>
            <strong>{value}</strong>
            <span>{caption}</span>
          </article>
        ))}
      </div>
      <div className={upcoming ? 'two-columns' : 'preview-stack'}>
        <section className="preview-stack">
          <h2>
            Заявки на наставничество <span className="tag">2 новые</span>
          </h2>
          {applicationData.map(([name, tags, goal]) => (
            <article className="card" key={name}>
              <div className="row">
                <h3>{name}</h3>
                <span className="muted">Сегодня</span>
              </div>
              <Tags values={[...tags]} />
              <p>Цель: {goal}</p>
              <div className="actions">
                {['Принять', 'Уточнить', 'Отклонить'].map((label, i) => (
                  <Button
                    key={label}
                    variant={i === 0 ? 'primary' : 'secondary'}
                    disabled
                  >
                    {label}
                  </Button>
                ))}
              </div>
            </article>
          ))}
        </section>
        {upcoming && (
          <aside className="preview-stack">
            <h2>Предстоящая сессия</h2>
            <article className="card preview-stack">
              <Tags values={['3 ноября · вторник']} />
              <h3>18:00 — 19:00</h3>
              <h3>Екатерина Смирнова</h3>
              <p>Разбор ML-проекта</p>
              <Tags values={['Онлайн', '60 мин']} />
              <Link className="button button--primary" to="/calendar">
                Открыть календарь
              </Link>
            </article>
          </aside>
        )}
      </div>
      <section className="schedule-banner">
        <div>
          <h2>Расписание под рукой</h2>
          <p>
            Нажмите на день в календаре — дата новой сессии заполнится
            автоматически.
          </p>
        </div>
        <Link className="button button--primary" to="/calendar">
          Календарь
        </Link>
      </section>
    </Preview>
  );
}
export function CalendarRoute() {
  const { data } = useSession();
  const mentor = data?.role === 1;
  const names = mentor
    ? ['Екатерина Смирнова', 'Павел Андреев']
    : ['Александр Иванов', 'Мария Петрова'];
  const cells = Array.from({ length: 42 }, (_, index) => ({
    day: index < 6 ? 26 + index : index > 35 ? index - 35 : index - 5,
    outside: index < 6 || index > 35,
  }));
  const events = [3, 4, 10, 17];
  const calendar = (
    <section className="card calendar" aria-label="Ноябрь 2026">
      <div className="row">
        <Button disabled aria-label="Предыдущий месяц">
          ‹
        </Button>
        <h2>Ноябрь 2026</h2>
        <Button disabled aria-label="Следующий месяц">
          ›
        </Button>
      </div>
      <div className="calendar__grid">
        {['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс'].map((day) => (
          <strong key={day}>{day}</strong>
        ))}
        {cells.map(({ day, outside }, index) => (
          <div
            className={`calendar__day${outside ? ' calendar__day--outside' : ''}${day === 3 && !outside ? ' calendar__day--selected' : ''}`}
            key={index}
          >
            <strong>{day}</strong>
            {!outside && events.includes(day) && (
              <div className="calendar__event">
                <strong>{day === 4 ? '19:00–20:00' : '18:00–19:00'}</strong>
                <span>{names[day === 4 ? 1 : 0]}</span>
              </div>
            )}
          </div>
        ))}
      </div>
    </section>
  );
  return (
    <Preview>
      <h1>Календарь сессий</h1>
      <p className="muted">
        {mentor
          ? 'Планируйте встречи с подопечными и управляйте расписанием'
          : 'Ваши встречи с наставниками · время указано по Москве, UTC+3'}
      </p>
      <div className={mentor ? 'two-columns' : 'preview-stack'}>
        {calendar}
        {mentor && (
          <aside className="card preview-stack">
            <h2>Новая сессия</h2>
            <Input label="Подопечный" value={names[0]} disabled />
            <Input label="Дата" value="03.11.2026" disabled />
            <p>
              Дата подставлена из выбранного дня.
              <br />
              Можно выбрать другую в поле даты.
            </p>
            <div className="form-grid">
              <Input label="Время начала" value="18:00" disabled />
              <Input label="Время конца" value="19:00" disabled />
            </div>
            <Input label="Тема сессии" value="Разбор ML-проекта" disabled />
            <Button disabled>Запланировать</Button>
          </aside>
        )}
      </div>
      <h2>Сессии месяца</h2>
      <div className="card">
        <table className="sessions">
          <tbody>
            {events.slice(0, mentor ? 2 : 4).map((day, i) => (
              <tr key={day}>
                <td>{String(day).padStart(2, '0')}.11.2026</td>
                <td>{day === 4 ? '19:00–20:00' : '18:00–19:00'}</td>
                <td>{names[day === 4 ? 1 : 0]}</td>
                <td>
                  {
                    (mentor
                      ? ['Разбор ML-проекта', 'Архитектура сервиса']
                      : [
                          'Разбор HTML-дашборда',
                          'Адаптивная вёрстка',
                          'Разбор задач',
                          'Проверка прогресса',
                        ])[i]
                  }
                </td>
                {mentor && (
                  <td>
                    <Button variant="secondary" disabled>
                      Отменить
                    </Button>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </Preview>
  );
}
function ProgressPanel() {
  return (
    <aside className="preview-stack">
      <article className="card">
        <h2>Прогресс по навыкам</h2>
        {[
          ['HTML', 50, 5, 10],
          ['CSS', 100, 3, 3],
        ].map(([name, value, count, total]) => (
          <div className="progress" key={name}>
            <div className="row">
              <label htmlFor={`progress-${name}`}>{name}</label>
              <strong>{value}%</strong>
            </div>
            <progress id={`progress-${name}`} value={Number(value)} max={100} />
            <p>
              {count} из {total} задач выполнено
            </p>
          </div>
        ))}
        <p className="muted">Прогресс обновляется по выполненным задачам.</p>
      </article>
      <article className="card">
        <h2>Проверка цели</h2>
        <p>
          Когда выполнены все задачи, цель автоматически отправляется
          наставнику.
          <br />
          Наставник подтвердит её завершение.
        </p>
      </article>
    </aside>
  );
}
export function GoalsRoute() {
  const { data } = useSession();
  const mentor = data?.role === 1;
  const [parameters, setParameters] = useSearchParams();
  const selection =
    parameters.get('mentor') === 'maria' ? 'maria' : 'alexander';
  return (
    <Preview>
      <h1>Цели и прогресс</h1>
      <p className="muted">
        {mentor
          ? 'Создавайте цели для подопечных и отслеживайте развитие их навыков'
          : 'Выполняйте задачи и развивайте навыки вместе с наставником'}
      </p>
      {mentor && (
        <Select label="Подопечный" disabled>
          <option>Екатерина Смирнова</option>
        </Select>
      )}
      {!mentor && (
        <div className="goals__mentor">
          <Select
            label="Наставник"
            value={selection}
            onChange={(event) =>
              setParameters((previous) => {
                const next = new URLSearchParams(previous);
                if (event.target.value === 'maria') next.set('mentor', 'maria');
                else next.delete('mentor');
                return next;
              })
            }
          >
            <option value="alexander">Александр Иванов</option>
            <option value="maria">Мария Петрова</option>
          </Select>
        </div>
      )}
      {!mentor && selection === 'maria' ? (
        <section className="card role-page__empty">
          <h2>Пока нет целей</h2>
          <p>Здесь появятся цели и задачи от Марии Петровой.</p>
        </section>
      ) : (
        <div className="two-columns">
          <section className="preview-stack">
            {mentor && (
              <>
                <article className="card preview-stack">
                  <h2>Новая цель</h2>
                  <div className="form-grid">
                    <Input
                      label="Название цели"
                      value="Подключить данные к дашборду"
                      disabled
                    />
                    <Input label="Навык" value="JavaScript" disabled />
                  </div>
                  <Input
                    label="Задача 1"
                    value="Получить данные из API"
                    disabled
                  />
                  <Input
                    label="Задача 2"
                    value="Вывести данные в карточках"
                    disabled
                  />
                  <div className="row">
                    <Button variant="secondary" disabled>
                      Добавить задачу
                    </Button>
                    <Button disabled>Создать цель</Button>
                  </div>
                </article>
                <h2>Цели Екатерины</h2>
              </>
            )}
            {[
              [htmlTasks, 'Сверстать дашборд', 'HTML', 5],
              [cssTasks, 'Сделать дашборд адаптивным', 'CSS', 3],
            ].map(([tasks, title, skill, done]) => (
              <article className="card" key={String(title)}>
                <div className="row">
                  <h2>{String(title)}</h2>
                  {!mentor && (
                    <span className="tag">
                      {done === 3 ? 'На проверке' : 'В работе'}
                    </span>
                  )}
                </div>
                <div className="actions">
                  <span className="tag">{String(skill)}</span>
                  <p className="muted">
                    {Number(done)} из {(tasks as string[]).length} задач
                    выполнено
                  </p>
                </div>
                {done === 3 && (
                  <p>
                    Все задачи выполнены · цель отправлена на проверку
                    наставнику
                  </p>
                )}
                <h3>Задачи</h3>
                <ul className="task-list">
                  {(tasks as string[]).map((task, index) => (
                    <li key={task}>
                      <input
                        type="checkbox"
                        checked={index < Number(done)}
                        disabled
                        aria-label={task}
                      />
                      <span>{task}</span>
                      {index < Number(done) ? (
                        <span className="task-list__done">Выполнено</span>
                      ) : (
                        <Button variant="secondary" disabled>
                          {mentor ? 'В работе' : 'Выполнено'}
                        </Button>
                      )}
                      {mentor && (
                        <Button
                          variant="secondary"
                          disabled
                          aria-label={`Удалить задачу: ${task}`}
                        >
                          <svg
                            width="16"
                            height="16"
                            viewBox="0 0 16 16"
                            aria-hidden="true"
                          >
                            <path d="m4 4 8 8M4 12l8-8" stroke="currentColor" />
                          </svg>
                        </Button>
                      )}
                    </li>
                  ))}
                </ul>
                {mentor && (
                  <div className="row">
                    <Button variant="secondary" disabled>
                      Добавить задачу
                    </Button>
                    {done === 3 ? (
                      <Button disabled>Отметить цель выполненной</Button>
                    ) : (
                      <span className="muted">
                        Завершение доступно после выполнения всех задач
                      </span>
                    )}
                  </div>
                )}
              </article>
            ))}
          </section>
          <ProgressPanel />
        </div>
      )}
    </Preview>
  );
}
