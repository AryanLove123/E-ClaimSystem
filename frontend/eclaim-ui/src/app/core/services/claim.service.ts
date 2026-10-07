import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { ApiResponse } from '../models/api-response.model';
import { Claim } from '../models/claim.model';
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
  
}
