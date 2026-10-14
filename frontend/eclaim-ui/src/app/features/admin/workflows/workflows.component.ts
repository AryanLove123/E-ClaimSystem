import { Component, inject, OnInit, signal } from '@angular/core';
import { WorkflowService } from '../../../core/services/workflow.service';
import { Workflow } from '../../../core/models/workflow.model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-workflows',
  imports: [CommonModule],
  templateUrl: './workflows.component.html',
  styleUrl: '../../../shared/styles/page.scss',
})
export class WorkflowsComponent implements OnInit {
  workflowService = inject(WorkflowService);

  workflows = signal<Workflow[] | undefined>(undefined);
  loading = signal<boolean>(true);

  ngOnInit(): void {
    this.load();
  }

  load() : void{
    this.loading.set(true);
    this.workflowService.getAll().subscribe({
      next: (res) => {
        this.workflows.set(res?.data);
        this.loading.set(false);
      },
      error: (err) =>{
        this.loading.set(false);
      }
    })
  }
}
