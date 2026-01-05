import { CommonModule } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import { type MenuItem, SharedModule } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { IconFieldModule } from "primeng/iconfield";
import { InputIconModule } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";
import { MenuModule } from "primeng/menu";
import { type TableLazyLoadEvent, TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { Api, formatSortField, getLoads$Json } from "@/core/api";
import  type { LoadDto } from "@/core/api/models";
import { LoadStatusTag, LoadTypeTag } from "@/shared/components";
import { AddressPipe, DistanceUnitPipe } from "@/shared/pipes";

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
export class LoadsListComponent {
  private readonly router = inject(Router);
  private readonly api = inject(Api);

  protected readonly data = signal<LoadDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly totalRecords = signal(0);
  protected readonly first = signal(0);
  protected readonly actionMenuItems: MenuItem[];
  protected readonly selectedRow = signal<LoadDto | null>(null);
  protected readonly groupByTrip = signal(false);

  constructor() {
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

  protected async onLazyLoad(event: TableLazyLoadEvent): Promise<void> {
    this.isLoading.set(true);
    const rows = event.rows ?? 10;
    const page = (event.first ?? 0) / rows;
    const sortField = formatSortField(event.sortField as string, event.sortOrder);

    const result = await this.api.invoke(getLoads$Json, {
      Page: page + 1,
      PageSize: rows,
      OrderBy: sortField || "-DispatchedAt",
    });

    if (result.data) {
      this.data.set(result.data);
      this.totalRecords.set(result.totalItems ?? 0);
      this.first.set(page * rows);
    }

    this.isLoading.set(false);
  }

  protected async onSearch(event: Event): Promise<void> {
    this.isLoading.set(true);
    const value = (event.target as HTMLInputElement).value;

    const result = await this.api.invoke(getLoads$Json, {
      Search: value,
      Page: 1,
      PageSize: 10,
      OrderBy: "-DispatchedAt",
    });

    if (result.data) {
      this.data.set(result.data);
      this.totalRecords.set(result.totalItems ?? 0);
      this.first.set(0);
    }

    this.isLoading.set(false);
  }
}
