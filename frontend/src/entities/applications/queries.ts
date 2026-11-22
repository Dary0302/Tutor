import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { createApiClient } from '@/shared/api';

type Client = ReturnType<typeof createApiClient>;
export type ApplicationAction =
  | { type: 'send'; mentorId: string }
  | { type: 'cancel'; id: string }
  | { type: 'approve'; role: 'mentor' | 'tutor'; id: string }
  | { type: 'refuse'; role: 'mentor' | 'tutor'; id: string; reason: string };

export function useApplications<T>(
  userId: string,
  scope: string,
  load: (signal?: AbortSignal) => Promise<T>,
) {
  return useQuery({
    queryKey: ['applications', userId, scope],
    queryFn: ({ signal }) => load(signal),
    refetchOnWindowFocus: true,
    refetchInterval: 30_000,
  });
}

export function useApplicationAction(api: Client) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async (action: ApplicationAction) => {
      switch (action.type) {
        case 'send':
          await api.createApplication(action.mentorId);
          break;
        case 'cancel':
          await api.cancelApplication(action.id);
          break;
        case 'approve':
          await api.approveApplication(action.role, action.id);
          break;
        case 'refuse':
          await api.refuseApplication(action.role, action.id, action.reason);
          break;
      }
    },
    retry: false,
    onSuccess: async () => {
      await Promise.all([
        client.invalidateQueries({ queryKey: ['applications'] }),
        client.invalidateQueries({ queryKey: ['mentor-search'] }),
      ]);
    },
  });
}
