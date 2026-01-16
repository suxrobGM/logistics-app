import { CommonModule } from "@angular/common";
import { type HttpErrorResponse } from "@angular/common/http";
import { Component, computed, inject, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { LabeledField, SearchTruckComponent } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { type FileSelectEvent, FileUploadModule } from "primeng/fileupload";
import { MessageModule } from "primeng/message";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { ToastModule } from "primeng/toast";
import { Api, importLoadFromPdf } from "@/core/api";
import type { ExtractedLoadDataDto, ImportLoadFromPdfResponse, TruckDto } from "@/core/api";
import { ToastService } from "@/core/services";

@Component({
  selector: "app-load-import",
  templateUrl: "./load-import.html",
  imports: [
    CommonModule,
    CardModule,
    FileUploadModule,
    ButtonModule,
    ProgressSpinnerModule,
    MessageModule,
    ToastModule,
    RouterLink,
    SearchTruckComponent,
    LabeledField,
    DividerModule,
  ],
})
export class LoadImportComponent {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);

  protected readonly isUploading = signal(false);
  protected readonly extractedData = signal<ExtractedLoadDataDto | null>(null);
  protected readonly importResult = signal<ImportLoadFromPdfResponse | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly selectedTruck = signal<TruckDto | null>(null);
  protected readonly canUpload = computed(
    () => this.selectedTruck() !== null && !this.isUploading(),
  );

  protected async onFileSelect(event: FileSelectEvent): Promise<void> {
    const file = event.files[0];
    if (!file) return;

    const truck = this.selectedTruck();
    if (!truck?.id) {
      this.error.set("Please select a truck before uploading");
      return;
    }

    // Validate file type
    if (!file.name.toLowerCase().endsWith(".pdf")) {
      this.error.set("Please select a PDF file");
      return;
    }

    // Validate file size (10 MB limit)
    if (file.size > 10 * 1024 * 1024) {
      this.error.set("File size exceeds 10 MB limit");
      return;
    }

    this.error.set(null);
    this.isUploading.set(true);
    this.extractedData.set(null);
    this.importResult.set(null);

    try {
      const response = await this.api.invoke(importLoadFromPdf, {
        body: {
          File: file,
          AssignedTruckId: truck.id,
        },
      });

      if (response) {
        this.importResult.set(response);
        this.extractedData.set(response.extractedData ?? null);
        this.toastService.showSuccess(`Load '${response.loadName}' created successfully`);
      }
    } catch (err: unknown) {
      const errorMessage =
        (err as HttpErrorResponse).error?.error ?? "Failed to import PDF. Please try again.";
      this.error.set(errorMessage);
    } finally {
      this.isUploading.set(false);
    }
  }

  protected reset(): void {
    this.isUploading.set(false);
    this.extractedData.set(null);
    this.importResult.set(null);
    this.error.set(null);
    this.selectedTruck.set(null);
  }

  protected viewLoad(): void {
    const result = this.importResult();
    if (result) {
      this.router.navigate(["/loads", result.loadId, "edit"]);
    }
  }
}
