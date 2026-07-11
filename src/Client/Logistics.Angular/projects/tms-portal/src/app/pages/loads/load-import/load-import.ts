import { CommonModule } from "@angular/common";
import { type HttpErrorResponse } from "@angular/common/http";
import { Component, computed, inject, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import {
  Api,
  importLoadFromPdf,
  type ExtractedLoadDataDto,
  type ImportLoadFromPdfResponse,
  type TruckDto,
} from "@logistics/shared/api";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import {
  Alert,
  Card,
  Grid,
  Icon,
  PdfViewer,
  Spinner,
  Stack,
  Typography,
  UiButton,
  UiFileUpload,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { SearchTruck, UiFormField } from "@/shared/components";

@Component({
  selector: "app-load-import",
  templateUrl: "./load-import.html",
  imports: [
    Alert,
    Card,
    CommonModule,
    CurrencyFormatPipe,
    Grid,
    Icon,
    PdfViewer,
    RouterLink,
    SearchTruck,
    Spinner,
    Stack,
    Typography,
    UiButton,
    UiFileUpload,
    UiFormField,
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
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly canUpload = computed(() => !this.isUploading());

  protected async onFileSelect(files: File[]): Promise<void> {
    const file = files[0];
    if (!file) return;

    // `ui-file-upload` already enforces `accept=".pdf"` and the 10 MB `maxFileSize` — including for
    // drag-and-drop, which the native `accept` attribute does not cover — and reports refusals via
    // `(rejected)`. These two checks are kept as a backstop: they are the same limits, stated once
    // more at the point that actually consumes the file.
    if (!file.name.toLowerCase().endsWith(".pdf")) {
      this.error.set("Please select a PDF file");
      return;
    }

    if (file.size > 10 * 1024 * 1024) {
      this.error.set("File size exceeds 10 MB limit");
      return;
    }

    // Store file for preview
    this.selectedFile.set(file);

    this.error.set(null);
    this.isUploading.set(true);
    this.extractedData.set(null);
    this.importResult.set(null);

    try {
      const response = await this.api.invoke(importLoadFromPdf, {
        body: {
          File: file,
          AssignedTruckId: this.selectedTruck()?.id ?? undefined,
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
    this.selectedFile.set(null);
  }

  protected viewLoad(): void {
    const result = this.importResult();
    if (result) {
      this.router.navigate(["/loads", result.loadId, "edit"]);
    }
  }
}
