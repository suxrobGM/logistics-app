import { DatePipe } from "@angular/common";
import { Component, inject, signal, type OnInit } from "@angular/core";
import { Router } from "@angular/router";
import type { ConditionReportDto } from "@logistics/shared/api";
import { Icon } from "@logistics/shared/components";
import { isContainerLoadType } from "@logistics/shared/utils";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { MenuModule } from "primeng/menu";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
import { ConditionReportsListStore } from "../store/condition-reports-list.store";

@Component({
  selector: "app-condition-reports-list",
  templateUrl: "./condition-reports-list.html",
  providers: [ConditionReportsListStore],
  imports: [
    ButtonModule,
    TooltipModule,
    CardModule,
    TableModule,
    MenuModule,
    DataContainer,
    DatePipe,
    PageHeader,
    SearchInput,
    TagModule,
    Icon,
  ],
})
export class ConditionReportsListPage implements OnInit {
  private readonly router = inject(Router);
  protected readonly store = inject(ConditionReportsListStore);

  protected readonly selectedRow = signal<ConditionReportDto | null>(null);
  protected readonly actionMenuItems: MenuItem[];

  constructor() {
    this.actionMenuItems = [
      {
        label: "View details",
        icon: "pi pi-eye",
        command: () =>
          this.router.navigateByUrl(`/safety/condition-reports/${this.selectedRow()!.id}`),
      },
      {
        label: "View load",
        icon: "pi pi-box",
        command: () => this.router.navigateByUrl(`/loads/${this.selectedRow()!.loadId}`),
      },
    ];
  }

  ngOnInit(): void {
    this.store.load();
  }

  protected onSearch(value: string): void {
    this.store.setSearch(value);
  }

  protected openActionMenu(
    report: ConditionReportDto,
    menu: { toggle: (event: Event) => void },
    event: Event,
  ): void {
    this.selectedRow.set(report);
    menu.toggle(event);
  }

  protected getTypeSeverity(type: string | undefined): "success" | "info" {
    return type === "pickup" ? "info" : "success";
  }

  protected getTypeLabel(type: string | undefined): string {
    return type === "pickup" ? "Pickup" : "Delivery";
  }

  protected getDefectCount(report: ConditionReportDto): number {
    return report.defects?.length ?? 0;
  }

  /**
   * Identifier shown in list view: VIN for Vehicle loads, container number for
   * container loads, "Load #{loadReferenceId}" for everything else.
   */
  protected getIdentifier(report: ConditionReportDto): string {
    if (report.loadType === "vehicle") {
      return report.vin ?? "Vehicle (no VIN)";
    }
    if (isContainerLoadType(report.loadType)) {
      return report.containerNumber ?? "Container (no number)";
    }
    return report.loadReferenceId ? `Load #${report.loadReferenceId}` : "Load";
  }

  protected getVehicleInfo(report: ConditionReportDto): string {
    const parts = [report.vehicleYear, report.vehicleMake, report.vehicleModel].filter(Boolean);
    return parts.length > 0 ? parts.join(" ") : "N/A";
  }
}
