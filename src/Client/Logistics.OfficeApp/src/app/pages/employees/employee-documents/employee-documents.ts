import {Component, OnInit, inject, signal} from "@angular/core";
import {ActivatedRoute, RouterLink} from "@angular/router";
import {CardModule} from "primeng/card";
import {ToastModule} from "primeng/toast";
import {ButtonModule} from "primeng/button";
import { DocumentManagerComponent } from "@/shared/components/document-manager/document-manager";
import {DocumentType} from "@/core/api/models";

@Component({
  selector: "app-employee-documents",
  templateUrl: "./employee-documents.html",
  styleUrls: ["./employee-documents.css"],
  imports: [CardModule, ToastModule, RouterLink, DocumentManagerComponent, ButtonModule],
})
export class EmployeeDocumentsPage implements OnInit {
  private readonly route = inject(ActivatedRoute);

  protected readonly employeeId = signal<string>("");
  protected readonly employeeDocTypes: DocumentType[] = [
    DocumentType.DriverLicense,
    DocumentType.InsuranceCertificate,
    DocumentType.IdentityDocument,
    DocumentType.VehicleRegistration,
    DocumentType.Other,
  ];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get("id");
    if (id) this.employeeId.set(id);
  }
}




