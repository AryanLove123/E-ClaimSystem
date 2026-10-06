import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styles: ``,
})
export class LoginComponent {
  fb = inject(FormBuilder);
  authService = inject(AuthService);
  router = inject(Router);

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  loading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  submit() {
    if (this.form.invalid) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    this.authService.login(this.form.getRawValue()).subscribe({
      next: (response) => {
        if (response.success) {
          const role = response.data?.role;
          this.loading.set(false);
          const routeByRole: Record<string, string> = {
            'Claimant': '/claimant/dashboard',
            'Adjuster': '/adjuster/dashboard',
            'Approver': '/approver/dashboard',
            'Admin': '/admin/dashboard',
          };
          this.router.navigate([routeByRole[role] ?? '/']);
        }
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'An error occurred. Please try again.');
        this.loading.set(false);
      },
    });
  }
}
