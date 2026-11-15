import { useQuery } from '@tanstack/react-query';
import { ApiError } from '@/shared/api';
import type { Questionnaire } from '@/shared/api';

export const questionnaireKey = (id?: string) => ['questionnaire', id] as const;

export function useSavedQuestionnaire(
  id: string | undefined,
  load: () => Promise<Questionnaire>,
  enabled: boolean,
) {
  return useQuery({
    queryKey: questionnaireKey(id),
    queryFn: async () => {
      try {
        return await load();
      } catch (error) {
        if (error instanceof ApiError && error.status === 404) return null;
        throw error;
      }
    },
    enabled: !!id && enabled,
    retry: false,
    staleTime: Infinity,
  });
}
