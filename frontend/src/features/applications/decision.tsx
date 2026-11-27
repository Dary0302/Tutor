import { useState } from 'react';
import { useSession } from '@/entities/session';
import { useApplicationAction } from '@/entities/applications';
import { Button, Dialog, Input } from '@/shared/ui';

export function DecisionDialog({
  id,
  role,
  onClose,
}: {
  id: string;
  role: 'student' | 'mentor' | 'tutor';
  onClose: () => void;
}) {
  const { service } = useSession();
  const action = useApplicationAction(service.api);
  const [reason, setReason] = useState('');
  const [error, setError] = useState('');
  const student = role === 'student';
  const curator = role === 'tutor';
  function submit() {
    if (!student && !reason.trim()) {
      setError('Укажите причину.');
      return;
    }
    const request = student
      ? { type: 'cancel' as const, id }
      : { type: 'refuse' as const, role, id, reason: reason.trim() };
    action.mutate(request, { onSuccess: onClose });
  }
  return (
    <Dialog
      size={student ? 'application-cancel' : 'application-refusal'}
      title={
        student
          ? 'Отменить заявку?'
          : curator
            ? 'Отклонение заявки'
            : 'Отмена заявки'
      }
      {...(student && {
        description:
          'Заявка будет отозвана. Вы сможете подать новую заявку к этому или другому наставнику.',
      })}
      onClose={() => {
        if (!action.isPending) onClose();
      }}
    >
      {!student && (
        <Input
          label={curator ? 'Причина отклонения' : 'Причина отмены'}
          placeholder={
            curator ? 'Напишите причину отклонения' : 'Напишите причину отмены'
          }
          value={reason}
          disabled={action.isPending}
          onChange={(event) => {
            setReason(event.target.value);
            setError('');
          }}
          error={error}
        />
      )}
      {action.isError && (
        <p role="alert">
          Не удалось выполнить действие. {action.error.message}
        </p>
      )}
      <div className="application-dialog__actions">
        <Button
          variant="secondary"
          disabled={action.isPending}
          onClick={onClose}
        >
          {curator ? 'Не отклонять' : 'Не отменять'}
        </Button>
        <Button disabled={action.isPending} onClick={submit}>
          {action.isPending
            ? 'Сохранение…'
            : student
              ? 'Да, отменить заявку'
              : curator
                ? 'Отклонить заявку'
                : 'Отменить заявку'}
        </Button>
      </div>
    </Dialog>
  );
}
