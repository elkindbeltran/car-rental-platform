import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-logo',
  template: `<span class="mark" aria-hidden="true"><span></span><span></span></span><span class="wordmark">DRIVE<span>LY</span></span>`,
  styles: [`:host{display:inline-flex;align-items:center;gap:.7rem}.mark{width:2rem;height:2rem;border-radius:.6rem;background:var(--ink);display:grid;place-items:center;position:relative;transform:rotate(-5deg)}.mark span:first-child{width:1rem;height:.48rem;border:2px solid var(--lime);border-radius:.5rem .5rem .2rem .2rem;position:absolute;top:.55rem}.mark span:last-child{width:1.2rem;height:.38rem;border:2px solid var(--lime);border-top:0;border-radius:.1rem .1rem .3rem .3rem;position:absolute;bottom:.45rem}.wordmark{font-weight:850;letter-spacing:-.06em;font-size:1.25rem}.wordmark span{color:var(--accent)}`],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LogoComponent {}
