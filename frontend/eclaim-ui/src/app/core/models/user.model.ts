export type UserRole = 'Claimant' | 'Adjuster' | 'Approver' | 'Admin';

export interface AuthResponse {
  token: string;
  expiresAt: string;
  userId: number;
  fullName: string;
  email: string;
  role: UserRole;
}

export interface UserProfile {
  id: number;
  fullName: string;
  email: string;
  phoneNumber?: string;
  role: UserRole;
  emailVerified: boolean;
}