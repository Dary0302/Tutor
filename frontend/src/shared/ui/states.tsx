export function LoadingState() {
  return <p role="status">Загрузка…</p>;
}

export function ErrorState() {
  return (
    <section className="panel">
      <h1>Не удалось открыть страницу</h1>
      <p>Попробуйте ещё раз.</p>
      <a href="/">На главную</a>
    </section>
  );
}
