import { Component, OnInit, inject, signal } from "@angular/core";
import { ActivatedRoute, RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ToastModule } from "primeng/toast";
import { DocumentType } from "@/core/api/models";
import { DocumentManagerComponent } from "@/shared/components/document-manager/document-manager";

@Component({
  selector: "app-load-documents",
  templateUrl: "./load-documents.html",
  imports: [CardModule, ToastModule, RouterLink, DocumentManagerComponent, ButtonModule],
})
export class LoadDocumentsPage implements OnInit {
  private readonly route = inject(ActivatedRoute);

  protected readonly loadId = signal<string>("");
  protected readonly loadDocTypes: DocumentType[] = [
    DocumentType.BillOfLading,
    DocumentType.ProofOfDelivery,
    DocumentType.Invoice,
    DocumentType.Receipt,
    DocumentType.Contract,
    DocumentType.Photo,
    DocumentType.Other,
  ];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get("id");
    if (id) this.loadId.set(id);
  }
}
