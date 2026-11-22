import { Dialog, Button } from '@/shared/ui';
import {
  mentorName,
  mentorDuration,
  mentorFormat,
  mentorSlots,
} from '@/entities/mentor';
import type { MentorQuestionnaire } from '@/shared/api';

export type ParticipantDetails = {
  form: MentorQuestionnaire | null | undefined;
  role: 'student' | 'mentor';
};

export function Participant({
  form,
  role,
  onDetails,
}: ParticipantDetails & { onDetails: () => void }) {
  return (
    <section className="application-person">
      <h2>{role === 'student' ? 'Менти' : 'Наставник'}</h2>
      <h3>{form ? mentorName(form) : 'Анкета недоступна'}</h3>
      {form?.teacherInfo && (
        <p>
          {[form.teacherInfo.position, form.teacherInfo.placeOfWork]
            .filter(Boolean)
            .join(' · ')}
        </p>
      )}
      <ul className="tags">
        {form?.skills?.map((skill) => (
          <li className="tag" key={skill.name}>
            {skill.name}
          </li>
        ))}
      </ul>
      {role === 'student' && <p>{form?.goalDescription || '—'}</p>}
      <p>
        {role === 'mentor'
          ? `${form?.teacherInfo?.yearsExperience ?? '—'} лет опыта · ${Math.min(2, form?.teacherInfo?.maxStudents ?? 3)} из ${form?.teacherInfo?.maxStudents ?? 3} учеников`
          : `${form?.hoursNumberWeek ?? '—'} ч в неделю · ${form ? mentorDuration(form) : '—'}`}
      </p>
      <Button variant="secondary" disabled={!form} onClick={onDetails}>
        Подробнее
        <span className="visually-hidden">
          {' '}
          · {form ? mentorName(form) : 'Анкета недоступна'}
        </span>
      </Button>
    </section>
  );
}

export function ParticipantDialog({
  form,
  role,
  onClose,
}: ParticipantDetails & { onClose: () => void }) {
  return (
    <Dialog
      title={role === 'student' ? 'Анкета менти' : 'Анкета наставника'}
      size="details"
      onClose={onClose}
    >
      <h3>{form ? mentorName(form) : 'Анкета недоступна'}</h3>
      <dl className="approval__details">
        <div>
          <dt>Город и часовой пояс</dt>
          <dd>{form?.city || '—'}</dd>
        </div>
        <div>
          <dt>Образование</dt>
          <dd>{form?.education || '—'}</dd>
        </div>
        {form?.teacherInfo && (
          <div>
            <dt>Опыт и нагрузка</dt>
            <dd>
              {form.teacherInfo.yearsExperience} лет · {form.hoursNumberWeek} ч
              в неделю
            </dd>
          </div>
        )}
        <div>
          <dt>
            {role === 'student' ? 'Цель' : 'К каким целям поможет прийти'}
          </dt>
          <dd>{form?.goalDescription || '—'}</dd>
        </div>
        <div>
          <dt>Условия наставничества</dt>
          <dd>
            {form
              ? `${mentorFormat(form)} · ${mentorDuration(form)} · ${form.hoursNumberWeek} ч в неделю`
              : '—'}
          </dd>
        </div>
        <div>
          <dt>Доступное время</dt>
          <dd>{form ? mentorSlots(form).join(' · ') || '—' : '—'}</dd>
        </div>
        <div>
          <dt>Навыки</dt>
          <dd>{form?.skills?.map(({ name }) => name).join(', ') || '—'}</dd>
        </div>
      </dl>
    </Dialog>
  );
}

export function ApplicationMatch() {
  return (
    <section className="application-person application-person--match">
      <h2>Совместимость</h2>
      <strong className="match">92%</strong>
      <h3>Общие слоты</h3>
      <p>Пн, 18:00–19:00 · Ср, 19:00–20:00</p>
      <p className="application-person__duration">Сроки совпадают · 6 мес.</p>
    </section>
  );
}
