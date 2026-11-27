import { Link } from 'react-router';
import { useSession } from '@/entities/session';
import { useApplicationAction } from '@/entities/applications';
import { Button } from '@/shared/ui';

export function SendApplication({ mentorId }: { mentorId: string }) {
  const { data, service } = useSession();
  const action = useApplicationAction(service.api);
  return (
    <div className="application-send">
      <Button
        disabled={data?.role !== 0 || action.isPending || action.isSuccess}
        onClick={() => action.mutate({ type: 'send', mentorId })}
      >
        {action.isPending
          ? 'Отправка…'
          : action.isSuccess
            ? 'Заявка отправлена'
            : 'Отправить заявку'}
      </Button>
      {action.isSuccess && (
        <p role="status">
          Заявка отправлена. <Link to="/applications">Мои заявки</Link>
        </p>
      )}
      {action.isError && (
        <p role="alert">Не удалось отправить заявку. {action.error.message}</p>
      )}
    </div>
  );
}
