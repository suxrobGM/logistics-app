import { Injectable, inject } from "@angular/core";
import { downloadBlobFile } from "@logistics/shared";
import { Api, downloadLoadInvoicePdf, downloadPayrollPayStubPdf } from "@logistics/shared/api";

export interface PdfGeneratorOptions {
  filename?: string;
}

/**
 * Service for downloading PDF documents from the backend.
 */
@Injectable({ providedIn: "root" })
export class PdfService {
  private readonly api = inject(Api);

  /**
   * Download a load invoice PDF from the backend.
   * @param invoiceId The invoice ID
   * @param options Optional settings for the download
   */
  async downloadLoadInvoicePdf(invoiceId: string, options?: PdfGeneratorOptions): Promise<void> {
    const response = await this.api.invoke$Response(downloadLoadInvoicePdf, { id: invoiceId });
    const blob = response.body;

    // Get filename from Content-Disposition header or use default
    const contentDisposition = response.headers.get("Content-Disposition");
    let filename = options?.filename ?? `invoice-${invoiceId}.pdf`;

    if (contentDisposition) {
      const filenameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
      if (filenameMatch && filenameMatch[1]) {
        filename = filenameMatch[1].replace(/['"]/g, "");
      }
    }

    // Create a temporary anchor element and trigger download
    downloadBlobFile(blob, filename);
  }

  /**
   * Download a payroll pay stub PDF from the backend.
   * @param payrollId The payroll invoice ID
   * @param options Optional settings for the download
   */
  async downloadPayrollPayStubPdf(payrollId: string, options?: PdfGeneratorOptions): Promise<void> {
    const response = await this.api.invoke$Response(downloadPayrollPayStubPdf, { id: payrollId });
    const blob = response.body;

    // Get filename from Content-Disposition header or use default
    const contentDisposition = response.headers.get("Content-Disposition");
    let filename = options?.filename ?? `paystub-${payrollId}.pdf`;

    if (contentDisposition) {
      const filenameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
      if (filenameMatch && filenameMatch[1]) {
        filename = filenameMatch[1].replace(/['"]/g, "");
      }
    }

    // Create a temporary anchor element and trigger download
    downloadBlobFile(blob, filename);
  }
}
