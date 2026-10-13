import { Component, inject, signal } from '@angular/core';
import { ClaimsReport } from '../../../core/models/report.model';
import { ReportService } from '../../../core/services/report.service';
import { AuthService } from '../../../core/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-reports',
  imports: [CommonModule],
  templateUrl: './reports.component.html',
  styleUrl: '../../../shared/styles/page.scss',
})
export class ReportsComponent {
  reportService = inject(ReportService);
  authService = inject(AuthService);
  report = signal<ClaimsReport | undefined>(undefined);
  loading = signal<boolean>(true);

  ngOnInit(): void {
    this.reportService.getClaimsReport().subscribe({
      next: (res) =>{
        this.report.set(res.data);
        this.loading.set(false);
      },
      error: (err: any) =>{
        this.loading.set(false);
        console.log(err);
      }
    });
  }

  download(kind: 'excel' | 'pdf'): void {
    const url = kind === 'excel' ? this.reportService.exportExcelUrl() : this.reportService.exportPdfUrl();
    fetch(url, { headers: { Authorization: `Bearer ${this.authService.getToken()}` } })
      .then(res => res.blob())
      .then(blob => {
        const link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = kind === 'excel' ? 'claims-report.xlsx' : 'claims-report.pdf';
        link.click();
      });
  }
}
