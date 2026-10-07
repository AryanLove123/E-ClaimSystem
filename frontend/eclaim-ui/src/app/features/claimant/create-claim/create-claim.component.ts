import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ClaimService } from '../../../core/services/claim.service';
import { ClaimType } from '../../../core/models/claim.model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-create-claim',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-claim.component.html',
  styles: ``,
})
export class CreateClaimComponent {
  fb = inject(FormBuilder);
  claimService = inject(ClaimService);
  router = inject(Router);

  claimTypes: ClaimType[] = ['Vehicle', 'Health', 'Property', 'Travel'];

  form = this.fb.nonNullable.group({
    policyNumber: ['', Validators.required],
    claimType: ['', Validators.required],
    incidentDate: ['', Validators.required],
    description: ['', Validators.required],
    location: ['', Validators.required],
    requestedAmount: [0, [Validators.required, Validators.min(1)]],
  });

  loading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  submit(): void {
    if (this.form.invalid) {
      return;
    }
    this.loading.set(true);
    this.errorMessage.set(null);

    this.claimService.create(this.form.getRawValue()).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.router.navigate(['/claimant/claims', res.data.id]);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Claim not created, Please try again.');
      },
    });
  }
}
