import {CommonModule, CurrencyPipe, DatePipe, PercentPipe} from "@angular/common";
import {Component} from "@angular/core";
import {RouterLink} from "@angular/router";
import {SharedModule} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {InputTextModule} from "primeng/inputtext";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {ApiService} from "@/core/api";
import {SalaryType, SalaryTypeEnum} from "@/core/enums";
import {EmployeeDto} from "@/core/models";

@Component({
  selector: "app-list-employees",
  templateUrl: "./list-employees.component.html",
  styleUrls: [],
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    SharedModule,
    InputTextModule,
    DatePipe,
    PercentPipe,
    CurrencyPipe,
  ],
})
export class ListEmployeeComponent {
  public employees: EmployeeDto[];
  public isLoading: boolean;
  public totalRecords: number;
  public first: number;

  constructor(private apiService: ApiService) {
    this.employees = [];
    this.isLoading = false;
    this.totalRecords = 0;
    this.first = 0;
  }

  search(event: Event) {
    this.isLoading = true;
    const searchValue = (event.target as HTMLInputElement).value;

    this.apiService.getEmployees({search: searchValue}).subscribe((result) => {
      if (result.success && result.data) {
        this.employees = result.data;
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

    this.apiService
      .getEmployees({orderBy: sortField, page: page, pageSize: rows})
      .subscribe((result) => {
        if (result.success && result.data) {
          this.employees = result.data;
          this.totalRecords = result.totalItems;
        }

        this.isLoading = false;
      });
  }

  getSalaryTypeDesc(enumValue: SalaryType): string {
    return SalaryTypeEnum.getValue(enumValue).description;
  }
}
