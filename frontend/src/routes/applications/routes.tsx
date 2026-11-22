import { Navigate } from 'react-router';
import { useSession } from '@/entities/session';
import {
  CuratorApplications,
  StudentApplications,
} from '@/features/applications';

export function ApplicationsRoute() {
  const { data } = useSession();
  if (!data) return <Navigate to="/login" replace />;
  if (data.role !== 0) return <h1>Доступ запрещён</h1>;
  return <StudentApplications />;
}
export function ApprovalQueueRoute() {
  const { data } = useSession();
  if (!data) return <Navigate to="/login" replace />;
  if (data.role !== 2) return <h1>Доступ запрещён</h1>;
  return <CuratorApplications />;
}
