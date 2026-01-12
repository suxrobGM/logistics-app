import { CommonModule } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { ChangeDetectionStrategy, Component, inject, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { FileUploadModule, type FileSelectEvent } from "primeng/fileupload";
import { MessageModule } from "primeng/message";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { ToastModule } from "primeng/toast";
import { ApiConfiguration } from "@/core/api/generated/api-configuration";
import { ToastService } from "@/core/services";

interface ExtractedAddressDto {
  line1?: string | null;
  line2?: string | null;
  city?: string | null;
  state?: string | null;
  zipCode?: string | null;
  country?: string | null;
  contactName?: string | null;
  phone?: string | null;
}

interface ExtractedLoadDataDto {
  orderId?: string | null;
  vehicleYear?: number | null;
  vehicleMake?: string | null;
  vehicleModel?: string | null;
  vehicleVin?: string | null;
  vehicleType?: string | null;
  originAddress?: ExtractedAddressDto | null;
  destinationAddress?: ExtractedAddressDto | null;
  pickupDate?: string | null;
  deliveryDate?: string | null;
  paymentAmount?: number | null;
  shipperName?: string | null;
  sourceTemplate?: string | null;
}

interface ImportLoadFromPdfResponse {
  loadId: string;
  loadName: string;
  loadNumber: number;
  extractedData?: ExtractedLoadDataDto | null;
  customerCreated: boolean;
  customerName?: string | null;
  warnings: string[];
}

@Component({
  selector: "app-load-import",
  templateUrl: "./load-import.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    CardModule,
    FileUploadModule,
    ButtonModule,
    ProgressSpinnerModule,
    MessageModule,
    ToastModule,
    RouterLink,
  ],
})
export class LoadImportComponent {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);

  protected readonly isUploading = signal(false);
  protected readonly extractedData = signal<ExtractedLoadDataDto | null>(null);
  protected readonly importResult = signal<ImportLoadFromPdfResponse | null>(null);
  protected readonly error = signal<string | null>(null);

  protected async onFileSelect(event: FileSelectEvent): Promise<void> {
    const file = event.files[0];
    if (!file) return;

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
      const formData = new FormData();
      formData.append("File", file);

      const response = await this.http
        .post<ImportLoadFromPdfResponse>(`${this.apiConfig.rootUrl}/loads/import`, formData)
        .toPromise();

      if (response) {
        this.importResult.set(response);
        this.extractedData.set(response.extractedData ?? null);
        this.toastService.showSuccess(`Load "${response.loadName}" created successfully`);
      }
    } catch (err: unknown) {
      const errorMessage = this.extractErrorMessage(err);
      this.error.set(errorMessage);
      this.toastService.showError(errorMessage);
    } finally {
      this.isUploading.set(false);
    }
  }

  protected reset(): void {
    this.isUploading.set(false);
    this.extractedData.set(null);
    this.importResult.set(null);
    this.error.set(null);
  }

  protected viewLoad(): void {
    const result = this.importResult();
    if (result) {
      this.router.navigate(["/loads", result.loadId, "edit"]);
    }
  }

  private extractErrorMessage(err: unknown): string {
    if (err && typeof err === "object") {
      const error = err as { error?: { errors?: string[] } };
      if (error.error?.errors && error.error.errors.length > 0) {
        return error.error.errors.join(". ");
      }
    }
    return "Failed to import PDF. Please try again.";
  }
}
