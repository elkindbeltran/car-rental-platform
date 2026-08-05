import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => TestBed.configureTestingModule({ imports: [App], providers: [provideRouter([])] }).compileComponents());
  it('creates the application', () => expect(TestBed.createComponent(App).componentInstance).toBeTruthy());
});
