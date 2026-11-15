import { Navigate } from 'react-router';
import { useSession } from '@/entities/session';
import { MentorSearch } from '@/features/mentor-search';

export function SearchRoute() {
  const session = useSession();
  if (!session.data) return <Navigate to="/login" replace />;
  if (session.data.role > 1) return <h1>Доступ запрещён</h1>;

  return (
    <>
      <h1 className="page-title">Подбор и поиск</h1>
      <MentorSearch
        userId={session.data.id}
        searchMentors={session.service.api.searchMentors}
      />
    </>
  );
}
