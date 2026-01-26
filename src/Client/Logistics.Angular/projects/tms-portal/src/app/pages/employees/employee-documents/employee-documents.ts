import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Api, getEmployeeById } from "@logistics/shared/api";
import type { DocumentType, EmployeeDto } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ToastModule } from "primeng/toast";
import { DocumentManagerComponent } from "@/shared/components";

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
