import { Component, inject, OnInit, signal } from '@angular/core';
import { ClaimsReport } from '../../../core/models/report.model';
import { ReportService } from '../../../core/services/report.service';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: '../../../shared/styles/page.scss',
})
export class DashboardComponent implements OnInit {
  reportService = inject(ReportService);
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
  
}
