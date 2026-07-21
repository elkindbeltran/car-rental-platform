import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from './models';

export function apiErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 0) return 'We could not reach the rental service. Check that the API is running and try again.';
    const problem = error.error as ProblemDetails | undefined;
    if (problem?.errors) return Object.values(problem.errors).flat().join(' ');
    return problem?.detail || problem?.title || 'The request could not be completed.';
  }
  return 'Something unexpected happened. Please try again.';
}
