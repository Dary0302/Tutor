import { useState } from 'react';
import { Navigate, useSearchParams } from 'react-router';
import { useSession } from '@/entities/session';
import { Button, Dialog } from '@/shared/ui';
import { approvalPairs, rankings, rewards } from './fixtures';

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

type Pair = (typeof approvalPairs)[number];
type Details = { pair: Pair; role: 'mentee' | 'mentor' };

function Participant({
  pair,
  role,
  onDetails,
}: Details & { onDetails: () => void }) {
  const person = pair[role];
  return (
    <section className="approval__person">
      <p>{role === 'mentee' ? 'Менти' : 'Наставник'}</p>
      <h2>{person.name}</h2>
      {'workplace' in person && <p>{person.workplace}</p>}
      <SkillTags skills={person.skills} />
      {role === 'mentee' && <p>{person.summary}</p>}
      <p>{person.workload}</p>
      <Button variant="secondary" onClick={onDetails}>
        Подробнее<span className="visually-hidden"> · {person.name}</span>
      </Button>
    </section>
  );
}

function SkillTags({ skills }: { skills: string[] }) {
  return (
    <ul className="tags">
      {skills.map((skill) => (
        <li className="tag" key={skill}>
          {skill}
        </li>
      ))}
    </ul>
  );
}

function ParticipantDialog({
  pair,
  role,
  onClose,
}: Details & { onClose: () => void }) {
  const person = pair[role];
  return (
    <Dialog
      title={role === 'mentee' ? 'Анкета менти' : 'Анкета наставника'}
      size="details"
      onClose={onClose}
    >
      <div>
        <h3>{person.name}</h3>
        {'workplace' in person && <p>{person.workplace}</p>}
        <SkillTags skills={person.skills} />
      </div>
      <dl className="approval__details">
        <div>
          <dt>Город и часовой пояс</dt>
          <dd>Москва · UTC+3</dd>
        </div>
        <div>
          <dt>{role === 'mentee' ? 'Учёба' : 'Образование'}</dt>
          <dd>{person.education}</dd>
        </div>
        {role === 'mentor' && (
          <div>
            <dt>Опыт и нагрузка</dt>
            <dd>{person.workload}</dd>
          </div>
        )}
        <div>
          <dt>{role === 'mentee' ? 'Цель' : 'Чем может помочь'}</dt>
          <dd>{person.summary}</dd>
        </div>
        <div>
          <dt>Условия наставничества</dt>
          <dd>{person.terms}</dd>
        </div>
        <div>
          <dt>Доступное время</dt>
          <dd>{pair.available}</dd>
        </div>
      </dl>
    </Dialog>
  );
}

export function ApprovalQueueRoute() {
  const { data } = useSession();
  const [search] = useSearchParams();
  const [details, setDetails] = useState<Details>();
  if (!data) return <Navigate to="/login" replace />;
  if (data.role !== 2) return <h1>Доступ запрещён</h1>;
  return (
    <section className="role-page">
      <header>
        <h1>Очередь на утверждение</h1>
        <p>Рассмотрите заявки на наставничество и примите решение по паре.</p>
      </header>
      {search.get('state') === 'empty' ? (
        <section className="card role-page__empty">
          <h2>Нет пар на утверждение</h2>
          <p>Новые заявки на наставничество появятся здесь.</p>
        </section>
      ) : (
        approvalPairs.map((pair) => (
          <article key={pair.mentee.name} className="card approval">
            <div className="approval__columns">
              <Participant
                pair={pair}
                role="mentee"
                onDetails={() => setDetails({ pair, role: 'mentee' })}
              />
              <Participant
                pair={pair}
                role="mentor"
                onDetails={() => setDetails({ pair, role: 'mentor' })}
              />
              <section className="approval__person">
                <h2>Совместимость</h2>
                <strong className="match">{pair.compatibility}%</strong>
                <h3>Общие слоты</h3>
                <p>{pair.available}</p>
                <p>{pair.duration}</p>
              </section>
            </div>
            <div className="approval__decision">
              <label className="field">
                <span>Комментарий к решению</span>
                <textarea
                  className="field__control"
                  placeholder="Добавьте комментарий"
                  rows={2}
                />
              </label>
              <div className="approval__actions">
                <Button variant="secondary" disabled>
                  Отклонить
                </Button>
                <Button disabled>Утвердить</Button>
              </div>
            </div>
          </article>
        ))
      )}
      {details && (
        <ParticipantDialog {...details} onClose={() => setDetails(undefined)} />
      )}
    </section>
  );
}
