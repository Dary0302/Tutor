import type { MentorSearchQuery } from '@/shared/api';

export const experienceOptions = [
  { value: '', label: 'Любой' },
  { value: 'up-to-2', label: 'До 2 лет', max: 2 },
  { value: '3-to-5', label: '3–5 лет', min: 3, max: 5 },
  { value: '6-plus', label: '6 лет и больше', min: 6 },
];

export const ratingOptions = [
  ['', 'Любой'],
  ['4', '4+'],
  ['4.5', '4,5+'],
  ['5', '5'],
] as const;

export const weekdays = [
  [1, 'Пн', 'Понедельник'],
  [2, 'Вт', 'Вторник'],
  [3, 'Ср', 'Среда'],
  [4, 'Чт', 'Четверг'],
  [5, 'Пт', 'Пятница'],
  [6, 'Сб', 'Суббота'],
  [0, 'Вс', 'Воскресенье'],
] as const;

export const sortOptions = [
  [0, 'По совместимости'],
  [1, 'По рейтингу'],
  [2, 'По опыту'],
  [3, 'По часам в неделю'],
] as const;

export const pageSize = 10;
const lastPossiblePage = Math.floor(2_147_483_647 / pageSize) + 1;

type Source = FormData | URLSearchParams;

function read(source: Source, name: string) {
  const value = source.get(name);
  return typeof value === 'string' ? value.trim() : '';
}

function readHours(source: Source, name: string) {
  const value = read(source, name);
  return value ? Number(value) : undefined;
}

function validHours(hours: number | undefined) {
  return hours === undefined || (hours >= 0 && hours <= 168);
}

export function normalizeFilters(source: Source) {
  const filters = new URLSearchParams();
  const experience = read(source, 'experience');
  const places = read(source, 'places');
  const rating = read(source, 'rating');
  const workplace = read(source, 'workplace').slice(0, 255);
  const days = source.getAll('day');
  const from = readHours(source, 'hoursFrom');
  const to = readHours(source, 'hoursTo');

  if (
    experience &&
    experienceOptions.some((option) => option.value === experience)
  )
    filters.set('experience', experience);
  if (places === 'true' || places === 'false') filters.set('places', places);
  if (rating && ratingOptions.some(([value]) => value === rating))
    filters.set('rating', rating);
  if (workplace) filters.set('workplace', workplace);
  for (const [day] of weekdays) {
    if (days.includes(String(day))) filters.append('day', String(day));
  }
  if (
    from !== undefined &&
    validHours(from) &&
    (to === undefined || !validHours(to) || from <= to)
  )
    filters.set('hoursFrom', String(from));
  if (
    to !== undefined &&
    validHours(to) &&
    (from === undefined || !validHours(from) || from <= to)
  )
    filters.set('hoursTo', String(to));
  return filters;
}

export function validateHours(
  form: HTMLFormElement,
  data: FormData,
): Partial<Record<'hoursFrom' | 'hoursTo', string>> {
  const from = readHours(data, 'hoursFrom');
  const to = readHours(data, 'hoursTo');
  for (const [name, value] of [
    ['hoursFrom', from],
    ['hoursTo', to],
  ] as const) {
    const input = form.elements.namedItem(name);
    if (
      input instanceof HTMLInputElement &&
      (input.validity.badInput || !validHours(value))
    )
      return { [name]: 'Введите число от 0 до 168.' };
  }
  if (from !== undefined && to !== undefined && from > to)
    return { hoursTo: 'Значение «До» должно быть не меньше значения «От».' };
  return {};
}

export function readSearchCriteria(params: URLSearchParams) {
  const filters = normalizeFilters(params);
  const sort =
    sortOptions.find(([value]) => String(value) === params.get('sort'))?.[0] ??
    0;
  const requestedPage = Number(params.get('page'));
  const page =
    Number.isSafeInteger(requestedPage) &&
    requestedPage > 0 &&
    requestedPage <= lastPossiblePage
      ? requestedPage
      : 1;
  const experience = experienceOptions.find(
    ({ value }) => value === filters.get('experience'),
  );
  const places = filters.get('places');
  const hoursFrom = readHours(filters, 'hoursFrom');
  const hoursTo = readHours(filters, 'hoursTo');
  const workplace = filters.get('workplace');
  const rating = filters.get('rating');
  const selectedDays = filters.getAll('day');
  const days = weekdays
    .map(([day]) => day)
    .filter((day) => selectedDays.includes(String(day)));
  const query: MentorSearchQuery = {
    Page: page,
    PageSize: pageSize,
    Sort: sort,
    ...(experience?.min !== undefined && {
      MinYearsExperience: experience.min,
    }),
    ...(experience?.max !== undefined && {
      MaxYearsExperience: experience.max,
    }),
    ...(places !== null && { HasAvailablePlaces: places === 'true' }),
    ...(hoursFrom !== undefined && { MinHoursPerWeek: hoursFrom }),
    ...(hoursTo !== undefined && { MaxHoursPerWeek: hoursTo }),
    ...(workplace && { PlaceOfWork: workplace }),
    ...(rating !== null && { MinRating: Number(rating) }),
    ...(days.length > 0 && { DaysOfWeek: days }),
  };
  return { filters, query, sort, page };
}

export function searchAddress(
  filters: URLSearchParams,
  sort: number,
  page = 1,
) {
  const params = new URLSearchParams(filters);
  if (sort !== 0) params.set('sort', String(sort));
  if (page > 1) params.set('page', String(page));
  return params;
}
