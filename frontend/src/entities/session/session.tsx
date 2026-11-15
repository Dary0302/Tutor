import { createContext, useContext, useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { ApiError, createApiClient, profileSchema } from '@/shared/api';
import type { Profile, Session } from '@/shared/api';
import { z } from 'zod';

export const SESSION_KEY = 'kurator.session.v1';
const storedSession = z.object({ refreshToken: z.uuid(), sessionId: z.uuid() });
export class SessionService {
  constructor(private onEnded: () => void = () => {}) {}
  private access: string | null = null;
  private pending: Promise<void> | null = null;
  private generation = 0;
  readonly publicApi = createApiClient();
  readonly api = createApiClient(undefined, {
    token: () => this.access,
    renew: () => this.renew(),
  });
  private read() {
    const value = localStorage.getItem(SESSION_KEY);
    if (!value) return null;
    try {
      const parsed = storedSession.safeParse(JSON.parse(value));
      if (parsed.success) return parsed.data;
    } catch {
      /* Invalid user-owned storage is reset below. */
    }
    localStorage.removeItem(SESSION_KEY);
    return null;
  }
  private accept(session: Session, newLogin = false) {
    try {
      localStorage.setItem(
        SESSION_KEY,
        JSON.stringify({
          refreshToken: session.refreshToken,
          sessionId: newLogin
            ? crypto.randomUUID()
            : (this.read()?.sessionId ?? crypto.randomUUID()),
        }),
      );
    } catch {
      throw new Error(
        'Браузер запретил сохранение входа. Разрешите локальное хранилище.',
      );
    }
    this.access = session.token;
  }
  async login(email: string, password: string): Promise<Profile> {
    this.logout();
    const session = await this.publicApi.login(email, password);
    this.generation++;
    this.accept(session, true);
    try {
      return await this.api.profile();
    } catch (error) {
      this.logout();
      throw error;
    }
  }
  async restore(): Promise<Profile | null> {
    if (!this.read()) return null;
    try {
      await this.renew();
      return await this.api.profile();
    } catch (error) {
      if (error instanceof ApiError && error.status === 401) {
        this.logout();
        return null;
      }
      throw error;
    }
  }
  renew(): Promise<void> {
    if (this.pending) return this.pending;
    const generation = this.generation;
    const action = async () => {
      const refresh = this.read();
      if (!refresh) throw new ApiError(401, 'Войдите в аккаунт.');
      try {
        const session = await this.publicApi.refresh(refresh.refreshToken);
        if (
          this.generation !== generation ||
          this.read()?.sessionId !== refresh.sessionId
        )
          throw new Error('Сессия изменилась. Повторите запрос.');
        this.accept(session);
      } catch (error) {
        if (
          error instanceof ApiError &&
          error.status === 401 &&
          this.generation === generation
        )
          this.logout();
        throw error;
      }
    };
    this.pending = (async () => {
      if (!navigator.locks)
        throw new Error(
          'Браузер не поддерживает безопасное обновление сессии между вкладками.',
        );
      await navigator.locks.request(SESSION_KEY, action);
    })().finally(() => {
      this.pending = null;
    });
    return this.pending;
  }
  logout() {
    this.generation++;
    this.access = null;
    localStorage.removeItem(SESSION_KEY);
    this.onEnded();
  }
  externalChange() {
    this.generation++;
    this.access = null;
  }
}

const Context = createContext<SessionService | null>(null);
export function SessionProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient();
  const clearPrivate = () => {
    queryClient.setQueryData(['session'], null);
    queryClient.removeQueries({
      predicate: (query) => query.queryKey[0] !== 'session',
    });
  };
  const [service] = useState(() => new SessionService(clearPrivate));
  useEffect(() => {
    const listener = (event: StorageEvent) => {
      if (event.key !== SESSION_KEY && event.key !== null) return;
      if (event.newValue && event.oldValue) {
        try {
          if (
            JSON.parse(event.newValue).sessionId ===
            JSON.parse(event.oldValue).sessionId
          )
            return;
        } catch {
          /* A malformed external record requires revalidation. */
        }
      }
      service.externalChange();
      queryClient.setQueryData(['session'], null);
      queryClient.removeQueries({
        predicate: (query) => query.queryKey[0] !== 'session',
      });
      if (event.newValue)
        void queryClient.resetQueries({ queryKey: ['session'] });
    };
    window.addEventListener('storage', listener);
    return () => window.removeEventListener('storage', listener);
  }, [service, queryClient]);
  return <Context value={service}>{children}</Context>;
}
export function useSession() {
  const service = useContext(Context);
  if (!service) throw new Error('SessionProvider is required');
  const queryClient = useQueryClient();
  const query = useQuery({
    queryKey: ['session'],
    queryFn: () => service.restore(),
    retry: false,
    staleTime: Infinity,
  });
  return {
    ...query,
    service,
    logout: () => service.logout(),
    login: async (email: string, password: string) => {
      const profile = profileSchema.parse(await service.login(email, password));
      queryClient.removeQueries({
        predicate: (query) => query.queryKey[0] !== 'session',
      });
      queryClient.setQueryData(['session'], profile);
      return profile;
    },
  };
}
