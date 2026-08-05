import { Routes } from '@angular/router';
import { AuthGuard } from '@auth0/auth0-angular';
import { adminGuard } from './core/admin.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent), title: 'Welcome · Drively' },
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  {
    path: '', canActivate: [AuthGuard], loadComponent: () => import('./layout/shell.component').then(m => m.ShellComponent),
    children: [
      { path: 'vehicles', loadComponent: () => import('./features/vehicles/vehicles.component').then(m => m.VehiclesComponent), title: 'Fleet · Drively' },
      { path: 'book', loadComponent: () => import('./features/booking/booking.component').then(m => m.BookingComponent), title: 'New booking · Drively' },
      { path: 'customers', canActivate: [adminGuard], loadComponent: () => import('./features/customers/customers.component').then(m => m.CustomersComponent), title: 'Customers · Drively' },
      { path: '', pathMatch: 'full', redirectTo: 'vehicles' },
    ],
  },
  { path: '**', redirectTo: 'vehicles' },
];
