import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { Component, type OnInit, inject, input, signal, viewChild } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { Api, getLoadById } from "@logistics/shared/api";
import type { DocumentType, LoadDto, LoadExceptionDto } from "@logistics/shared/api";
import { AddressPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TabsModule } from "primeng/tabs";
import { DocumentManagerComponent, PageHeader } from "@/shared/components";
import { LoadStatusTag, LoadTypeTag } from "@/shared/components/tags";
import { DistanceUnitPipe } from "@/shared/pipes";
import {
  LoadExceptionsContent,
  LoadPodContent,
  LoadStatusStepper,
  ReportExceptionDialog,
  ResolveExceptionDialog,
  TrackingLinkDialog,
} from "../components";

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
    LoadStatusStepper,
    LoadPodContent,
    LoadExceptionsContent,
    TrackingLinkDialog,
    ReportExceptionDialog,
    ResolveExceptionDialog,
  ],
})
export class LoadDetailPage implements OnInit {
  private readonly api = inject(Api);
  private readonly router = inject(Router);

  private readonly exceptionsContent = viewChild(LoadExceptionsContent);

  protected readonly id = input.required<string>();
  protected readonly isLoading = signal(false);
  protected readonly load = signal<LoadDto | null>(null);
  protected readonly activeTab = signal(0);
  protected readonly showTrackingDialog = signal(false);

  // Exception dialog state
  protected readonly showReportExceptionDialog = signal(false);
  protected readonly showResolveExceptionDialog = signal(false);
  protected readonly selectedExceptionToResolve = signal<LoadExceptionDto | null>(null);

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

  onReportException(): void {
    this.showReportExceptionDialog.set(true);
  }

  onResolveException(exception: LoadExceptionDto): void {
    this.selectedExceptionToResolve.set(exception);
    this.showResolveExceptionDialog.set(true);
  }

  onExceptionReported(): void {
    this.exceptionsContent()?.refresh();
  }

  onExceptionResolved(): void {
    this.selectedExceptionToResolve.set(null);
    this.exceptionsContent()?.refresh();
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
