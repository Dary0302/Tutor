import { useQuery } from '@tanstack/react-query';
import { Link, useSearchParams } from 'react-router';
import { Button, LoadingState } from '@/shared/ui';
import type { createApiClient } from '@/shared/api';
import { MentorCard } from './card';
import { readSearchCriteria, searchAddress, sortOptions } from './criteria';
import { SearchFilters } from './filters';

export function MentorSearch({
  userId,
  searchMentors,
}: {
  userId: string;
  searchMentors: ReturnType<typeof createApiClient>['searchMentors'];
}) {
  const [params, setParams] = useSearchParams();
  const { filters, query, sort, page } = readSearchCriteria(params);
  const results = useQuery({
    queryKey: ['mentor-search', userId, query],
    queryFn: ({ signal }) => searchMentors(query, signal),
  });
  const currentPage = results.data?.page ?? page;
  const totalPages = results.data?.totalPages ?? 0;

  function pageLink(target: number) {
    const search = searchAddress(filters, sort, target).toString();
    return { search: search ? `?${search}` : '' };
  }

  return (
    <div className="search-layout">
      <SearchFilters
        key={filters.toString()}
        applied={filters}
        onApply={(next) => setParams(searchAddress(next, sort))}
      />
      <section className="preview-stack" aria-label="Результаты подбора">
        <select
          className="field__control search-layout__sort"
          aria-label="Сортировка"
          value={sort}
          onChange={(event) =>
            setParams(searchAddress(filters, Number(event.target.value)))
          }
        >
          {sortOptions.map(([value, label]) => (
            <option key={value} value={value}>
              {label}
            </option>
          ))}
        </select>
        {results.isPending && <LoadingState />}
        {results.isError && (
          <div className="card search-layout__notice">
            <p role="alert">Не удалось загрузить наставников.</p>
            <Button variant="secondary" onClick={() => void results.refetch()}>
              Повторить
            </Button>
          </div>
        )}
        {results.isSuccess &&
          (results.data.items?.length ? (
            results.data.items.map((mentor) => (
              <MentorCard key={mentor.userId} mentor={mentor} />
            ))
          ) : (
            <p className="card">Наставники не найдены.</p>
          ))}
        {results.isSuccess && (totalPages > 1 || currentPage > 1) && (
          <nav
            className="search-layout__pages"
            aria-label="Страницы результатов"
          >
            {currentPage > 1 && (
              <Link
                className="button button--secondary"
                to={pageLink(
                  Math.min(currentPage - 1, Math.max(1, totalPages)),
                )}
              >
                Назад
              </Link>
            )}
            <p>
              Страница {currentPage}
              {totalPages >= currentPage && ` из ${totalPages}`}
            </p>
            {currentPage < totalPages && (
              <Link
                className="button button--secondary"
                to={pageLink(currentPage + 1)}
              >
                Дальше
              </Link>
            )}
          </nav>
        )}
      </section>
    </div>
  );
}
