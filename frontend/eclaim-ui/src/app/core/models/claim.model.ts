export type ClaimType = 'Vehicle'| 'Health'| 'Property'| 'Travel'
export interface ClaimDocument {
  id: number;
  originalFileName: string;
  contentType: string;
  fileSizeBytes: number;
  createdAt: string;
}

export interface Claim {
  id: number;
  claimNumber: string;
  claimantId: number;
  claimantName: string;
  policyNumber: string;
  claimType: ClaimType;
  incidentDate: string;
  description: string;
  location: string;
  requestedAmount: number;
  adjustedAmount?: number;
  approvedAmount?: number;
  severity: string;
  status: string;
  assignedAdjusterName?: string;
  assignedApproverName?: string;
  createdAt: string;
  updatedAt?: string;
  documents: ClaimDocument[];
}

export interface ClaimFilter {
  claimNumber?: string;
  policyNumber?: string;
  claimType?: ClaimType;
  status?: string;
  severity?: string;
  fromDate?: string;
  toDate?: string;
  pageNumber: number;
  pageSize: number;
}