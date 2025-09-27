import { Component, OnInit, inject, input, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ToastModule } from "primeng/toast";
import { ApiService } from "@/core/api";
import { DocumentType, EmployeeDto } from "@/core/api/models";
import { DocumentManagerComponent } from "@/shared/components/document-manager/document-manager";

@Component({
  selector: "app-employee-documents",
  templateUrl: "./employee-documents.html",
  imports: [CardModule, ToastModule, RouterLink, DocumentManagerComponent, ButtonModule],
})
export class EmployeeDocumentsPage implements OnInit {
  private readonly apiService = inject(ApiService);

  protected readonly employee = signal<EmployeeDto | null>(null);

  public readonly id = input.required<string>();

  protected readonly employeeDocTypes: DocumentType[] = [
    DocumentType.DriverLicense,
    DocumentType.InsuranceCertificate,
    DocumentType.IdentityDocument,
    DocumentType.VehicleRegistration,
    DocumentType.Other,
  ];

  ngOnInit(): void {
    this.fetchEmployee();
  }

  private fetchEmployee(): void {
    this.apiService.employeeApi.getEmployee(this.id()).subscribe((result) => {
      if (result.success && result.data) {
        this.employee.set(result.data);
      }
    });
  }
}
