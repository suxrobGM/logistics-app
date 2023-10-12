import {Component} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterLink} from '@angular/router';
import {ButtonModule} from 'primeng/button';
import {CardModule} from 'primeng/card';
import {TableModule} from 'primeng/table';
import {TooltipModule} from 'primeng/tooltip';
import {Customer} from '@core/models';
import { ApiService } from '@core/services';


@Component({
  selector: 'app-list-customers',
  standalone: true,
  templateUrl: './list-customers.component.html',
  styleUrls: ['./list-customers.component.scss'],
  imports: [
    CommonModule,
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
  ],
})
export class ListCustomersComponent {
  public customers: Customer[];
  public isLoading: boolean;
  public totalRecords: number;
  public first: number;

  constructor(private readonly apiService: ApiService) {
    this.customers = [];
  }
}
