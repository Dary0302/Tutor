import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { useNavigate } from 'react-router';
import { useState } from 'react';
import { useSession } from '@/entities/session';
import { Button, Input } from '@/shared/ui';
import { ApiError } from '@/shared/api';

const schema = z.object({
  email: z.email('Введите корректный email').min(3).max(255),
  password: z
    .string()
    .min(3, 'Минимум 3 символа')
    .max(100)
    .refine(
      (v) => new TextEncoder().encode(v).length <= 72,
      'Не более 72 байт UTF-8',
    ),
  role: z.coerce.number().pipe(z.union([z.literal(0), z.literal(1)])),
  consent: z.boolean(),
});
type Values = z.infer<typeof schema>;
export function AuthForm({
  registerAccount = false,
}: {
  registerAccount?: boolean;
}) {
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<Values>({
    defaultValues: { email: '', password: '', role: 0, consent: false },
  });
  const session = useSession();
  const navigate = useNavigate();
  const [message, setMessage] = useState('');
  const submit = handleSubmit(async (values) => {
    setMessage('');
    const parsed = schema.safeParse(values);
    if (!parsed.success) {
      for (const issue of parsed.error.issues) {
        const field = issue.path[0];
        if (field === 'email' || field === 'password')
          setError(field, { message: issue.message });
      }
      return;
    }
    if (registerAccount && !values.consent) {
      setError('consent', { message: 'Подтвердите принятие соглашения.' });
      return;
    }
    try {
      if (registerAccount) {
        await session.service.publicApi.register({
          email: parsed.data.email,
          password: parsed.data.password,
          role: parsed.data.role,
        });
        navigate('/login', { state: { registered: true }, replace: true });
      } else {
        const profile = await session.login(values.email, values.password);
        navigate(profile.role === 2 ? '/' : '/questionnaire', {
          replace: true,
        });
      }
    } catch (error) {
      setMessage(
        error instanceof ApiError
          ? error.message
          : 'Не удалось связаться с сервером или обработать ответ. Попробуйте ещё раз.',
      );
    }
  });
  return (
    <section className="auth">
      <h1 className="auth__title">
        {registerAccount ? 'Регистрация' : 'Вход'}
      </h1>
      <form className="auth__form" onSubmit={submit} noValidate>
        {registerAccount && (
          <fieldset className="auth__roles">
            <legend>Ваша роль</legend>
            <label>
              <input
                {...register('role', { setValueAs: Number })}
                type="radio"
                value="0"
              />{' '}
              Менти
            </label>
            <label>
              <input
                {...register('role', { setValueAs: Number })}
                type="radio"
                value="1"
              />{' '}
              Наставник
            </label>
          </fieldset>
        )}
        <Input
          label="Email"
          autoComplete="email"
          type="email"
          {...register('email')}
          error={errors.email?.message}
        />
        <Input
          label="Пароль"
          type="password"
          autoComplete={registerAccount ? 'new-password' : 'current-password'}
          {...register('password')}
          error={errors.password?.message}
        />
        {registerAccount ? (
          <>
            <label className="auth__check">
              <input type="checkbox" {...register('consent')} /> Принимаю
              пользовательское соглашение
            </label>
            {errors.consent && (
              <p className="field__error">{errors.consent.message}</p>
            )}
          </>
        ) : (
          <div className="auth__options">
            <label className="auth__check">
              <input type="checkbox" checked disabled /> Запомнить меня
            </label>
            <button className="text-button" type="button" disabled>
              Забыли пароль?
            </button>
          </div>
        )}
        {message && (
          <p role="alert" className="field__error">
            {message}
          </p>
        )}
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting
            ? 'Подождите…'
            : registerAccount
              ? 'Создать аккаунт'
              : 'Войти'}
        </Button>
      </form>
    </section>
  );
}
