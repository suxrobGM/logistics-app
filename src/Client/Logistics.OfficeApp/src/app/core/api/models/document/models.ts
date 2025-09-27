import { PagedResult } from "../paged-result";

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

export enum DocumentOwnerType {
  Load = "Load",
  Employee = "Employee",
}

export enum DocumentStatus {
  Active = "Active",
  Archived = "Archived",
  Deleted = "Deleted",
}

export enum DocumentType {
  BillOfLading = "BillOfLading",
  ProofOfDelivery = "ProofOfDelivery",
  Invoice = "Invoice",
  Receipt = "Receipt",
  Contract = "Contract",
  InsuranceCertificate = "InsuranceCertificate",
  Photo = "Photo",
  DriverLicense = "DriverLicense",
  VehicleRegistration = "VehicleRegistration",
  IdentityDocument = "IdentityDocument",
  Other = "Other",
}
