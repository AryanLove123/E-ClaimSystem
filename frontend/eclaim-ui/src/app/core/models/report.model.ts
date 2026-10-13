export interface CountItem { count: number; }
export interface StatusCount extends CountItem { status: string; }
export interface TypeCount extends CountItem { claimType: string; }
export interface SeverityCount extends CountItem { severity: string; }
export interface MonthlyCount extends CountItem { month: string; }

export interface ClaimsReport {
  totalClaims: number;
  totalRequestedAmount: number;
  totalApprovedAmount: number;
  byStatus: StatusCount[];
  byType: TypeCount[];
  bySeverity: SeverityCount[];
  byMonth: MonthlyCount[];
}
