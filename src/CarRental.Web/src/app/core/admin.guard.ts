import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, take } from 'rxjs';
import { AuthzService } from './authz.service';

export const adminGuard: CanActivateFn = () => {
  const authz = inject(AuthzService);
  const router = inject(Router);
  return authz.isAdmin$.pipe(
    take(1),
    map(isAdmin => isAdmin || router.createUrlTree(['/vehicles'])),
  );
};
