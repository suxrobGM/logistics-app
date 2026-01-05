import { CommonModule } from "@angular/common";
import { Component, type OnInit, inject, input, output, signal } from "@angular/core";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { FileUploadModule } from "primeng/fileupload";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { Tag, TagModule } from "primeng/tag";
import { ToastModule } from "primeng/toast";
import { TooltipModule } from "primeng/tooltip";
import {
  Api,
  deleteDocument$Json,
  downloadDocument$Json,
  getDocuments$Json,
  uploadDocument$Json,
} from "@/core/api";
import type { DocumentDto, DocumentStatus, DocumentType } from "@/core/api/models";
import { ToastService } from "@/core/services";
import { downloadBlobFile } from "@/shared/utils";

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
    DividerModule,
    ProgressSpinnerModule,
    TooltipModule,
  ],
})
export class DocumentManagerComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  readonly employeeId = input<string>();
  readonly loadId = input<string>();
  readonly types = input<DocumentType[]>([]);

  readonly changed = output<void>();

  protected readonly isLoading = signal(false);
  protected readonly rows = signal<DocumentDto[]>([]);
  protected readonly uploadProgress = signal<Record<string, number>>({});

  ngOnInit(): void {
    this.refresh();
  }

  protected async refresh(): Promise<void> {
    this.isLoading.set(true);

    const result = await this.api.invoke(getDocuments$Json, {
      OwnerType: this.employeeId() ? "employee" : "load",
      OwnerId: this.employeeId() || this.loadId() || "",
    });

    if (result.success) {
      this.rows.set(result.data || []);
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
    if (file.size > 50 * 1024 * 1024) {
      this.toast.showError("File exceeds 50MB limit");
      return;
    }

    const ownerType = this.employeeId() ? "employee" : "load";
    const ownerId = this.employeeId() || this.loadId() || "";

    this.uploadProgress.set({ [type]: 0 });

    try {
      const result = await this.api.invoke(uploadDocument$Json, {
        body: {
          OwnerType: ownerType,
          OwnerId: ownerId,
          File: file,
          Type: type,
          Description: `${type} document`,
        },
      });

      if (result.success) {
        this.toast.showSuccess(`${type} uploaded successfully`);
        this.refresh();
        this.changed.emit();
      }
      this.uploadProgress.set({ [type]: 100 });
    } catch {
      this.toast.showError(`Failed to upload ${type}`);
      this.uploadProgress.set({ [type]: 0 });
    }
  }

  protected async download(row: DocumentDto): Promise<void> {
    try {
      const blob = await this.api.invoke(downloadDocument$Json, { documentId: row.id! });
      const fileName = row.originalFileName || row.fileName;
      downloadBlobFile(blob, fileName!);
    } catch {
      this.toast.showError("Failed to download file");
    }
  }

  protected async delete(row: DocumentDto): Promise<void> {
    console.log(row);
    if (confirm(`Are you sure you want to delete "${row.fileName}"?`)) {
      try {
        const result = await this.api.invoke(deleteDocument$Json, { documentId: row.id! });
        if (result.success) {
          this.toast.showSuccess("Document deleted successfully");
          this.refresh();
          this.changed.emit();
        }
      } catch {
        this.toast.showError("Failed to delete document");
      }
    }
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

  protected formatFileSize(bytes: number): string {
    if (bytes === 0) {
      return "0 Bytes";
    }
    const k = 1024;
    const sizes = ["Bytes", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  }

  // Get a user-friendly label for the document type
  // Remove underscores and capitalize words from the enum value
  protected getTypeLabel(type: DocumentType): string {
    return type.replace(/_/g, " ").replace(/\b\w/g, (c) => c.toUpperCase());
  }
}
