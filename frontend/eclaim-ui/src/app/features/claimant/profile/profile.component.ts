import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../../core/services/user.service';

@Component({
  selector: 'app-profile',
  imports: [ReactiveFormsModule],
  templateUrl: './profile.component.html',
  styles: ``,
})
export class ProfileComponent implements OnInit {
  fb = inject(FormBuilder);
  userService = inject(UserService);

  form = this.fb.nonNullable.group({
    fullName: ['', Validators.required],
    phoneNumber: ['']
  });

  loading = signal<boolean>(true);
  saved = signal<boolean>(false);

  email = '';
  role = '';
  emailVerified = false;

  ngOnInit(): void {
    this.userService.getProfile().subscribe({
      next: (res) =>{
        this.form.patchValue({fullName: res.data.fullName, phoneNumber: res.data.phoneNumber});
        this.email = res.data.email;
        this.role = res.data.role;
        this.emailVerified = res.data.emailVerified;
        this.loading.set(false);
      },
      error: () =>{
        this.loading.set(false);
      }
    })
  }

  save() : void{
    if(this.form.invalid) return;
    this.userService.updateProfile(this.form.getRawValue()).subscribe({
      next: () => {
        this.saved.set(true);
        setTimeout(() => this.saved.set(false), 3000);
      }
    });
  }
}
