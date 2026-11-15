import { Button, Input, Select } from '@/shared/ui';
import { useState } from 'react';
import type { FormEvent } from 'react';
import {
  experienceOptions,
  normalizeFilters,
  ratingOptions,
  validateHours,
  weekdays,
} from './criteria';

export function SearchFilters({
  applied,
  onApply,
}: {
  applied: URLSearchParams;
  onApply: (filters: URLSearchParams) => void;
}) {
  const [errors, setErrors] = useState<{
    hoursFrom?: string;
    hoursTo?: string;
  }>({});

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    const data = new FormData(form);
    const errors = validateHours(form, data);
    setErrors(errors);
    if (errors.hoursFrom || errors.hoursTo) return;
    onApply(normalizeFilters(data));
  }

  return (
    <form
      aria-label="Фильтры"
      className="card search-filters"
      noValidate
      onSubmit={submit}
    >
      <h2>Фильтры</h2>
      <fieldset className="search-filters__group">
        <legend>Навыки</legend>
        <div className="search-filters__skills">
          <Button variant="secondary" disabled>
            Добавить навык
            <svg width="12" height="8" viewBox="0 0 12 8" aria-hidden="true">
              <path d="m1 1 5 5 5-5" fill="none" stroke="currentColor" />
            </svg>
          </Button>
        </div>
      </fieldset>
      <Select
        name="experience"
        label="Опыт"
        defaultValue={applied.get('experience') ?? ''}
      >
        {experienceOptions.map(({ value, label }) => (
          <option key={value} value={value}>
            {label}
          </option>
        ))}
      </Select>
      <fieldset className="search-filters__group">
        <legend>Дни недели</legend>
        <ul className="search-filters__days">
          {weekdays.map(([day, label, name]) => (
            <li key={day}>
              <label className="search-filters__day">
                <input
                  type="checkbox"
                  name="day"
                  value={day}
                  aria-label={name}
                  defaultChecked={applied.getAll('day').includes(String(day))}
                />
                <span>{label}</span>
              </label>
            </li>
          ))}
        </ul>
      </fieldset>
      <Select
        name="places"
        label="Есть свободные места"
        defaultValue={applied.get('places') ?? ''}
      >
        <option value="">Неважно</option>
        <option value="true">Да</option>
        <option value="false">Нет</option>
      </Select>
      <fieldset className="search-filters__group">
        <legend>Часов в неделю</legend>
        <div className="search-filters__range">
          <Input
            label="От"
            name="hoursFrom"
            type="number"
            min={0}
            max={168}
            step="any"
            defaultValue={applied.get('hoursFrom') ?? ''}
            error={errors.hoursFrom}
          />
          <Input
            label="До"
            name="hoursTo"
            type="number"
            min={0}
            max={168}
            step="any"
            defaultValue={applied.get('hoursTo') ?? ''}
            error={errors.hoursTo}
          />
        </div>
      </fieldset>
      <Input
        label="Место работы"
        name="workplace"
        maxLength={255}
        defaultValue={applied.get('workplace') ?? ''}
      />
      <Select
        name="rating"
        label="Мин. рейтинг"
        defaultValue={applied.get('rating') ?? ''}
      >
        {ratingOptions.map(([value, label]) => (
          <option key={value} value={value}>
            {label}
          </option>
        ))}
      </Select>
      <Button className="search-filters__apply" type="submit">
        Применить
      </Button>
    </form>
  );
}
