import { PagedResult } from "../paged-result";
import { DocumentOwnerType, DocumentStatus, DocumentType } from "./enums";

export interface DocumentDto {
  id: string;
  fileName: string;
  originalFileName: string;
  contentType: string;
  fileSizeBytes: number;
  blobPath: string;
  blobContainer: string;
  type: DocumentType;
  status: DocumentStatus;
  description?: string;
  uploadedById: string;
  loadId?: string;
  employeeId?: string;
  createdAt: Date;
  updatedAt?: Date;
}

export interface UploadDocumentRequest {
  ownerType: DocumentOwnerType;
  ownerId: string;
  file: File;
  type: DocumentType;
  description?: string;
}

export interface UpdateDocumentCommand {
  id: string;
  description?: string;
}

export interface GetDocumentsQuery {
  ownerType?: DocumentOwnerType;
  ownerId?: string;
  status?: DocumentStatus;
  type?: DocumentType;
}

export type PagedDocuments = PagedResult<DocumentDto>;
