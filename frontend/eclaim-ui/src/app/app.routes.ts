import { Routes } from '@angular/router';
import { RegisterComponent } from '../app/features/auth/register/register.component';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('../app/features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('../app/features/auth/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: 'verify-email',
    loadComponent: () => import('../app/features/auth/verify-email/verify-email.component').then((m) => m.VerifyEmailComponent),
  }
];
