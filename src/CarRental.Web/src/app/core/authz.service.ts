import { Injectable, inject } from '@angular/core';
import { AuthService } from '@auth0/auth0-angular';
import { map, shareReplay } from 'rxjs';

const ROLE_CLAIM = 'https://car-rental.example.com/roles';

@Injectable({ providedIn: 'root' })
export class AuthzService {
  private readonly auth = inject(AuthService);
  readonly isAdmin$ = this.auth.user$.pipe(
    map(user => {
      const roles = user?.[ROLE_CLAIM];
      return Array.isArray(roles) ? roles.includes('Administrator') : roles === 'Administrator';
    }),
    shareReplay({ bufferSize: 1, refCount: true }),
  );
}
