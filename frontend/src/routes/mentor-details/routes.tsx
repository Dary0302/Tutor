import { Navigate, useParams } from 'react-router';
import type { ReactNode } from 'react';
import { useSession } from '@/entities/session';
import { MentorComparison, MentorProfile } from '@/features/mentor-details';

function Access({ children }: { children: ReactNode }) {
  const { data } = useSession();
  if (!data) return <Navigate to="/login" replace />;
  if (data.role > 1) return <h1>Доступ запрещён</h1>;
  return children;
}
export function CompareRoute() {
  return (
    <Access>
      <MentorComparison />
    </Access>
  );
}
export function MentorProfileRoute() {
  const { id = '' } = useParams();
  return (
    <Access>
      <MentorProfile id={id} />
    </Access>
  );
}
