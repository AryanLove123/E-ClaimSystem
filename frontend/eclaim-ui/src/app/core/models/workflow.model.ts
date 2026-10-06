export interface WorkflowStep {
  id: number;
  stepOrder: number;
  name: string;
  responsibleRole: string;
  allowReject: boolean;
}

export interface Workflow {
  id: number;
  name: string;
  description: string;
  claimType?: string;
  minAmount?: number;
  maxAmount?: number;
  severity?: string;
  priority: number;
  isActive: boolean;
  steps: WorkflowStep[];
}

export interface ClaimWorkflowStepStatus {
  stepOrder: number;
  name: string;
  responsibleRole: string;
  status: string;
  assignedToName?: string;
  completedAt?: string;
}

export interface ClaimWorkflowStatus {
  workflowName: string;
  status: string;
  steps: ClaimWorkflowStepStatus[];
}
