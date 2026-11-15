import { render, screen } from '@testing-library/react';
import { expect, it } from 'vitest';
import { LoadingState } from './index';
it('communicates a loading state', () => {
  render(<LoadingState />);
  expect(screen.getByRole('status')).toHaveTextContent('Загрузка…');
});
