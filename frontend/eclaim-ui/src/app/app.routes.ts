import { Routes } from '@angular/router';
import { RegisterComponent } from '../app/features/auth/register/register.component';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: 'verify-email',
    loadComponent: () => import('./features/auth/verify-email/verify-email.component').then((m) => m.VerifyEmailComponent),
  },
  {
    path: 'claimant',
    children: [
      {path: 'dashboard', loadComponent: () => import('./features/claimant/dashboard/dashboard.component').then((m) => m.DashboardComponent)},
      {path: 'claims', loadComponent: () => import('./features/claimant/claims-list/claims-list.component').then((m) => m.ClaimsListComponent)},
      {path: 'create-claim', loadComponent: () => import('./features/claimant/create-claim/create-claim.component').then((m) => m.CreateClaimComponent)},
      {path: 'claims/:id', loadComponent: () => import('./features/claimant/claim-detail/claim-detail.component').then((m) => m.ClaimDetailComponent)},
    ]
  },
  {
    path: 'adjuster',
    children: [
      {path: 'dashboard', loadComponent: () => import('./features/adjuster/dashboard/dashboard.component').then((m) => m.DashboardComponent)},
      {path: 'claims/:id', loadComponent: () => import('./features/claimant/claim-detail/claim-detail.component').then((m) => m.ClaimDetailComponent)}
    ]
  },
  {
    path: 'approver',
    children: [
      {path: 'dashboard', loadComponent: () => import('./features/approver/dashboard/dashboard.component').then((m) => m.DashboardComponent)},
      {path: 'claims/:id',loadComponent: () => import('./features/claimant/claim-detail/claim-detail.component').then((m) => m.ClaimDetailComponent)}
    ]
  }
];
