import { useId } from 'react';
import type {
  ButtonHTMLAttributes,
  InputHTMLAttributes,
  ReactNode,
  SelectHTMLAttributes,
} from 'react';

export function Button({
  variant = 'primary',
  className = '',
  type = 'button',
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: 'primary' | 'secondary';
}) {
  return (
    <button
      type={type}
      className={`button button--${variant} ${className}`}
      {...props}
    />
  );
}

export function Card({
  children,
  title,
}: {
  children: ReactNode;
  title: string;
}) {
  const id = useId();
  return (
    <article className="card" aria-labelledby={id}>
      <h3 className="card__title" id={id}>
        {title}
      </h3>
      {children}
    </article>
  );
}

export function Input({
  label,
  error,
  id,
  ...props
}: InputHTMLAttributes<HTMLInputElement> & {
  label: string;
  error?: string | undefined;
}) {
  const generated = useId();
  const fieldId = id ?? generated;
  return (
    <div className="field">
      <label className="field__label" htmlFor={fieldId}>
        {label}
      </label>
      <input
        {...props}
        className="field__control"
        id={fieldId}
        aria-invalid={!!error}
        aria-describedby={error ? `${fieldId}-error` : undefined}
      />
      {error && (
        <span className="field__error" id={`${fieldId}-error`}>
          {error}
        </span>
      )}
    </div>
  );
}

export function Select({
  label,
  children,
  error,
  id,
  ...props
}: SelectHTMLAttributes<HTMLSelectElement> & {
  label: string;
  error?: string | undefined;
}) {
  const generated = useId();
  const fieldId = id ?? generated;
  return (
    <div className="field">
      <label className="field__label" htmlFor={fieldId}>
        {label}
      </label>
      <select
        {...props}
        className="field__control"
        id={fieldId}
        aria-invalid={!!error}
        aria-describedby={error ? `${fieldId}-error` : undefined}
      >
        {children}
      </select>
      {error && (
        <span className="field__error" id={`${fieldId}-error`}>
          {error}
        </span>
      )}
    </div>
  );
}
