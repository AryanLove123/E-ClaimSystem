import { Component, inject, OnInit, signal } from '@angular/core';
import { UserService } from '../../../core/services/user.service';
import { UserListItem } from '../../../core/models/user.model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-users',
  imports: [CommonModule],
  templateUrl: './users.component.html',
  styleUrl: '../../../shared/styles/page.scss',
})
export class UsersComponent implements OnInit {
  userService = inject(UserService);
  users = signal<UserListItem[]>([]);
  loading = signal<boolean>(true);

  ngOnInit(): void {
    this.load();
  }

  load(): void{
    this.loading.set(true);
    this.userService.getAll().subscribe({
      next: (res) =>{
        this.users.set(res.data);
        this.loading.set(false);
      },
      error: (err) =>{
        this.loading.set(false);
      }
    })
  }

  toggleActive(user: UserListItem): void {
    this.userService.setActiveStatus(user.id, !user.isActive).subscribe({ next: () => this.load() });
  }
}
