import { CommonModule } from '@angular/common';
import { Component, input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `<span class="badge" [ngClass]="statusClass">{{ status() }}</span>`,
  styles: [
    `
      .badge {
        padding: 0.2rem 0.6rem;
        border-radius: 999px;
        font-size: 0.75rem;
        font-weight: 600;
        white-space: nowrap;
      }
      .badge.neutral {
        background: #e2e8f0;
        color: #334155;
      }
      .badge.info {
        background: #dbeafe;
        color: #1e40af;
      }
      .badge.warn {
        background: #fef3c7;
        color: #92400e;
      }
      .badge.success {
        background: #dcfce7;
        color: #166534;
      }
      .badge.danger {
        background: #fee2e2;
        color: #991b1b;
      }
    `,
  ],
})
export class StatusBadgeComponent {
  status = input.required<string>();

  get statusClass(): string {
    switch (this.status().toLowerCase()) {
      case 'approved':
      case 'paid':
        return 'status-success';

      case 'rejected':
        return 'status-error';

      case 'additionaldocumentsrequired':
      case 'pendingapproval':
      case 'paymentpending':
      case 'underreview':
      case 'underadjustment':
        return 'status-warning';

      case 'submitted':
        return 'status-info';

      case 'draft':
        return 'status-neutral';

      default:
        return 'status-neutral';
    }
  }
}
