import { useState } from 'react';
import { useForm, useFieldArray } from 'react-hook-form';
import { useQueryClient } from '@tanstack/react-query';
import { Navigate } from 'react-router';
import {
  questionnaireKey,
  useSavedQuestionnaire,
} from '@/entities/questionnaire';
import {
  AvailabilityEditor,
  DurationEditor,
  SkillEditor,
  days,
  levels,
} from './editors';
import { useSession } from '@/entities/session';
import { ApiError } from '@/shared/api';
import type { Questionnaire } from '@/shared/api';
import { Button, Input, Select, LoadingState } from '@/shared/ui';
import { blankQuestionnaire, formSchema } from './model';
import type { FormValues } from './model';

export function QuestionnairePage() {
  const session = useSession();
  const query = useSavedQuestionnaire(
    session.data?.id,
    session.service.api.questionnaire,
    (session.data?.role ?? 2) < 2,
  );
  if (!session.data) return <Navigate to="/login" replace />;
  if (session.data.role > 1)
    return (
      <section>
        <h1>Доступ запрещён</h1>
        <p>Анкета доступна менти и наставникам.</p>
      </section>
    );
  if (query.isPending) return <LoadingState />;
  if (query.isError)
    return (
      <section>
        <h1>Не удалось загрузить анкету</h1>
        <p role="alert">{query.error.message}</p>
        <Button onClick={() => void query.refetch()}>Повторить</Button>
      </section>
    );
  return (
    <QuestionnaireForm
      key={session.data.id}
      initial={query.data}
      mentor={session.data.role === 1}
    />
  );
}

