import { useState } from 'react';
import { Button, Dialog, Input, Select } from '@/shared/ui';
import type { FormValues } from './model';

export const levels = ['Начальный', 'Средний', 'Продвинутый', 'Экспертный'];
export const days = [
  ['Пн', 'Понедельник', 1],
  ['Вт', 'Вторник', 2],
  ['Ср', 'Среда', 3],
  ['Чт', 'Четверг', 4],
  ['Пт', 'Пятница', 5],
  ['Сб', 'Суббота', 6],
  ['Вс', 'Воскресенье', 0],
] as const;
type Slot = FormValues['availability'][number];
type Close = { onClose: () => void };

export function SkillEditor({
  name,
  current,
  onSave,
  onClose,
}: Close & {
  name: string;
  current: number;
  onSave: (value: number) => void;
}) {
  const [level, setLevel] = useState(current);
  return (
    <Dialog title="Уровень навыка" description={name} onClose={onClose}>
      <Select
        label="Уровень"
        value={level}
        onChange={(event) => setLevel(Number(event.target.value))}
      >
        {levels.map((label, value) => (
          <option key={label} value={value}>
            {label}
          </option>
        ))}
      </Select>
      <Button onClick={() => onSave(level)}>Сохранить</Button>
    </Dialog>
  );
}

export function DurationEditor({
  minimum,
  maximum,
  onSave,
  onClose,
}: Close & {
  minimum: number;
  maximum: number;
  onSave: (minimum: number, maximum: number) => void;
}) {
  const [from, setFrom] = useState(String(minimum));
  const [to, setTo] = useState(String(maximum));
  const [error, setError] = useState<string>();
  function save() {
    const low = Number(from);
    const high = Number(to);
    if (
      !Number.isInteger(low) ||
      !Number.isInteger(high) ||
      low < 1 ||
      high < low
    ) {
      setError(
        'Введите целые месяцы. Срок «До» должен быть не меньше срока «От».',
      );
      return;
    }
    onSave(low, high);
  }
  return (
    <Dialog
      title="Длительность"
      description="Выбери предпочтительный срок"
      onClose={onClose}
    >
      <div className="form-grid">
        <Input
          label="Срок от (мес.)"
          type="number"
          min={1}
          value={from}
          onChange={(event) => setFrom(event.target.value)}
        />
        <Input
          label="Срок до (мес.)"
          type="number"
          min={1}
          value={to}
          onChange={(event) => setTo(event.target.value)}
          error={error}
        />
      </div>
      <Button onClick={save}>Сохранить</Button>
    </Dialog>
  );
}

type DraftSlot = Slot & { id: number };
const normalizeTime = (value: string) =>
  value.length === 5 ? `${value}:00` : value;

export function AvailabilityEditor({
  day,
  slots,
  onSave,
  onClose,
}: Close & {
  day: (typeof days)[number];
  slots: Slot[];
  onSave: (slots: Slot[]) => void;
}) {
  const [entries, setEntries] = useState<DraftSlot[]>(() =>
    slots.map((slot, id) => ({ ...slot, id })),
  );
  const [nextId, setNextId] = useState(slots.length);
  const [error, setError] = useState('');
  function update(id: number, key: 'startTime' | 'endTime', value: string) {
    setEntries((current) =>
      current.map((slot) =>
        slot.id === id ? { ...slot, [key]: value } : slot,
      ),
    );
  }
  function save() {
    const sorted = entries
      .map(({ dayOfWeek, startTime, endTime }) => ({
        dayOfWeek,
        startTime: normalizeTime(startTime),
        endTime: normalizeTime(endTime),
      }))
      .sort((a, b) => a.startTime.localeCompare(b.startTime));
    if (
      sorted.some(
        (slot, index) =>
          !slot.startTime ||
          !slot.endTime ||
          slot.startTime >= slot.endTime ||
          (sorted[index - 1]?.endTime ?? '') > slot.startTime,
      )
    ) {
      setError(
        'Проверьте время начала и окончания. Временные окна не должны пересекаться.',
      );
      return;
    }
    onSave(sorted);
  }
  return (
    <Dialog
      title={day[1]}
      description="Добавь удобное время для встреч"
      size="wide"
      onClose={onClose}
    >
      {entries.map((entry) => (
        <div key={entry.id} className="availability__row">
          <Input
            label="Начало"
            type="time"
            step={1}
            value={entry.startTime}
            onChange={(event) =>
              update(entry.id, 'startTime', event.target.value)
            }
          />
          <Input
            label="Окончание"
            type="time"
            step={1}
            value={entry.endTime}
            onChange={(event) =>
              update(entry.id, 'endTime', event.target.value)
            }
          />
          <Button
            variant="secondary"
            aria-label="Удалить окно"
            onClick={() =>
              setEntries((current) =>
                current.filter((slot) => slot.id !== entry.id),
              )
            }
          >
            Удалить
          </Button>
        </div>
      ))}
      <Button
        className="dialog__add"
        variant="secondary"
        onClick={() => {
          setEntries((current) => [
            ...current,
            {
              id: nextId,
              dayOfWeek: day[2],
              startTime: '18:00',
              endTime: '20:00',
            },
          ]);
          setNextId(nextId + 1);
        }}
      >
        + Добавить окно
      </Button>
      {error && (
        <p role="alert" className="field__error">
          {error}
        </p>
      )}
      <Button onClick={save}>Сохранить</Button>
    </Dialog>
  );
}
