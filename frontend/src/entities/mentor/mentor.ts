import { useQuery } from '@tanstack/react-query';
import { z } from 'zod';
import { ApiError } from '@/shared/api';
import type { createApiClient, MentorQuestionnaire } from '@/shared/api';

export function mentorQuestionnaireOptions(
  userId: string,
  id: string,
  load: ReturnType<typeof createApiClient>['mentorQuestionnaire'],
) {
  return {
    queryKey: ['mentor-questionnaire', userId, id],
    queryFn: async ({ signal }: { signal: AbortSignal }) => {
      if (!z.uuid().safeParse(id).success)
        throw new ApiError(404, 'Наставник не найден.');
      const form = await load(id, signal);
      if (!form.teacherInfo)
        throw new ApiError(404, 'Анкета наставника не найдена.');
      return form;
    },
    retry: (attempt: number, error: Error) =>
      !(error instanceof ApiError && error.status === 404) && attempt < 1,
  };
}
export function useMentorQuestionnaire(
  ...args: Parameters<typeof mentorQuestionnaireOptions>
) {
  return useQuery(mentorQuestionnaireOptions(...args));
}

export function mentorName(form: MentorQuestionnaire) {
  return (
    [form.firstName, form.lastName].filter(Boolean).join(' ') || 'Наставник'
  );
}
export function mentorDuration(form: MentorQuestionnaire) {
  return `От ${form.monthsNumberMin} до ${form.monthsNumberMax} месяцев`;
}
export function mentorFormat(form: MentorQuestionnaire) {
  return ['Онлайн', 'Очно', 'Очно и онлайн'][form.communicationFormat] ?? '—';
}
export function mentorSlots(form: MentorQuestionnaire) {
  const days = ['Вс', 'Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб'];
  return (
    form.availability?.map(
      (slot) =>
        `${days[slot.dayOfWeek]} ${slot.startTime.slice(0, 5)}–${slot.endTime.slice(0, 5)}`,
    ) ?? []
  );
}
