import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ClaimService } from '../../../core/services/claim.service';
import { AuthService } from '../../../core/services/auth.service';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Claim } from '../../../core/models/claim.model';
import { CommonModule } from '@angular/common';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { FormsModule } from '@angular/forms';
import { ClaimWorkflowStatus } from '../../../core/models/workflow.model';
import { WorkflowService } from '../../../core/services/workflow.service';

@Component({
  selector: 'app-claim-detail',
  imports: [CommonModule, StatusBadgeComponent, FormsModule,RouterLink],
  templateUrl: './claim-detail.component.html',
  styleUrl: '../page.scss',
})
export class ClaimDetailComponent implements OnInit {
  claimService = inject(ClaimService);
  authService = inject(AuthService);
  workflowService = inject(WorkflowService);

  route = inject(ActivatedRoute);

  claim = signal<Claim | undefined>(undefined);
  workflowStatus = signal<ClaimWorkflowStatus | undefined>(undefined);

  loading = signal<boolean>(true);
  actionMessage = signal<string>('');
  actionError = signal<string>('');
  loadError = signal<string>('');

  adjustAmount = signal<number>(0);
  comments = signal<string>('');
  selectedFile = signal<File | undefined>(undefined);

  claimId = computed(() => Number(this.route.snapshot.paramMap.get('id')));
  isOwner = computed(() => this.authService.currentRole() === 'Claimant');
  isAdjuster = computed(() => this.authService.currentRole() === 'Adjuster');
  isApprover = computed(() => this.authService.currentRole() === 'Approver');

  ngOnInit(): void {
    this.reload();
  }

  reload() : void{
    this.loading.set(true);
    this.loadError.set('');
    this.claimService.getById(this.claimId()).subscribe({
      next: (res) =>{
        this.loading.set(false);
        this.claim.set(res.data);
        this.adjustAmount.set(res.data.requestedAmount);
        if(this.claim()?.status !='Draft'){
          this.loadWorkflowStatus();
        }
      },
      error: (err) =>{
        this.loading.set(false);
        let msg = err?.error?.message ?? 'You do not have access to this claim or it could not be found.';
        this.loadError.set(msg);
      }
    })
  }

  loadWorkflowStatus(): void{
    this.workflowService.getCLaimStatus(this.claimId()).subscribe({
      next: (res) =>{
        this.workflowStatus.set(res.data);
      },
      error: (err) =>{
        this.workflowStatus.set(undefined);
      }
    })
  }

  onFileSelected(event: Event): void{
    const input = event.target as HTMLInputElement;
    this.selectedFile.set(input.files?.[0]);
  }

  uploadDocument(): void{
    const file = this.selectedFile();
    if(!file) return;
    this.claimService.uploadDocument(this.claimId(), file).subscribe({
      next: (res) =>{
        this.actionMessage.set('Document uploaded.');
        this.selectedFile.set(undefined);
        this.reload();
      },
      error: (err) =>{
        let msg = err?.error?.message ?? 'Upload failed!!';
        this.actionError.set(msg);
      }
    })
  }

  submitClaim(): void{
    this.claimService.submit(this.claimId()).subscribe({
      next: (res) =>{
        this.actionMessage.set("Claim Submitted for review.");
        this.reload();
      },
      error: (err) =>{
        let msg = err?.error?.message ?? "Claim Submission Failed.";
        this.actionMessage.set(msg);
      }
    })
  }

  requestDocuments(): void{
    this.claimService.requestDocuments(this.claimId(), this.comments()).subscribe({
      next: (res) =>{
        this.actionMessage.set("Additional documents required.");
        this.comments.set("");
        this.reload();
      },
      error: (err) =>{
        let msg =  err?.error?.message ?? "Could not request documents";
        this.actionError.set(msg);
      }
    })
  }

  adjustClaim(): void {
    this.claimService.adjust(this.claimId(), { adjustedAmount: this.adjustAmount(), comments: this.comments() }).subscribe({
      next: () => { 
        this.actionMessage.set('Claim adjusted.'); 
        this.comments.set(""); 
        this.reload(); 
      },
      error: err => { 
        let msg = err?.error?.message ?? 'Could not adjust claim.';
        this.actionError.set(msg); }
    });
  }

  resubmitAfterDocuments(): void{
    this.claimService.resubmitAfterDocuments(this.claimId()).subscribe({
      next: (res) =>{
        this.actionMessage.set("Documents submitted; workflow resumed.");
        this.reload();
      },
      error: (err) =>{
        let msg = err?.error?.message ?? 'Could not submit claim.';
        this.actionError.set(msg);
      }
    });
  }

  completeReview(): void {
    this.claimService.completeReview(this.claimId(), this.comments()).subscribe({
      next: () => { this.actionMessage.set('Review completed.'); 
        this.comments.set('');
        this.reload(); },
      error: err => { 
        let msg = err?.error?.message ?? 'Could not complete review.';
        this.actionError.set(msg);
      }
    });
  }

  approve(): void {
    this.claimService.approve(this.claimId(), this.comments()).subscribe({
      next: () => { 
        this.actionMessage.set('Claim approved.');
        this.comments.set('');
        this.reload(); 
      },
      error: err => { 
        let msg = err?.error?.message ?? 'Could not approve claim.';
        this.actionError.set(msg);
      }
    });
  }

  reject(): void {
    this.claimService.reject(this.claimId(), this.comments()).subscribe({
      next: () => { 
        this.actionMessage.set('Claim rejected.'); 
        this.comments.set(''); 
        this.reload(); 
      },
      error: err => {
        let msg = err?.error?.message ?? 'Could not reject claim.';
        this.actionError.set(msg) 
      }
    });
  }
}
