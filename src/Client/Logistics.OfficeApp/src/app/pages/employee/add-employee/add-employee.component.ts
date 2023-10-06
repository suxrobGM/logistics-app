import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {NgIf} from '@angular/common';
import {FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule} from '@angular/forms';
import {RouterLink} from '@angular/router';
import {MessageService} from 'primeng/api';
import {ButtonModule} from 'primeng/button';
import {DropdownModule} from 'primeng/dropdown';
import {AutoCompleteModule} from 'primeng/autocomplete';
import {MessagesModule} from 'primeng/messages';
import {ProgressSpinnerModule} from 'primeng/progressspinner';
import {CardModule} from 'primeng/card';
import {ConfirmDialogModule} from 'primeng/confirmdialog';
import {ToastModule} from 'primeng/toast';
import {CreateEmployee, Role, User} from '@core/models';
import {ApiService} from '@core/services';
import {UserService} from '../services';


@Component({
  selector: 'app-add-employee',
  templateUrl: './add-employee.component.html',
  styleUrls: ['./add-employee.component.scss'],
  encapsulation: ViewEncapsulation.None,
  standalone: true,
  imports: [
    ToastModule,
    ConfirmDialogModule,
    CardModule,
    NgIf,
    ProgressSpinnerModule,
    MessagesModule,
    FormsModule,
    ReactiveFormsModule,
    AutoCompleteModule,
    DropdownModule,
    ButtonModule,
    RouterLink,
  ],
  providers: [
    UserService,
  ],
})
export class AddEmployeeComponent implements OnInit {
  public suggestedUsers: User[];
  public form: FormGroup;
  public roles: Role[];
  public isBusy: boolean;

  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
    private userService: UserService)
  {
    this.suggestedUsers = [];
    this.roles = [];
    this.isBusy = false;

    this.form = new FormGroup({
      user: new FormControl('', Validators.required),
      role: new FormControl('', Validators.required),
    });
  }

  public ngOnInit(): void {
    this.fetchRoles();
  }

  public searchUser(event: any) {
    this.userService.searchUser(event.query).subscribe((users) => {
      if (users) {
        this.suggestedUsers = users;
      }
    });
  }

  public clearSelctedRole() {
    this.form.patchValue({
      role: {name: '', displayName: ' '},
    });
  }

  public submit() {
    const user = this.form.value.user as User;

    if (!user) {
      this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: 'Select user'});
      return;
    }

    const newEmployee: CreateEmployee = {
      userId: user.id!,
      role: this.form.value.role,
    };

    this.isBusy = true;
    this.apiService.createEmployee(newEmployee).subscribe((result) => {
      if (result.isSuccess) {
        this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'New employee has been added successfully'});
        this.form.reset();
      }

      this.isBusy = false;
    });
  }

  private fetchRoles() {
    this.isBusy = true;

    this.userService.fetchRoles().subscribe((roles) => {
      if (roles) {
        this.roles = roles;
      }

      this.isBusy = false;
    });
  }
}
