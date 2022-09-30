import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { CreateEmployee, Role, User } from '@shared/models';
import { ApiService } from '@shared/services';
import { UserService } from '../../shared';

@Component({
  selector: 'app-add-employee',
  templateUrl: './add-employee.component.html',
  styleUrls: ['./add-employee.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class AddEmployeeComponent implements OnInit {
  public suggestedUsers: User[];
  public form: FormGroup;
  public roles: Role[];
  public loading: boolean;

  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
    private userService: UserService) 
  {
    this.suggestedUsers = [];
    this.roles = [];
    this.loading = false;

    this.form = new FormGroup({
      user: new FormControl('', Validators.required),
      role: new FormControl('', Validators.required),
    });
  }

  public ngOnInit(): void {
    this.fetchRoles();
  }

  public searchUser(event: any) {
    this.userService.searchUser(event.query).subscribe(users => {
      if (users) {
        this.suggestedUsers = users;
      }
    });
  }

  public clearSelctedRole() {
    this.form.patchValue({
      role: {name: '', displayName: ' '}
    });
  }

  public submit() {
    const user = this.form.value.user as User;
    
    if (!user) {
      this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: 'Select user'});
      return;
    }

    const newEmployee: CreateEmployee = {
      id: user.id!,
      role: this.form.value.role
    };

    this.loading = true;
    this.apiService.createEmployee(newEmployee).subscribe(result => {
      if (result.success) {
        this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'New employee has been added successfully'});
        this.form.reset();
      }

      this.loading = false;
    });
  }

  private fetchRoles() {
    this.loading = true;

    this.userService.fetchRoles().subscribe(roles => {
      if (roles) {
        this.roles = roles;
      }

      this.loading = false;
    });
  }
}