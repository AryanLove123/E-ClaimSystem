import { Component, inject, OnInit, signal } from '@angular/core';
import { Claim } from '../../../core/models/claim.model';
import { ClaimService } from '../../../core/services/claim.service';
import { AuthService } from '../../../core/services/auth.service';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, StatusBadgeComponent, RouterLink],
  templateUrl: './dashboard.component.html',
  styles: ``,
})
export class DashboardComponent implements OnInit {
  claims = signal<Claim[] | undefined>(undefined);
  loading = signal<boolean>(true);

  claimService = inject(ClaimService);
  authService = inject(AuthService);

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

  get activeCount(): number | undefined { return this.claims()?.filter(c => !['Approved', 'Rejected', 'Paid'].includes(c.status)).length; }
  get approvedCount(): number | undefined { return this.claims()?.filter(c => ['Approved', 'Paid'].includes(c.status)).length; }
}
