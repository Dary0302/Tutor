export function comparisonIds(params: URLSearchParams) {
  return [...new Set(params.getAll('mentor').filter(Boolean))];
}

export function comparisonAddress(path: string, ids: string[]) {
  const query = new URLSearchParams();
  for (const id of [...new Set(ids)]) query.append('mentor', id);
  return query.size ? `${path}?${query}` : path;
}
