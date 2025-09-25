import {CommonModule} from "@angular/common";
import {Component, inject, signal} from "@angular/core";
import {FormsModule} from "@angular/forms";
import {Router, RouterLink} from "@angular/router";
import {MenuItem, SharedModule} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {IconFieldModule} from "primeng/iconfield";
import {InputIconModule} from "primeng/inputicon";
import {InputTextModule} from "primeng/inputtext";
import {MenuModule} from "primeng/menu";
import {TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {TooltipModule} from "primeng/tooltip";
import {Observable} from "rxjs";
import {ApiService} from "@/core/api";
import {LoadDto, LoadStatus, PagedResult} from "@/core/api/models";
import {
  BaseTableComponent,
  LoadStatusTag,
  LoadTypeTag,
  TableQueryParams,
} from "@/shared/components";
import {AddressPipe, DistanceUnitPipe} from "@/shared/pipes";

@Component({
  selector: "app-loads-list",
  templateUrl: "./loads-list.html",
  imports: [
    CommonModule,
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    SharedModule,
    InputTextModule,
    DistanceUnitPipe,
    AddressPipe,
    TagModule,
    IconFieldModule,
    InputIconModule,
    FormsModule,
    LoadStatusTag,
    LoadTypeTag,
    MenuModule,
  ],
})
export class LoadsListComponent extends BaseTableComponent<LoadDto> {
  private readonly router = inject(Router);
  private readonly apiService = inject(ApiService);

  protected readonly actionMenuItems: MenuItem[];
  protected readonly loadStatus = LoadStatus;
  protected readonly selectedRow = signal<LoadDto | null>(null);
  protected readonly groupByTrip = signal(false);

  constructor() {
    super();
    this.actionMenuItems = [
      {
        label: "Edit load details",
        icon: "pi pi-pen-to-square",
        command: () => this.router.navigateByUrl(`/loads/${this.selectedRow()!.id}/edit`),
      },
      {
        label: "Manage documents",
        icon: "pi pi-paperclip",
        command: () => this.router.navigateByUrl(`/loads/${this.selectedRow()!.id}/documents`),
      },
      {
        label: "View truck details",
        icon: "pi pi-truck",
        command: () => this.router.navigateByUrl(`/trucks/${this.selectedRow()!.assignedTruckId}`),
      },
      {
        label: "View invoices",
        icon: "pi pi-book",
        command: () => this.router.navigateByUrl(`/invoices/loads/${this.selectedRow()!.id}`),
      },
    ];
  }

  protected override query(params: TableQueryParams): Observable<PagedResult<LoadDto>> {
    const sortField = this.apiService.formatSortField(params.sortField, params.sortOrder);

    return this.apiService.loadApi.getLoads({
      page: params.page + 1,
      pageSize: params.size,
      orderBy: sortField || "-DispatchedAt",
      search: params.search,
    });
  }
}
