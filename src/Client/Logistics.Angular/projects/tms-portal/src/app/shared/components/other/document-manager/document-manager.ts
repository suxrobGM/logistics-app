import { CommonModule } from "@angular/common";
import { Component, type OnInit, inject, input, output, signal } from "@angular/core";
import {
  Api,
  deleteDocument,
  downloadDocument,
  getDocuments,
  uploadDocument,
} from "@logistics/shared/api";
import type { DocumentDto, DocumentStatus, DocumentType } from "@logistics/shared/api";
import { downloadBlobFile, formatFileSize } from "@logistics/shared/utils";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { FileUploadModule } from "primeng/fileupload";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { Tag, TagModule } from "primeng/tag";
import { ToastModule } from "primeng/toast";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { Converters } from "@/shared/utils";

@Component({
  selector: "app-document-manager",
  templateUrl: "./document-manager.html",
  styleUrl: "./document-manager.css",
  imports: [
    CommonModule,
    ButtonModule,
    TableModule,
    FileUploadModule,
    TagModule,
    ToastModule,
    CardModule,
    ProgressSpinnerModule,
    TooltipModule,
  ],
})
export class DocumentManagerComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  public readonly employeeId = input<string>();
  public readonly loadId = input<string>();
  public readonly truckId = input<string>();
  public readonly types = input<DocumentType[]>([]);

  public readonly changed = output<void>();

  protected readonly isLoading = signal(false);
  protected readonly rows = signal<DocumentDto[]>([]);
  protected readonly uploadProgress = signal<Record<string, number>>({});

  ngOnInit(): void {
    this.refresh();
  }

  protected async refresh(): Promise<void> {
    this.isLoading.set(true);

    const ownerType = this.employeeId() ? "employee" : this.loadId() ? "load" : "truck";
    const ownerId = this.employeeId() || this.loadId() || this.truckId() || "";

    const result = await this.api.invoke(getDocuments, {
      OwnerType: ownerType,
      OwnerId: ownerId,
    });

    if (result) {
      this.rows.set(result);
    }
    this.isLoading.set(false);
  }

  protected onFileChange(event: Event, type: DocumentType): void {
    const input = event.target as HTMLInputElement;
    const files = input.files;
    if (files && files.length > 0) {
      this.upload(files[0], type);
    }
    input.value = ""; // reset
  }

  protected async upload(file: File, type: DocumentType): Promise<void> {
    if (file.size > 20 * 1024 * 1024) {
      this.toast.showError("File exceeds 20MB limit");
      return;
    }

    const ownerType = this.employeeId() ? "employee" : this.loadId() ? "load" : "truck";
    const ownerId = this.employeeId() || this.loadId() || this.truckId() || "";

    this.uploadProgress.set({ [type]: 0 });

    try {
      await this.api.invoke(uploadDocument, {
        body: {
          OwnerType: ownerType as "employee" | "load",
          OwnerId: ownerId,
          File: file,
          Type: Converters.toPascalCase(type) as DocumentType,
          Description: `${this.getTypeLabel(type)} document`,
        },
      });

      this.uploadProgress.set({ [type]: 100 });
      this.toast.showSuccess(`${this.getTypeLabel(type)} uploaded successfully`);
      await this.refresh();
      this.changed.emit();

      // Reset progress after a short delay
      setTimeout(() => this.uploadProgress.set({ [type]: 0 }), 1500);
    } catch {
      this.toast.showError(`Failed to upload ${this.getTypeLabel(type)}`);
      this.uploadProgress.set({ [type]: 0 });
    }
  }

  protected async download(row: DocumentDto): Promise<void> {
    try {
      const blob = await this.api.invoke(downloadDocument, { documentId: row.id! });
      const fileName = row.originalFileName || row.fileName;
      downloadBlobFile(blob, fileName!);
    } catch {
      this.toast.showError("Failed to download file");
    }
  }

  protected delete(row: DocumentDto): void {
    this.toast.confirmDelete("document", async () => {
      try {
        await this.api.invoke(deleteDocument, { documentId: row.id! });
        this.toast.showSuccess("Document deleted successfully");
        await this.refresh();
        this.changed.emit();
      } catch {
        this.toast.showError("Failed to delete document");
      }
    });
  }

  protected statusSeverity(status: DocumentStatus): Tag["severity"] {
    switch (status) {
      case "active":
        return "success";
      case "archived":
        return "warn";
      case "deleted":
        return "danger";
      default:
        return "info";
    }
  }

  protected readonly formatFileSize = formatFileSize;

  // Get a user-friendly label for the document type
  // Remove underscores and capitalize words from the enum value
  protected getTypeLabel(type: DocumentType): string {
    return type.replace(/_/g, " ").replace(/\b\w/g, (c) => c.toUpperCase());
  }
}
