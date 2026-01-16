import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { DocumentManagerComponent } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ToastModule } from "primeng/toast";
import { Api, getEmployeeById } from "@/core/api";
import type { DocumentType, EmployeeDto } from "@/core/api/models";

@Component({
  selector: "app-employee-documents",
  templateUrl: "./employee-documents.html",
  imports: [CardModule, ToastModule, RouterLink, DocumentManagerComponent, ButtonModule],
})
export class EmployeeDocumentsPage implements OnInit {
  private readonly api = inject(Api);

  protected readonly employee = signal<EmployeeDto | null>(null);

  public readonly id = input.required<string>();

  protected readonly employeeDocTypes: DocumentType[] = [
    "driver_license",
    "insurance_certificate",
    "identity_document",
    "vehicle_registration",
    "other",
  ];

  ngOnInit(): void {
    this.fetchEmployee();
  }

  private async fetchEmployee(): Promise<void> {
    const result = await this.api.invoke(getEmployeeById, { userId: this.id() });
    if (result) {
      this.employee.set(result);
    }
  }
}