export function QuestionnaireForm({
  initial,
  mentor,
}: {
  initial: Questionnaire | null;
  mentor: boolean;
}) {
  const session = useSession();
  const client = useQueryClient();
  const {
    register,
    control,
    handleSubmit,
    setError,
    setValue,
    clearErrors,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    defaultValues: initial
      ? { ...initial, teacherInfo: initial.teacherInfo ?? null }
      : blankQuestionnaire(mentor),
  });
  const skills = useFieldArray({ control, name: 'skills' });
  const watchedSkills = watch('skills');
  const minimum = watch('monthsNumberMin');
  const maximum = watch('monthsNumberMax');
  const [skillName, setSkillName] = useState('');
  const [skillIndex, setSkillIndex] = useState<number | null>(null);
  const [durationOpen, setDurationOpen] = useState(false);
  const watchedAvailability = watch('availability');
  const [day, setDay] = useState<(typeof days)[number] | null>(null);
  const [notice, setNotice] = useState('');
  const submit = handleSubmit(async (values) => {
    setNotice('');
    const result = formSchema.safeParse(values);
    if (!result.success) {
      for (const issue of result.error.issues)
        setError(issue.path.join('.') as Parameters<typeof setError>[0], {
          message: issue.message,
        });
      return;
    }
    try {
      const { teacherInfo, ...form } = result.data;
      await session.service.api.saveQuestionnaire({
        ...form,
        ...(teacherInfo ? { teacherInfo } : {}),
        communicationFormat: form.communicationFormat as 0 | 1 | 2,
        skills: form.skills.map((skill) => ({
          ...skill,
          skillLevel: skill.skillLevel as 0 | 1 | 2 | 3,
        })),
        availability: form.availability.map((slot) => ({
          ...slot,
          dayOfWeek: slot.dayOfWeek as 0 | 1 | 2 | 3 | 4 | 5 | 6,
          startTime:
            slot.startTime.length === 5
              ? `${slot.startTime}:00`
              : slot.startTime,
          endTime:
            slot.endTime.length === 5 ? `${slot.endTime}:00` : slot.endTime,
        })),
      });
      client.setQueryData(questionnaireKey(session.data?.id), result.data);
      setNotice('Анкета сохранена.');
    } catch (error) {
      if (error instanceof ApiError) {
        for (const [field, messages] of Object.entries(error.fields))
          setError(field as Parameters<typeof setError>[0], {
            message: messages.join(' '),
          });
      }
      setNotice(
        error instanceof Error ? error.message : 'Не удалось сохранить анкету.',
      );
    }
  });
  function addSkill() {
    const name = skillName.trim();
    if (!name) return;
    if (
      watchedSkills.some(
        (skill) => skill.name.toLowerCase() === name.toLowerCase(),
      )
    ) {
      setError('skills', { message: 'Каждый навык укажите один раз' });
      return;
    }
    clearErrors('skills');
    skills.append({ name, skillLevel: 0 }, { shouldFocus: false });
    setSkillName('');
  }
  const activeSkill =
    skillIndex === null ? undefined : watchedSkills[skillIndex];
  const input = (
    name: 'firstName' | 'lastName' | 'city' | 'education' | 'goalDescription',
    label: string,
  ) => (
    <Input
      label={label}
      {...register(name)}
      error={errors[name]?.message}
      maxLength={255}
    />
  );
  return (
    <section className="questionnaire">
      <h1>{mentor ? 'Анкета наставника' : 'Анкета менти'}</h1>
      <form className="questionnaire__form" onSubmit={submit} noValidate>
        <section>
          <h2>1. Основное</h2>
          <div className="form-grid">
            {input('firstName', 'Имя *')}
            {input('lastName', 'Фамилия *')}
            {input('city', 'Город / часовой пояс *')}
            {input(
              'education',
              mentor
                ? 'Образование: вуз, специальность *'
                : 'Вуз, курс, специальность *',
            )}
          </div>
        </section>
        {mentor && (
          <section>
            <h2>2. Профессиональный опыт</h2>
            <div className="form-grid">
              <Input
                label="Место работы *"
                {...register('teacherInfo.placeOfWork')}
                error={errors.teacherInfo?.placeOfWork?.message}
              />
              <Input
                label="Должность *"
                {...register('teacherInfo.position')}
                error={errors.teacherInfo?.position?.message}
              />
              <Input
                label="Сколько лет опыта *"
                type="number"
                min={0}
                {...register('teacherInfo.yearsExperience')}
                error={errors.teacherInfo?.yearsExperience?.message}
              />
              <Input
                label="Максимальное количество учеников *"
                type="number"
                min={1}
                {...register('teacherInfo.maxStudents')}
                error={errors.teacherInfo?.maxStudents?.message}
              />
            </div>
          </section>
        )}
        <section>
          <h2>
            {mentor ? '3. Чем вы можете помочь ученикам' : '2. Цели и интересы'}
          </h2>
          <div className="form-stack">
            {input(
              'goalDescription',
              mentor
                ? 'К каким целям могут прийти ваши ученики? *'
                : 'Опишите цели (срок) *',
            )}
            <div className="questionnaire__skill-input">
              <Input
                label={
                  mentor
                    ? 'Каким навыкам вы можете научить? *'
                    : 'Навыки для развития *'
                }
                placeholder="Введите название навыка"
                value={skillName}
                maxLength={255}
                onChange={(event) => setSkillName(event.target.value)}
                onKeyDown={(event) => {
                  if (event.key === 'Enter') {
                    event.preventDefault();
                    addSkill();
                  }
                }}
              />
              <Button onClick={addSkill} disabled={!skillName.trim()}>
                Добавить
              </Button>
            </div>
            {skills.fields.length ? (
              <ul className="skills">
                {skills.fields.map((field, index) => {
                  const skill = watchedSkills[index] ?? field;
                  return (
                    <li key={field.id} className="skills__tag">
                      <button
                        type="button"
                        className="skills__edit"
                        aria-haspopup="dialog"
                        onClick={() => setSkillIndex(index)}
                      >
                        {skill.name} · {levels[skill.skillLevel]}
                      </button>
                      <button
                        type="button"
                        className="skills__remove"
                        aria-label={`Удалить навык ${skill.name}`}
                        onClick={() => {
                          skills.remove(index);
                          clearErrors('skills');
                        }}
                      >
                        <svg
                          width="14"
                          height="14"
                          viewBox="0 0 16 16"
                          aria-hidden="true"
                        >
                          <path
                            d="m4 4 8 8m0-8-8 8"
                            stroke="currentColor"
                            strokeLinecap="round"
                          />
                        </svg>
                      </button>
                    </li>
                  );
                })}
              </ul>
            ) : (
              <p className="muted">Навыки пока не добавлены</p>
            )}
            {errors.skills?.message && (
              <p className="field__error">{errors.skills.message}</p>
            )}
          </div>
        </section>
        <section>
          <h2>
            {mentor ? '4. Предпочтения в наставничестве' : '3. Предпочтения'}
          </h2>
          <div className="form-grid form-grid--preferences">
            <Select label="Формат" {...register('communicationFormat')}>
              {['Онлайн', 'Очно', 'Гибрид'].map((label, value) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </Select>
            <div className="field">
              <span id="questionnaire-duration">Срок</span>
              <button
                type="button"
                className="field__control questionnaire__duration"
                aria-labelledby="questionnaire-duration questionnaire-duration-value"
                aria-haspopup="dialog"
                onClick={() => setDurationOpen(true)}
              >
                <span id="questionnaire-duration-value">
                  {minimum === maximum
                    ? `${minimum} мес.`
                    : `От ${minimum} до ${maximum} месяцев`}
                </span>
                <span aria-hidden="true">⌄</span>
              </button>
              {(errors.monthsNumberMin?.message ||
                errors.monthsNumberMax?.message) && (
                <p className="field__error">
                  {errors.monthsNumberMin?.message ||
                    errors.monthsNumberMax?.message}
                </p>
              )}
            </div>
            <Input
              label="Часов в неделю"
              type="number"
              min={1}
              max={168}
              {...register('hoursNumberWeek')}
              error={errors.hoursNumberWeek?.message}
            />
          </div>
        </section>
        <section>
          <h2>
            {mentor
              ? '5. Когда вам удобно проводить занятия?'
              : '4. Удобное время для занятий'}
          </h2>
          <p className="muted">
            Выбери день недели, чтобы добавить временные окна
          </p>
          <div className="availability">
            {days.map((entry) => (
              <button
                className="availability__day"
                type="button"
                key={entry[2]}
                aria-haspopup="dialog"
                onClick={() => setDay(entry)}
              >
                <strong>{entry[0]}</strong>
                <span>
                  {watchedAvailability
                    .filter((slot) => slot.dayOfWeek === entry[2])
                    .map(
                      (slot) =>
                        `${slot.startTime.slice(0, 5)}–${slot.endTime.slice(0, 5)}`,
                    )
                    .join(', ') || '+'}
                </span>
              </button>
            ))}
          </div>
          <p className="field__error">{errors.availability?.message}</p>
        </section>
        {notice && <p role="status">{notice}</p>}
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Сохранение…' : 'Сохранить'}
        </Button>
        {mentor && (
          <p className="muted">
            Данные анкеты помогут подобрать подходящих учеников
          </p>
        )}
      </form>
      {skillIndex !== null && activeSkill && (
        <SkillEditor
          name={activeSkill.name}
          current={activeSkill.skillLevel}
          onClose={() => setSkillIndex(null)}
          onSave={(value) => {
            setValue(`skills.${skillIndex}.skillLevel`, value, {
              shouldDirty: true,
            });
            setSkillIndex(null);
          }}
        />
      )}
      {durationOpen && (
        <DurationEditor
          minimum={minimum}
          maximum={maximum}
          onClose={() => setDurationOpen(false)}
          onSave={(from, to) => {
            setValue('monthsNumberMin', from, { shouldDirty: true });
            setValue('monthsNumberMax', to, { shouldDirty: true });
            clearErrors(['monthsNumberMin', 'monthsNumberMax']);
            setDurationOpen(false);
          }}
        />
      )}
      {day && (
        <AvailabilityEditor
          day={day}
          slots={watchedAvailability.filter(
            (slot) => slot.dayOfWeek === day[2],
          )}
          onClose={() => setDay(null)}
          onSave={(slots) => {
            setValue(
              'availability',
              [
                ...watchedAvailability.filter(
                  (slot) => slot.dayOfWeek !== day[2],
                ),
                ...slots,
              ],
              { shouldDirty: true },
            );
            clearErrors('availability');
            setDay(null);
          }}
        />
      )}
    </section>
  );
}
