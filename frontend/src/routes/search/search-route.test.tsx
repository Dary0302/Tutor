import { act, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { expect, it } from 'vitest';
import { mentorFixture } from '@/test/fixtures/api';
import { server } from '@/test/mocks/server';
import { renderPage } from '@/test/render';

function interceptSearch(respond: (params: URLSearchParams) => Response) {
  const requests: URLSearchParams[] = [];
  server.use(
    http.get('*/api/mentors', ({ request }) => {
      const params = new URL(request.url).searchParams;
      requests.push(params);
      return respond(params);
    }),
  );
  return requests;
}

function result(params: URLSearchParams, items = [mentorFixture], pages = 1) {
  return HttpResponse.json({
    items,
    totalCount: pages > 1 ? pages * 10 : items.length,
    page: Number(params.get('Page')),
    pageSize: 10,
    totalPages: items.length ? pages : 0,
  });
}

it('renders live cards and keeps actions that require future integrations disabled', async () => {
  interceptSearch((params) =>
    HttpResponse.json({
      items: [
        mentorFixture,
        {
          ...mentorFixture,
          userId: '55555555-5555-4555-8555-555555555555',
          firstName: 'Игорь',
          compatibility: null,
          rating: null,
          skills: null,
        },
      ],
      page: Number(params.get('Page')),
      pageSize: 10,
      totalCount: 2,
      totalPages: 1,
    }),
  );
  renderPage('/search', 1);
  const rated = within(
    (await screen.findByRole('heading', { name: 'Мария Петрова' })).closest(
      'article',
    )!,
  );
  expect(rated.getByText('86%')).toBeVisible();
  expect(rated.getByText('TypeScript')).toBeVisible();
  expect(rated.getByText('2,5 ч/нед')).toBeVisible();
  expect(rated.getByRole('button', { name: 'Профиль' })).toBeDisabled();
  expect(rated.getByRole('button', { name: 'Сравнить' })).toBeDisabled();
  const unrated = within(
    screen.getByRole('heading', { name: 'Игорь Петрова' }).closest('article')!,
  );
  expect(unrated.queryByText('совместимость')).not.toBeInTheDocument();
  expect(unrated.queryByLabelText(/Рейтинг/)).not.toBeInTheDocument();
  const filters = within(screen.getByRole('form', { name: 'Фильтры' }));
  expect(
    filters.getByRole('button', { name: 'Добавить навык' }),
  ).toBeDisabled();
  expect(filters.queryByText('Python')).not.toBeInTheDocument();
});

it('keeps filter state when a failed search is retried and returns an empty result', async () => {
  let failed = false;
  const queries = interceptSearch((params) => {
    if (failed) return result(params, []);
    failed = true;
    return new HttpResponse(null, { status: 503 });
  });
  renderPage('/search?places=false', 0);
  const user = userEvent.setup();
  expect(await screen.findByRole('alert')).toHaveTextContent(
    'Не удалось загрузить наставников.',
  );
  expect(screen.getByLabelText('Есть свободные места')).toHaveValue('false');
  await user.click(screen.getByRole('button', { name: 'Повторить' }));
  expect(await screen.findByText('Наставники не найдены.')).toBeVisible();
  expect(queries.map((params) => params.get('HasAvailablePlaces'))).toEqual([
    'false',
    'false',
  ]);
});

it.each([
  [undefined, 'Вход'],
  [2, 'Доступ запрещён'],
])('keeps search unavailable for role %s', async (role, heading) => {
  const queries = interceptSearch((params) => result(params));
  renderPage('/search', role);
  expect(await screen.findByRole('heading', { name: heading })).toBeVisible();
  expect(queries).toHaveLength(0);
});

it('sends each availability value and resets the current page', async () => {
  const queries = interceptSearch((params) =>
    result(params, [mentorFixture], 3),
  );
  const { router } = renderPage('/search?page=2', 0);
  const user = userEvent.setup();
  await screen.findByText('Страница 2 из 3');
  const places = screen.getByLabelText('Есть свободные места');
  await user.selectOptions(places, 'true');
  expect(queries).toHaveLength(1);
  await user.click(screen.getByRole('button', { name: 'Применить' }));
  await screen.findByText('Страница 1 из 3');
  expect(router.state.location.search).toBe('?places=true');
  expect(queries[1]?.get('HasAvailablePlaces')).toBe('true');
  await user.selectOptions(
    screen.getByLabelText('Есть свободные места'),
    'false',
  );
  await user.click(screen.getByRole('button', { name: 'Применить' }));
  await waitFor(() =>
    expect(queries[2]?.get('HasAvailablePlaces')).toBe('false'),
  );
  await user.selectOptions(screen.getByLabelText('Есть свободные места'), '');
  await user.click(screen.getByRole('button', { name: 'Применить' }));
  await waitFor(() => expect(queries).toHaveLength(4));
  expect(queries[3]?.has('HasAvailablePlaces')).toBe(false);
  expect(queries.slice(1).map((query) => query.get('Page'))).toEqual([
    '1',
    '1',
    '1',
  ]);
});

it('validates hours, applies the other filters, and restores the URL state on navigation', async () => {
  const queries = interceptSearch((params) =>
    result(params, [mentorFixture], 3),
  );
  const { router } = renderPage('/search?sort=1&page=2', 0);
  const user = userEvent.setup();
  await screen.findByText('Страница 2 из 3');
  const filters = within(screen.getByRole('form', { name: 'Фильтры' }));
  await user.selectOptions(filters.getByLabelText('Опыт'), '3-to-5');
  await user.click(filters.getByLabelText('Понедельник'));
  await user.click(filters.getByLabelText('Воскресенье'));
  await user.type(filters.getByLabelText('От'), '6');
  await user.type(filters.getByLabelText('До'), '4');
  await user.type(filters.getByLabelText('Место работы'), 'Яндекс');
  await user.selectOptions(filters.getByLabelText('Мин. рейтинг'), '4.5');
  await user.click(filters.getByRole('button', { name: 'Применить' }));
  expect(filters.getByLabelText('До')).toHaveAccessibleDescription(
    'Значение «До» должно быть не меньше значения «От».',
  );
  expect(queries).toHaveLength(1);
  expect(screen.getByText('Страница 2 из 3')).toBeVisible();
  await user.clear(filters.getByLabelText('От'));
  await user.type(filters.getByLabelText('От'), '1.5');
  await user.click(filters.getByRole('button', { name: 'Применить' }));
  await screen.findByText('Страница 1 из 3');
  const applied = queries[1]!;
  expect(Object.fromEntries(applied)).toMatchObject({
    MinYearsExperience: '3',
    MaxYearsExperience: '5',
    MinHoursPerWeek: '1.5',
    MaxHoursPerWeek: '4',
    PlaceOfWork: 'Яндекс',
    MinRating: '4.5',
    Sort: '1',
  });
  expect(applied.getAll('DaysOfWeek')).toEqual(['1', '0']);
  expect(applied.has('DurationMonths')).toBe(false);
  expect(applied.has('SkillIds')).toBe(false);

  await user.click(screen.getByRole('link', { name: 'Дальше' }));
  await screen.findByText('Страница 2 из 3');
  expect(queries[2]?.get('Page')).toBe('2');
  await user.selectOptions(screen.getByLabelText('Сортировка'), '2');
  await screen.findByText('Страница 1 из 3');
  expect(queries[3]?.get('Sort')).toBe('2');
  expect(queries[3]?.get('Page')).toBe('1');
  await act(() => router.navigate(-3));
  expect(await screen.findByText('Страница 2 из 3')).toBeVisible();
  expect(screen.getByLabelText('Сортировка')).toHaveValue('1');
  expect(screen.getByLabelText('Опыт')).toHaveValue('');
  expect(screen.getByLabelText('Понедельник')).not.toBeChecked();
});
