import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';


export type RequestStatus = 'loading' | 'success' | 'error';
@Component({
  selector: 'app-verify-email',
  imports: [],
  templateUrl: './verify-email.component.html',
  styles: ``,
})
export class VerifyEmailComponent implements OnInit {
  route = inject(ActivatedRoute);
  authService = inject(AuthService);

  status = signal<RequestStatus>('loading');
  message = signal<string | null>(null);

  ngOnInit() {
    const email = this.route.snapshot.queryParamMap.get('email');
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!email || !token) {
      this.status.set('error');
      this.message.set('Missing verification link parameters.');
      return;
    }

    this.authService.verifyEmail({email, token}).subscribe({
      next: (res) => {
        this.status.set('success');
        let susscessMsg = res.message ?? 'Email verified successfully!, You can now log in.';
        this.message.set(susscessMsg);
      },
      error: (err) => {
        this.status.set('error');
        let errMsg = err?.error?.message ?? 'Failed to verify email. Please try again.';
        this.message.set(errMsg);
      },
    });
  }
}
