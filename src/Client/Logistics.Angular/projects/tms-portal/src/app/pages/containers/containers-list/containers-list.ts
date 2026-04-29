import { DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import { Permission } from "@logistics/shared";
import type { ContainerDto, ContainerIsoType, ContainerStatus } from "@logistics/shared/api";
import { containerIsoTypeOptions, containerStatusOptions } from "@logistics/shared/api/enums";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { SelectModule } from "primeng/select";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
import { ContainersListStore } from "../store";

@Component({
  selector: "app-containers-list",
  templateUrl: "./containers-list.html",
  providers: [ContainersListStore],
  imports: [
    ButtonModule,
    CardModule,
    TableModule,
    TagModule,
    SelectModule,
    TooltipModule,
    DatePipe,
    DataContainer,
    PageHeader,
    SearchInput,
  ],
})
export class ContainersList {
  private readonly router = inject(Router);
  protected readonly store = inject(ContainersListStore);
  protected readonly Permission = Permission;

  protected readonly statusOptions = containerStatusOptions;
  protected readonly isoTypeOptions = containerIsoTypeOptions;

  protected readonly statusFilter = signal<ContainerStatus | null>(null);
  protected readonly isoTypeFilter = signal<ContainerIsoType | null>(null);

  protected search(value: string): void {
    this.store.setSearch(value);
  }

  protected onStatusChange(status: ContainerStatus | null): void {
    this.statusFilter.set(status);
    this.applyFilters();
  }

  protected onIsoTypeChange(isoType: ContainerIsoType | null): void {
    this.isoTypeFilter.set(isoType);
    this.applyFilters();
  }

  protected addContainer(): void {
    this.router.navigate(["/containers/add"]);
  }

  protected editContainer(container: ContainerDto): void {
    if (container.id) {
      this.router.navigate(["/containers", container.id]);
    }
  }

  protected statusSeverity(status?: ContainerStatus): "info" | "success" | "warn" | "secondary" {
    switch (status) {
      case "loaded":
      case "in_transit":
        return "info";
      case "delivered":
        return "success";
      case "at_port":
      case "returned":
        return "warn";
      default:
        return "secondary";
    }
  }

  protected statusLabel(status?: ContainerStatus): string {
    return containerStatusOptions.find((opt) => opt.value === status)?.label ?? "";
  }

  protected isoTypeLabel(isoType?: ContainerIsoType): string {
    return containerIsoTypeOptions.find((opt) => opt.value === isoType)?.label ?? "";
  }

  private applyFilters(): void {
    const filters: Record<string, unknown> = {};
    const status = this.statusFilter();
    const isoType = this.isoTypeFilter();
    if (status) filters["Status"] = status;
    if (isoType) filters["IsoType"] = isoType;
    this.store.setFilters(filters);
  }
}
