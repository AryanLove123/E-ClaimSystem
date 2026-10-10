import { Component, inject, signal } from '@angular/core';
import { ClaimService } from '../../../core/services/claim.service';
import { Claim } from '../../../core/models/claim.model';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  imports: [StatusBadgeComponent, CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styles: ``,
})
export class DashboardComponent {
  claimService = inject(ClaimService);

  claims = signal<Claim[] | undefined>(undefined);
  loading = signal<boolean>(true);
  
  ngOnInit(): void {
    this.claimService.search({ pageNumber: 1, pageSize: 5 }).subscribe({
      next: res => { 
        this.claims.set(res.data.items); 
        this.loading.set(false); 
      },
      error: () => { 
        this.loading.set(false); 
      }
    });
  }

  get pendingCount(): number | undefined { return this.claims()?.filter(c => c.status === 'PendingApproval').length; }

}
