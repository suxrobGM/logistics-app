import {CommonModule} from "@angular/common";
import {Component} from "@angular/core";
import {RouterLink} from "@angular/router";
import {SharedModule} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {IconFieldModule} from "primeng/iconfield";
import {InputIconModule} from "primeng/inputicon";
import {InputTextModule} from "primeng/inputtext";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {ApiService} from "@/core/api";
import {TruckDto} from "@/core/api/models";
import {AddressPipe} from "@/core/pipes";

@Component({
  selector: "app-list-trucks",
  templateUrl: "./list-trucks.component.html",
  styleUrl: "./list-trucks.component.css",
  imports: [
    CommonModule,
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    SharedModule,
    InputTextModule,
    AddressPipe,
    IconFieldModule,
    InputIconModule,
  ],
})
export class ListTruckComponent {
  public trucks: TruckDto[];
  public isLoading: boolean;
  public totalRecords: number;

  constructor(private apiService: ApiService) {
    this.trucks = [];
    this.isLoading = false;
    this.totalRecords = 0;
  }

  search(event: Event) {
    this.isLoading = true;
    const searchValue = (event.target as HTMLInputElement).value;

    this.apiService.getTrucks({search: searchValue}).subscribe((result) => {
      if (result.success && result.data) {
        this.trucks = result.data;
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
      .getTrucks({orderBy: sortField, page: page, pageSize: rows})
      .subscribe((result) => {
        if (result.success && result.data) {
          this.trucks = result.data;
          this.totalRecords = result.totalItems;
        }

        this.isLoading = false;
      });
  }
}
