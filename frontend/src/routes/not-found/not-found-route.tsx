import { Link } from 'react-router';
export function NotFoundRoute() {
  return (
    <section className="panel">
      <p className="eyebrow">404</p>
      <h1>Страница не найдена</h1>
      <p>Проверьте адрес или вернитесь на главную.</p>
      <Link to="/">На главную</Link>
    </section>
  );
}
