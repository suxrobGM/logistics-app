import {Component} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterLink} from '@angular/router';
import {ButtonModule} from 'primeng/button';
import {CardModule} from 'primeng/card';
import {InputTextModule} from 'primeng/inputtext';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import {TooltipModule} from 'primeng/tooltip';
import {ConfirmDialogModule} from 'primeng/confirmdialog';
import {ConfirmationService} from 'primeng/api';
import {CustomerDto} from '@core/models';
import {ApiService, ToastService} from '@core/services';


@Component({
  selector: 'app-list-customers',
  standalone: true,
  templateUrl: './list-customers.component.html',
  styleUrls: [],
  imports: [
    CommonModule,
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    InputTextModule,
    ConfirmDialogModule,
    TooltipModule,
  ],
  providers: [
    ConfirmationService
  ]
})
export class ListCustomersComponent {
  public customers: CustomerDto[];
  public isLoading: boolean;
  public totalRecords: number;
  public first: number;

  constructor(
    private apiService: ApiService,
    private toastService: ToastService,
    private confirmationService: ConfirmationService)
  {
    this.customers = [];
    this.isLoading = false;
    this.totalRecords = 0;
    this.first = 0;
  }

  search(event: Event) {
    this.isLoading = true;
    const searchValue = (event.target as HTMLInputElement).value;

    this.apiService.getCustomers({search: searchValue}).subscribe((result) => {
      if (result.success && result.data) {
        this.customers = result.data;
        this.totalRecords = result.totalItems;
      }

      this.isLoading = false;
    });
  }

  load(event: TableLazyLoadEvent) {
    this.isLoading = true;
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    this.apiService.getCustomers({orderBy: sortField, page: page, pageSize: rows}).subscribe((result) => {
      if (result.success && result.data) {
        this.customers = result.data;
        this.totalRecords = result.totalItems;
      }

      this.isLoading = false;
    });
  }

  confirmToDelete(id: string) {
    this.confirmationService.confirm({
      message: 'Are you sure that you want to delete this customer?',
      accept: () => this.deleteCustomer(id),
    });
  }

  deleteCustomer(id: string) {
    this.isLoading = true;

    this.apiService.deleteCustomer(id).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess('The customer has been deleted successfully');
        const index = this.customers.findIndex((i) => i.id === id);

        if (index !== -1) {
          this.customers.splice(index);
        }
      }

      this.isLoading = false;
    })
  }
}
