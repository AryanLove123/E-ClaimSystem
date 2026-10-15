import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { ApiResponse } from '../models/api-response.model';
import { Observable } from 'rxjs';
import { NotificationSummary } from '../models/notification.model';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  baseUrl = `${environment.apiUrl}/notification`;

  constructor(private http: HttpClient){}

  getSummary(take = 10): Observable<ApiResponse<NotificationSummary>> {
    return this.http.get<ApiResponse<NotificationSummary>>(this.baseUrl, { params: { take } });
  }

  markAsRead(id: number): Observable<ApiResponse<object>> {
    return this.http.post<ApiResponse<object>>(`${this.baseUrl}/${id}/read`, {});
  }

  markAllAsRead(): Observable<ApiResponse<object>> {
    return this.http.post<ApiResponse<object>>(`${this.baseUrl}/read-all`, {});
  }
}
