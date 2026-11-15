import { useLocation } from 'react-router';
import { AuthForm } from '@/features/auth';
export function LoginRoute() {
  const location = useLocation();
  return (
    <>
      {location.state?.registered && (
        <p role="status" className="auth__notice">
          Аккаунт создан. Войдите с вашим email и паролем.
        </p>
      )}
      <AuthForm />
    </>
  );
}
export function RegisterRoute() {
  return <AuthForm registerAccount />;
}
