import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ApiResponse, PagedResult } from '../models/api-response.model';
import { Claim, ClaimDocument, ClaimFilter } from '../models/claim.model';
import { Observable } from 'rxjs';

const baseUrl = `${environment.apiUrl}/claims`;
@Injectable({
  providedIn: 'root',
})
export class ClaimService {
  constructor(private http: HttpClient){}

  create(payload: { policyNumber: string; claimType: string; incidentDate: string; description: string; location: string; requestedAmount: number }): Observable<ApiResponse<Claim>> {
    return this.http.post<ApiResponse<Claim>>(baseUrl, payload);
  }

  getById(id: number): Observable<ApiResponse<Claim>> {
    return this.http.get<ApiResponse<Claim>>(`${baseUrl}/${id}`);
  }

  search(filter: Partial<ClaimFilter>): Observable<ApiResponse<PagedResult<Claim>>>{
    let params = new HttpParams();
    Object.entries(filter).forEach(([key, value]) =>{
      if(value !== undefined && value !== null && value != ''){
        params = params.set(key, String(value));
      }
    });
    return this.http.get<ApiResponse<PagedResult<Claim>>>(baseUrl, {params});
  }

  uploadDocument(id: number, file: File):Observable<ApiResponse<ClaimDocument>>{
    const formData = new FormData();
    formData.append('file',file);
    return this.http.post<ApiResponse<ClaimDocument>>(`${baseUrl}/${id}/documents`,formData);
  }

  submit(id:number): Observable<ApiResponse<Claim>>{
    return this.http.post<ApiResponse<Claim>>(`${baseUrl}/${id}/submit`,{});
  }

  requestDocuments(id: number, comments: string): Observable<ApiResponse<Claim>>{
    return this.http.post<ApiResponse<Claim>>(`${baseUrl}/${id}/request-documents`, {comments});
  }

  adjust(id: number, payload: {adjustedAmount: number, comments?: string}): Observable<ApiResponse<Claim>>{
    return this.http.post<ApiResponse<Claim>>(`${baseUrl}/${id}/adjust`,payload);
  }

  resubmitAfterDocuments(id: number): Observable<ApiResponse<Claim>> {
    return this.http.post<ApiResponse<Claim>>(`${baseUrl}/${id}/resubmit-documents`, {});
  }

  completeReview(id: number, comments?: string): Observable<ApiResponse<Claim>> {
    return this.http.post<ApiResponse<Claim>>(`${baseUrl}/${id}/complete-review`, { comments });
  }

  approve(id: number, comments?: string): Observable<ApiResponse<Claim>> {
    return this.http.post<ApiResponse<Claim>>(`${baseUrl}/${id}/approve`, { comments });
  }

  reject(id: number, comments?: string): Observable<ApiResponse<Claim>> {
    return this.http.post<ApiResponse<Claim>>(`${baseUrl}/${id}/reject`, { comments });
  }
}
