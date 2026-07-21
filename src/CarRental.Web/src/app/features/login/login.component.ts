import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AuthService } from '@auth0/auth0-angular';
import { LogoComponent } from '../../shared/logo.component';

@Component({
  selector: 'app-login',
  imports: [LogoComponent],
  template: `
    <div class="login-page">
      <header><app-logo /><span>Premium car rental, made effortless.</span></header>
      <main>
        <section class="hero-copy">
          <span class="eyebrow">THE ROAD IS YOURS</span>
          <h1>Pick a car.<br><em>Make it a story.</em></h1>
          <p>A seamless rental experience built around your journey—not paperwork.</p>
          <button class="button primary large" (click)="login()">Explore the fleet <span>→</span></button>
          <div class="proof"><span>4.9 <small>★★★★★</small></span><span>24/7 <small>Roadside care</small></span><span>2 min <small>Average booking</small></span></div>
        </section>
        <section class="visual" aria-label="A modern road-trip illustration">
          <div class="sun"></div><div class="hill hill-one"></div><div class="hill hill-two"></div><div class="road"></div>
          <div class="car"><div class="car-top"></div><div class="car-body"></div><i></i><i></i></div>
          <div class="floating-card"><span>YOUR NEXT DRIVE</span><strong>Feels this good.</strong><small>No hidden fees · Instant confirmation</small></div>
        </section>
      </main>
      <footer><span>Secure sign-in powered by Auth0</span><span>© 2026 Drively</span></footer>
    </div>`,
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  login() { this.auth.loginWithRedirect({ appState: { target: '/vehicles' } }); }
}
