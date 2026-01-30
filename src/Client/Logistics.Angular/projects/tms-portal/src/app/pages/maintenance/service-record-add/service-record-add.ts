import { Component, inject } from "@angular/core";
import { Router } from "@angular/router";
import type { MaintenanceRecordDto } from "@logistics/shared/api";
import { CardModule } from "primeng/card";
import { PageHeader } from "@/shared/components";
import { MaintenanceRecordForm } from "../components/maintenance-record-form/maintenance-record-form";

@Component({
  selector: "app-service-record-add",
  templateUrl: "./service-record-add.html",
  imports: [CardModule, PageHeader, MaintenanceRecordForm],
})
export class ServiceRecordAddPage {
  private readonly router = inject(Router);

  protected onSave(record: MaintenanceRecordDto): void {
    this.router.navigateByUrl("/maintenance/records");
  }
}
