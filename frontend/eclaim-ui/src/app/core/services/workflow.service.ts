import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response.model';
import { ClaimWorkflowStatus } from '../models/workflow.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class WorkflowService {
  baseUrl = `${environment.apiUrl}/workflow`;
  constructor(private http: HttpClient){}

  getCLaimStatus(claimId: number): Observable<ApiResponse<ClaimWorkflowStatus>>{
    return this.http.get<ApiResponse<ClaimWorkflowStatus>>(`${this.baseUrl}/claims/${claimId}/status`);
  }
}
