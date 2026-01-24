import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { Api, getLoadById } from "@logistics/shared/api";
import type { DocumentType, LoadDto } from "@logistics/shared/api/models";
import { AddressPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TabsModule } from "primeng/tabs";
import { DocumentManagerComponent, PageHeader } from "@/shared/components";
import { LoadStatusTag, LoadTypeTag } from "@/shared/components/tags";
import { DistanceUnitPipe } from "@/shared/pipes";
import { LoadInvoicesContent, LoadPodContent, TrackingLinkDialog } from "../components";

@Component({
  selector: "app-load-detail",
  templateUrl: "./load-detail.html",
  imports: [
    CommonModule,
    RouterModule,
    CardModule,
    ButtonModule,
    TabsModule,
    DividerModule,
    ProgressSpinnerModule,
    DatePipe,
    CurrencyPipe,
    PageHeader,
    LoadStatusTag,
    LoadTypeTag,
    AddressPipe,
    DistanceUnitPipe,
    DocumentManagerComponent,
    LoadPodContent,
    LoadInvoicesContent,
    TrackingLinkDialog,
  ],
})
export class LoadDetailPage implements OnInit {
  private readonly api = inject(Api);
  private readonly router = inject(Router);

  protected readonly id = input.required<string>();
  protected readonly isLoading = signal(false);
  protected readonly load = signal<LoadDto | null>(null);
  protected readonly activeTab = signal(0);
  protected readonly showTrackingDialog = signal(false);

  // Document types for the Documents tab
  protected readonly loadDocTypes: DocumentType[] = [
    "bill_of_lading",
    "proof_of_delivery",
    "invoice",
    "receipt",
    "contract",
    "photo",
    "other",
  ];

  ngOnInit(): void {
    this.fetchLoad();
  }

  onTabChange(index: string | number | undefined): void {
    if (typeof index !== "number") return;
    this.activeTab.set(index);
  }

  onEdit(): void {
    this.router.navigate(["/loads", this.id(), "edit"]);
  }

  private async fetchLoad(): Promise<void> {
    if (!this.id()) return;

    this.isLoading.set(true);
    const result = await this.api.invoke(getLoadById, { id: this.id() });
    if (result) {
      this.load.set(result);
    }
    this.isLoading.set(false);
  }
}
