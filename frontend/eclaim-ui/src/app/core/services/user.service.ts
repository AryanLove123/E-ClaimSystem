import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response.model';
import { UserListItem } from '../models/user.model';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  baseUrl = `${environment.apiUrl}/users`;
  constructor(private http: HttpClient){}

  getAll(): Observable<ApiResponse<UserListItem[]>>{
    return this.http.get<ApiResponse<UserListItem[]>>(this.baseUrl);
  }

  setActiveStatus(id: number, isActive: boolean): Observable<ApiResponse<object>> {
    return this.http.put<ApiResponse<object>>(`${this.baseUrl}/${id}/active-status?isActive=${isActive}`, {});
  }
}
