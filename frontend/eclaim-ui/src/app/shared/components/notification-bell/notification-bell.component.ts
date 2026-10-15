import { Component, computed, DestroyRef, ElementRef, HostListener, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { timer, switchMap, catchError, EMPTY } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AppNotification } from '../../../core/models/notification.model';
import { CommonModule } from '@angular/common';

const POLL_INTERVAL_MS = 15_000;
const PAGE_SIZE = 10;
@Component({
  selector: 'app-notification-bell',
  imports: [CommonModule],
  templateUrl: './notification-bell.component.html',
  styleUrl: './notification-bell.component.scss',
})
export class NotificationBellComponent implements OnInit {
  private notificationService = inject(NotificationService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private host = inject<ElementRef<HTMLElement>>(ElementRef);
  private destroyRef = inject(DestroyRef);

  items = signal<AppNotification[]>([]);
  unreadCount = signal(0);
  open = signal(false);
  badgeText = computed(() => (this.unreadCount() > 99 ? '99+' : String(this.unreadCount())));

  ngOnInit(): void {
    timer(0, POLL_INTERVAL_MS)
      .pipe(
        switchMap(() => this.notificationService.getSummary(PAGE_SIZE).pipe(catchError(() => EMPTY))),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(res => this.apply(res.data));
  }

  toggle(): void {
    this.open.update(v => !v);
    if (this.open()) this.refreshNow();
  }

  markAllAsRead(): void {
    if (this.unreadCount() === 0) return;
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.items.update(list => list.map(n => ({ ...n, isRead: true })));
        this.unreadCount.set(0);
      }
    });
  }

  onItemClick(n: AppNotification): void {
    if (!n.isRead) {
      this.notificationService.markAsRead(n.id).subscribe({
        next: () => {
          this.items.update(list => list.map(x => (x.id === n.id ? { ...x, isRead: true } : x)));
          this.unreadCount.update(c => Math.max(0, c - 1));
        }
      });
    }

    this.open.set(false);
    const target = this.routeFor(n);
    if (target) this.router.navigate(target);
  }

  timeAgo(iso: string): string {
    const seconds = Math.max(0, Math.floor((Date.now() - this.parseUtc(iso).getTime()) / 1000));
    if (seconds < 60) return 'just now';
    const minutes = Math.floor(seconds / 60);
    if (minutes < 60) return `${minutes}m ago`;
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours}h ago`;
    return `${Math.floor(hours / 24)}d ago`;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.open() && !this.host.nativeElement.contains(event.target as Node)) this.open.set(false);
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.open.set(false);
  }

  private refreshNow(): void {
    this.notificationService.getSummary(PAGE_SIZE).pipe(catchError(() => EMPTY)).subscribe(res => this.apply(res.data));
  }

  private apply(data: { unreadCount: number; items: AppNotification[] }): void {
    this.items.set(data.items);
    this.unreadCount.set(data.unreadCount);
  }

  private routeFor(n: AppNotification): (string | number)[] | null {
    if (!n.claimId) return null;
    switch (this.authService.currentRole()) {
      case 'Claimant': return ['/claimant/claims', n.claimId];
      case 'Adjuster': return ['/adjuster/claims', n.claimId];
      case 'Approver': return ['/approver/claims', n.claimId];
      default: return null;
    }
  }

  private parseUtc(iso: string): Date {
    return new Date(/[zZ]$|[+-]\d{2}:?\d{2}$/.test(iso) ? iso : iso + 'Z');
  }
}
