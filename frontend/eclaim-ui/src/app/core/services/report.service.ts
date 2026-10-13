import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response.model';
import { ClaimsReport } from '../models/report.model';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class ReportService {
  baseUrl = `${environment.apiUrl}/reports`;

  constructor(private http: HttpClient) {}

  getClaimsReport(): Observable<ApiResponse<ClaimsReport>> {
    return this.http.get<ApiResponse<ClaimsReport>>(`${this.baseUrl}/claims`);
  }

  exportExcelUrl(): string {
    return `${this.baseUrl}/claims/export/excel`;
  }

  exportPdfUrl(): string {
    return `${this.baseUrl}/claims/export/pdf`;
  }
}
