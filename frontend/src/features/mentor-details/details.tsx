import { useQueries } from '@tanstack/react-query';
import { Link, useSearchParams } from 'react-router';
import { useSession } from '@/entities/session';
import {
  mentorQuestionnaireOptions,
  mentorDuration,
  mentorFormat,
  mentorName,
  mentorSlots,
  useMentorQuestionnaire,
} from '@/entities/mentor';
import { ApiError } from '@/shared/api';
import type { MentorQuestionnaire } from '@/shared/api';
import { comparisonAddress, comparisonIds } from '@/shared/lib';
import { Button, LoadingState } from '@/shared/ui';

function Tags({ values }: { values: string[] }) {
  return values.length ? (
    <ul className="tags">
      {values.map((value, index) => (
        <li className="tag" key={index}>
          {value}
        </li>
      ))}
    </ul>
  ) : (
    <span>—</span>
  );
}

function RequestError({ error, retry }: { error: Error; retry: () => void }) {
  const missing = error instanceof ApiError && error.status === 404;
  return (
    <div className="search-layout__notice">
      <p role="alert">
        {missing
          ? 'Наставник не найден.'
          : 'Не удалось загрузить анкету наставника.'}
      </p>
      {!missing && (
        <Button variant="secondary" onClick={retry}>
          Повторить
        </Button>
      )}
    </div>
  );
}

export function MentorProfile({ id }: { id: string }) {
  const { data: session, service } = useSession();
  const [params] = useSearchParams();
  const ids = comparisonIds(params);
  const query = useMentorQuestionnaire(
    session!.id,
    id,
    service.api.mentorQuestionnaire,
  );
  if (query.isPending)
    return (
      <>
        <h1>Профиль наставника</h1>
        <LoadingState />
      </>
    );
  if (query.isError)
    return (
      <>
        <h1>Профиль наставника</h1>
        <RequestError error={query.error} retry={() => void query.refetch()} />
      </>
    );
  const form = query.data;
  const name = mentorName(form);
  const work = [form.teacherInfo?.position, form.teacherInfo?.placeOfWork]
    .filter(Boolean)
    .join(' · ');
  return (
    <>
      <Link to={comparisonAddress('/search', ids)}>‹ К результатам поиска</Link>
      <div className="two-columns profile-layout">
        <section className="preview-stack">
          <article className="card">
            <div className="profile-heading">
              <span className="avatar" aria-hidden="true">
                {Array.from(name)[0]}
              </span>
              <div>
                <h1>{name}</h1>
                <p>{work}</p>
                <p>Опыт — {form.teacherInfo?.yearsExperience} лет</p>
                <Tags values={(form.skills ?? []).map((skill) => skill.name)} />
              </div>
            </div>
            <h2>К каким целям поможет прийти</h2>
            <p>{form.goalDescription || '—'}</p>
          </article>
        </section>
        <aside className="card preview-stack">
          <h2>Предпочтительная нагрузка</h2>
          <Tags
            values={[
              `${new Intl.NumberFormat('ru').format(form.hoursNumberWeek)} ч/нед`,
            ]}
          />
          <h2>Длительность наставничества</h2>
          <Tags values={[mentorDuration(form)]} />
          <h2>Формат урока</h2>
          <Tags values={[mentorFormat(form)]} />
          <h2>Доступные слоты</h2>
          <Tags values={mentorSlots(form)} />
          <label className="field">
            Сопроводительное сообщение
            <textarea
              className="field__control"
              rows={4}
              disabled
              placeholder="Расскажите, почему хотите заниматься и каких целей хотите достичь…"
            />
          </label>
          <Button disabled>Отправить заявку</Button>
        </aside>
      </div>
    </>
  );
}

const rows = [
  [
    'Опыт',
    (form: MentorQuestionnaire) => `${form.teacherInfo?.yearsExperience} лет`,
  ],
  [
    'Навыки',
    (form: MentorQuestionnaire) => (
      <Tags values={(form.skills ?? []).map((skill) => skill.name)} />
    ),
  ],
  [
    'Доступность',
    (form: MentorQuestionnaire) =>
      `${new Intl.NumberFormat('ru').format(form.hoursNumberWeek)} ч/нед`,
  ],
  ['Длительность', mentorDuration],
  [
    'Свободные слоты',
    (form: MentorQuestionnaire) => <Tags values={mentorSlots(form)} />,
  ],
] as const;

export function MentorComparison() {
  const { data: session, service } = useSession();
  const [params] = useSearchParams();
  const ids = comparisonIds(params);
  const queries = useQueries({
    queries: ids.map((id) =>
      mentorQuestionnaireOptions(
        session!.id,
        id,
        service.api.mentorQuestionnaire,
      ),
    ),
  });
  return (
    <>
      <h1 className="page-title">Сравнение наставников</h1>
      <Link to={comparisonAddress('/search', ids)}>Добавить наставника</Link>
      {ids.length ? (
        <div className="card comparison__container">
          <table className="comparison">
            <thead>
              <tr>
                <th scope="col">
                  <span className="visually-hidden">Критерий</span>
                </th>
                {ids.map((id, index) => (
                  <th key={id} scope="col">
                    <h2 className="comparison__name">
                      {queries[index]?.data
                        ? mentorName(queries[index].data)
                        : 'Анкета наставника'}
                    </h2>
                    <Link
                      className="button button--secondary"
                      to={comparisonAddress(
                        '/compare',
                        ids.filter((item) => item !== id),
                      )}
                    >
                      Убрать
                    </Link>
                    {queries[index]?.isPending && <LoadingState />}
                    {queries[index]?.isError && (
                      <RequestError
                        error={queries[index].error}
                        retry={() => void queries[index]?.refetch()}
                      />
                    )}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {rows.map(([label, render]) => (
                <tr key={label}>
                  <th scope="row">{label}</th>
                  {ids.map((id, index) => (
                    <td key={id}>
                      {queries[index]?.data ? render(queries[index].data) : '—'}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
            <tfoot>
              <tr>
                <td />
                {ids.map((id, index) => (
                  <td key={id}>
                    {queries[index]?.data && (
                      <Link
                        className="button button--primary"
                        to={comparisonAddress('/mentors/' + id, ids)}
                      >
                        Выбрать
                      </Link>
                    )}
                  </td>
                ))}
              </tr>
            </tfoot>
          </table>
        </div>
      ) : (
        <p className="card">
          Выберите наставников для сравнения в подборе и поиске.
        </p>
      )}
    </>
  );
}
