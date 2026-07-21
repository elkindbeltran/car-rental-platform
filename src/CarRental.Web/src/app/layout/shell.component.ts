import { AsyncPipe, NgOptimizedImage } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '@auth0/auth0-angular';
import { AuthzService } from '../core/authz.service';
import { LogoComponent } from '../shared/logo.component';

@Component({
  selector: 'app-shell',
  imports: [AsyncPipe, NgOptimizedImage, RouterLink, RouterLinkActive, RouterOutlet, LogoComponent],
  template: `
    <div class="app-shell">
      <header class="topbar">
        <a routerLink="/vehicles" class="brand" aria-label="Drively home"><app-logo /></a>
        <nav [class.open]="menuOpen()" aria-label="Main navigation">
          <a routerLink="/vehicles" routerLinkActive="active" (click)="closeMenu()"><span>⌁</span> Fleet</a>
          <a routerLink="/book" routerLinkActive="active" (click)="closeMenu()"><span>＋</span> New booking</a>
          @if (authz.isAdmin$ | async) {
            <a routerLink="/customers" routerLinkActive="active" (click)="closeMenu()"><span>◉</span> Customers</a>
          }
        </nav>
        <div class="user-area">
          @if (auth.user$ | async; as user) {
            <div class="user-copy"><strong>{{ user.name || 'Driver' }}</strong><span>{{ (authz.isAdmin$ | async) ? 'Administrator' : 'Member' }}</span></div>
            @if (user.picture) { <img [ngSrc]="user.picture" width="38" height="38" alt="User avatar" /> }
            @else { <span class="avatar">{{ (user.name || 'D').charAt(0) }}</span> }
            <button class="icon-button logout" (click)="logout()" title="Sign out" aria-label="Sign out">↗</button>
          }
          <button class="menu-button" (click)="menuOpen.set(!menuOpen())" aria-label="Toggle navigation">☰</button>
        </div>
      </header>
      <main><router-outlet /></main>
      <footer><app-logo /><span>Simple journeys. Memorable drives.</span><span>© 2026 Drively</span></footer>
    </div>`,
  styleUrl: './shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShellComponent {
  readonly auth = inject(AuthService);
  readonly authz = inject(AuthzService);
  readonly menuOpen = signal(false);
  closeMenu() { this.menuOpen.set(false); }
  logout() { this.auth.logout({ logoutParams: { returnTo: window.location.origin } }); }
}
