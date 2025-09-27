import { Observable } from "rxjs";
import { ApiBase } from "../api-base";
import {
  DocumentDto,
  GetDocumentsQuery,
  UpdateDocumentCommand,
  UploadDocumentRequest,
} from "../models";
import { Result } from "../models";

export class DocumentApiService extends ApiBase {
  getDocument(id: string): Observable<Result<DocumentDto>> {
    return this.get(`/documents/${id}`);
  }

  getDocuments(query?: GetDocumentsQuery): Observable<Result<DocumentDto[]>> {
    return this.get(`/documents?${this.stringfyQuery(query)}`);
  }

  uploadDocument(request: UploadDocumentRequest): Observable<Result<string>> {
    const formData = new FormData();
    formData.append("OwnerType", request.ownerType);
    formData.append("OwnerId", request.ownerId);
    formData.append("File", request.file);
    formData.append("Type", request.type);
    if (request.description) {
      formData.append("Description", request.description);
    }
    return this.postFormData("/documents/upload", formData);
  }

  updateDocument(command: UpdateDocumentCommand): Observable<Result> {
    return this.put(`/documents/${command.id}`, command);
  }

  deleteDocument(documentId: string): Observable<Result> {
    return this.delete(`/documents/${documentId}`);
  }

  downloadFile(documentId: string): Observable<Blob> {
    return this.getBlob(`/documents/${documentId}/download`);
  }
}
