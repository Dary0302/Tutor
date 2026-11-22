import { useState } from 'react';
import type { ReactNode } from 'react';
import { useSession } from '@/entities/session';
import { useApplications, useApplicationAction } from '@/entities/applications';
import { Button, LoadingState } from '@/shared/ui';
import {
  Participant,
  ParticipantDialog,
  ApplicationMatch,
} from './participants';
import type { ParticipantDetails } from './participants';
import { DecisionDialog } from './decision';

function ListState({
  pending,
  failed,
  empty,
  retry,
  children,
}: {
  pending: boolean;
  failed: boolean;
  empty: boolean;
  retry: () => void;
  children: ReactNode;
}) {
  if (pending) return <LoadingState />;
  if (failed)
    return (
      <div className="card search-layout__notice">
        <p role="alert">Не удалось загрузить заявки.</p>
        <Button variant="secondary" onClick={retry}>
          Повторить
        </Button>
      </div>
    );
  if (empty) return <p className="card role-page__empty">Заявок пока нет.</p>;
  return children;
}
export function MentorApplicationCount() {
  const { data, service } = useSession();
  const query = useApplications(
    data!.id,
    'mentor',
    service.api.mentorApplications,
  );
  return query.data?.length ?? '—';
}

export function StudentApplications() {
  const { data: session, service } = useSession();
  const query = useApplications(
    session!.id,
    'student',
    service.api.studentApplications,
  );
  const [cancel, setCancel] = useState<string>();
  const [details, setDetails] = useState<ParticipantDetails>();
  return (
    <section className="role-page">
      <header>
        <h1>Мои заявки</h1>
        <p>Просматривайте свои заявки и отслеживайте статус.</p>
      </header>
      <ListState
        pending={query.isPending}
        failed={query.isError}
        empty={!query.data?.length}
        retry={() => void query.refetch()}
      >
        {query.data?.map((application) => {
          const reason = application.reasonForRefusal?.trim();
          const participant: ParticipantDetails = {
            form: application.questionnaireForm,
            role: 'mentor',
          };
          return (
            <article className="card application-card" key={application.id}>
              <div className="application-card__columns">
                <Participant
                  {...participant}
                  onDetails={() => setDetails(participant)}
                />
                <ApplicationMatch />
                <section
                  className="application-status"
                  aria-label="Статус заявки"
                >
                  <ol
                    className={`application-status__steps${reason ? ' application-status__steps--refused' : ''}`}
                  >
                    <li>
                      <span aria-hidden="true">{reason ? '×' : '1'}</span>
                      <strong>Наставник</strong>
                      <p>{reason ? 'Отклонено' : 'Рассматривает'}</p>
                    </li>
                    <li>
                      <span aria-hidden="true">2</span>
                      <strong>Куратор</strong>
                      <p>Ожидает</p>
                    </li>
                  </ol>
                  {!reason && (
                    <Button
                      variant="secondary"
                      onClick={() => setCancel(application.id)}
                    >
                      Отменить заявку
                    </Button>
                  )}
                </section>
              </div>
              {reason && (
                <section className="application-card__reason">
                  <h2>Причина отказа</h2>
                  <p>{reason}</p>
                </section>
              )}
            </article>
          );
        })}
      </ListState>
      {cancel && (
        <DecisionDialog
          id={cancel}
          role="student"
          onClose={() => setCancel(undefined)}
        />
      )}
      {details && (
        <ParticipantDialog {...details} onClose={() => setDetails(undefined)} />
      )}
    </section>
  );
}

export function MentorApplications() {
  const { data: session, service } = useSession();
  const query = useApplications(
    session!.id,
    'mentor',
    service.api.mentorApplications,
  );
  const action = useApplicationAction(service.api);
  const [refuse, setRefuse] = useState<string>();
  const [details, setDetails] = useState<ParticipantDetails>();
  return (
    <section className="preview-stack">
      <h2>
        Заявки на наставничество{' '}
        <span className="tag">{query.data?.length ?? '—'} новые</span>
      </h2>
      <ListState
        pending={query.isPending}
        failed={query.isError}
        empty={!query.data?.length}
        retry={() => void query.refetch()}
      >
        {query.data?.map((application) => {
          const participant: ParticipantDetails = {
            form: application.questionnaireForm,
            role: 'student',
          };
          return (
            <article className="card application-card" key={application.id}>
              <Participant
                {...participant}
                onDetails={() => setDetails(participant)}
              />
              <div className="actions">
                <Button
                  disabled={action.isPending}
                  onClick={() =>
                    action.mutate({
                      type: 'approve',
                      role: 'mentor',
                      id: application.id,
                    })
                  }
                >
                  Принять
                </Button>
                <Button
                  variant="secondary"
                  disabled={action.isPending}
                  onClick={() => {
                    action.reset();
                    setRefuse(application.id);
                  }}
                >
                  Отклонить
                </Button>
              </div>
            </article>
          );
        })}
      </ListState>
      {action.isError && (
        <p role="alert">Не удалось принять заявку. {action.error.message}</p>
      )}
      {refuse && (
        <DecisionDialog
          id={refuse}
          role="mentor"
          onClose={() => setRefuse(undefined)}
        />
      )}
      {details && (
        <ParticipantDialog {...details} onClose={() => setDetails(undefined)} />
      )}
    </section>
  );
}

export function CuratorApplications() {
  const { data: session, service } = useSession();
  const query = useApplications(
    session!.id,
    'tutor',
    service.api.curatorApplications,
  );
  const action = useApplicationAction(service.api);
  const [refuse, setRefuse] = useState<string>();
  const [details, setDetails] = useState<ParticipantDetails>();
  return (
    <section className="role-page">
      <header>
        <h1>Очередь на утверждение</h1>
        <p>Рассмотрите заявки на наставничество и примите решение по паре.</p>
      </header>
      <ListState
        pending={query.isPending}
        failed={query.isError}
        empty={!query.data?.length}
        retry={() => void query.refetch()}
      >
        {query.data?.map((application) => {
          const student: ParticipantDetails = {
            form: application.studentQuestionnaireForm,
            role: 'student',
          };
          const mentor: ParticipantDetails = {
            form: application.mentorQuestionnaireForm,
            role: 'mentor',
          };
          return (
            <article className="card application-card" key={application.id}>
              <div className="application-card__columns">
                <Participant
                  {...student}
                  onDetails={() => setDetails(student)}
                />
                <Participant {...mentor} onDetails={() => setDetails(mentor)} />
                <ApplicationMatch />
              </div>
              <div className="approval__actions">
                <Button
                  variant="secondary"
                  disabled={action.isPending}
                  onClick={() => {
                    action.reset();
                    setRefuse(application.id);
                  }}
                >
                  Отклонить
                </Button>
                <Button
                  disabled={action.isPending}
                  onClick={() =>
                    action.mutate({
                      type: 'approve',
                      role: 'tutor',
                      id: application.id,
                    })
                  }
                >
                  Утвердить
                </Button>
              </div>
            </article>
          );
        })}
      </ListState>
      {action.isError && (
        <p role="alert">Не удалось утвердить заявку. {action.error.message}</p>
      )}
      {refuse && (
        <DecisionDialog
          id={refuse}
          role="tutor"
          onClose={() => setRefuse(undefined)}
        />
      )}
      {details && (
        <ParticipantDialog {...details} onClose={() => setDetails(undefined)} />
      )}
    </section>
  );
}
