import { Button } from '@/shared/ui';
import type { MentorSearchItem } from '@/shared/api';

const format = new Intl.NumberFormat('ru');
const placesPlural = new Intl.PluralRules('ru');

export function MentorCard({ mentor }: { mentor: MentorSearchItem }) {
  const name = [mentor.firstName, mentor.lastName].filter(Boolean).join(' ');
  const work = [mentor.position, mentor.placeOfWork]
    .filter(Boolean)
    .join(' · ');
  const places = mentor.availablePlaces;
  const plural = placesPlural.select(places);
  const placesLabel =
    plural === 'one' ? 'место' : plural === 'few' ? 'места' : 'мест';

  return (
    <article className="card mentor-card">
      <header className="mentor-card__header">
        <div>
          <h2>{name || 'Наставник'}</h2>
          {work && <p>{work}</p>}
        </div>
        {mentor.compatibility != null && (
          <div className="mentor-card__score">
            <strong className="match">
              {format.format(mentor.compatibility)}%
            </strong>
            <span>совместимость</span>
          </div>
        )}
      </header>
      {!!mentor.skills?.length && (
        <ul className="tags">
          {mentor.skills.map(
            (skill) =>
              skill.name && (
                <li className="tag" key={skill.id}>
                  {skill.name}
                </li>
              ),
          )}
        </ul>
      )}
      <ul className="mentor-card__stats">
        {mentor.rating != null && (
          <li aria-label={`Рейтинг ${format.format(mentor.rating)}`}>
            <svg
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="currentColor"
              aria-hidden="true"
            >
              <path d="m12 2 3 6 7 1-5 5 1 8-6-4-6 4 1-8-5-5 7-1Z" />
            </svg>
            {format.format(mentor.rating)}
          </li>
        )}
        <li>{format.format(mentor.hoursPerWeek)} ч/нед</li>
        <li>
          свободно {format.format(places)} {placesLabel}
        </li>
      </ul>
      <div className="mentor-card__actions">
        <Button disabled>Профиль</Button>
        <Button variant="secondary" disabled>
          Сравнить
        </Button>
      </div>
    </article>
  );
}
