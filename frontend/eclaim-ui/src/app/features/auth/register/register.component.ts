import { Component, inject, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  imports: [],
  templateUrl: './register.component.html',
  styles: ``,
})
export class RegisterComponent {
  fb = inject(FormBuilder);
  authService = inject(AuthService);
  router = inject(Router);

  form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    phoneNumber: [''],
  });

  loading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  submit() {
    if (this.form.invalid) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.authService.register(this.form.getRawValue()).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage.set('Registration successful! Please check your email for verification.');
          this.loading.set(false);
          this.form.reset();
        }
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Registration failed. Please try again.');
        this.loading.set(false);
      },
    });
  } 
}
