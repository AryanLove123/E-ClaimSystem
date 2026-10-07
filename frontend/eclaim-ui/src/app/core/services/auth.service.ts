import { computed, Injectable, signal } from '@angular/core';
import { AuthResponse, UserRole } from '../models/user.model';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { ApiResponse } from '../models/api-response.model';
import { environment } from '../../../environments/environment';

interface StoredSession {
  token: string;
  userId: number;
  fullName: string;
  email: string;
  role: UserRole;
  expiresAt: string;
}

const STORAGE_KEY = 'eclaim_session';
const baseUrl = `${environment.apiUrl}/Auth`;
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  sessionSignal = signal<StoredSession | null>(this.readSession());

  isAuthenticated = computed(() => !!this.sessionSignal());
  currentRole = computed(() => this.sessionSignal()?.role ?? null);
  currentUser = computed(() => this.sessionSignal());

  constructor(private http: HttpClient){}

  register(payload: { fullName: string; email: string; password: string; phoneNumber?: string }): Observable<ApiResponse<{userId: number}>> {
    return this.http.post<ApiResponse<{userId: number}>>(`${baseUrl}/register`, payload);
  }

  login(payload: { email: string; password: string }): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${baseUrl}/login`, payload).pipe(
      // On successful login, store the session in localStorage and update the signal
      tap(response => {
        if (response.success && response.data) {
          localStorage.setItem(STORAGE_KEY, JSON.stringify(response.data));
          this.sessionSignal.set(response.data);
        }
      })
    );
  }

  verifyEmail(payload: { email: string, token: string }): Observable<ApiResponse<object>> {
    return this.http.post<ApiResponse<object>>(`${baseUrl}/verify-email`, payload);
  }

  logout(): void{
    localStorage.removeItem(STORAGE_KEY);
    this.sessionSignal.set(null);
  }

  getToken(): string | null {
    const session = this.sessionSignal();
    return session ? session.token : null;
  }

  private readSession(): StoredSession | null {
    const sessionJson = localStorage.getItem(STORAGE_KEY);
    if (!sessionJson) {
      return null;
    }
    try {
      const parsed = JSON.parse(sessionJson) as StoredSession;
      if(new Date(parsed.expiresAt).getTime() < Date.now()) {
        localStorage.removeItem(STORAGE_KEY);
        return null;
      }
      return parsed;
    } catch (error) {
      console.error('Failed to parse session from localStorage', error);
      return null;
    }
  }
}
