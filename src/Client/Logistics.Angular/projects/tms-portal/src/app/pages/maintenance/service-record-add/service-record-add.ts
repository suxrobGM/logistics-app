import { Component, inject } from "@angular/core";
import { Router } from "@angular/router";
import { Card } from "@logistics/shared/ui";
import { PageHeader } from "@/shared/components";
import { MaintenanceRecordForm } from "../components/maintenance-record-form/maintenance-record-form";

@Component({
  selector: "app-service-record-add",
  templateUrl: "./service-record-add.html",
  imports: [Card, MaintenanceRecordForm, PageHeader],
})
export class ServiceRecordAddPage {
  private readonly router = inject(Router);

  protected onSave(): void {
    this.router.navigateByUrl("/maintenance/records");
  }
}
