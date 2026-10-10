import { Component, inject, OnInit, signal } from '@angular/core';
import { Claim, ClaimType } from '../../../core/models/claim.model';
import { ClaimService } from '../../../core/services/claim.service';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-claims-list',
  imports: [FormsModule, RouterLink,StatusBadgeComponent, CommonModule],
  templateUrl: './claims-list.component.html',
  styles: ``,
})
export class ClaimsListComponent implements OnInit {
  claimService = inject(ClaimService);
  claims = signal<Claim[] | undefined>(undefined);
  loading = signal<boolean>(true);
  statusFilter = signal<string | ''>('');
  typeFilter = signal<ClaimType | ''>('');

  statuses = ['Draft', 'Submitted', 'UnderReview', 'AdditionalDocumentsRequired', 'UnderAdjustment', 'PendingApproval', 'Approved', 'Rejected', 'PaymentPending', 'PaymentProcessing', 'Paid', 'PaymentFailed'];
  types: ClaimType[] = ['Vehicle', 'Health', 'Property', 'Travel'];
  
  ngOnInit(): void {
    this.load();
  }

  load(): void{
    this.loading.set(true);
    this.claimService.search({
      pageNumber: 1, pageSize: 50,
      status: this.statusFilter() || undefined,
      claimType: this.typeFilter() || undefined
    }).subscribe({
      next: (res) => {
        this.claims.set(res.data.items);
        this.loading.set(false);
      },
      error: (err) =>{
        this.loading.set(false);
      }
    });
  }

  onStatusChange(status: string): void {
    this.statusFilter.set(status);
    this.load();
  }

  onTypeChange(type: ClaimType | ''): void {
    this.typeFilter.set(type);
    this.load();
  }
}
