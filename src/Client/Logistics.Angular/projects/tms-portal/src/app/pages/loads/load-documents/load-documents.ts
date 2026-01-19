import { Component, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { DocumentType } from "@logistics/shared/api/models";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ToastModule } from "primeng/toast";
import { DocumentManagerComponent } from "@/shared/components";

@Component({
  selector: "app-load-documents",
  templateUrl: "./load-documents.html",
  imports: [CardModule, ToastModule, RouterLink, DocumentManagerComponent, ButtonModule],
})
export class LoadDocumentsPage {
  readonly id = input.required<string>();

  protected readonly loadDocTypes: DocumentType[] = [
    "bill_of_lading",
    "proof_of_delivery",
    "invoice",
    "receipt",
    "contract",
    "photo",
    "other",
  ];
}
