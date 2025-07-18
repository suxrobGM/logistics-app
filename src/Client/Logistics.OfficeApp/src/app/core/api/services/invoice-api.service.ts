import {Observable} from "rxjs";
import {ApiBase} from "../api-base";
import {
  CreateLoadInvoiceCommand,
  CreatePayrollInvoiceCommand,
  GetInvoicesQuery,
  InvoiceDto,
  PagedResult,
  PreviewPayrollInvoicesQuery,
  Result,
  UpdateInvoiceCommand,
  UpdatePayrollInvoiceCommand,
} from "../models";

export class InvoiceApiService extends ApiBase {
  getInvoice(id: string): Observable<Result<InvoiceDto>> {
    return this.get(`/invoices/${id}`);
  }

  getInvoices(query?: GetInvoicesQuery): Observable<PagedResult<InvoiceDto>> {
    return this.get(`/invoices?${this.stringfyQuery(query)}`);
  }

  updateInvoice(command: UpdateInvoiceCommand): Observable<Result> {
    return this.put(`/invoices/${command.id}`, command);
  }

  deleteInvoice(invoiceId: string): Observable<Result> {
    return this.delete(`/invoices/${invoiceId}`);
  }

  //#region Load Invoices

  createLoadInvoice(command: CreateLoadInvoiceCommand): Observable<Result> {
    return this.post("/invoices/loads", command);
  }

  //#endregion

  //#region Payroll Invoices

  previewPayrollInvoice(query: PreviewPayrollInvoicesQuery): Observable<Result<InvoiceDto>> {
    return this.get(`/invoices/payrolls/preview?${this.stringfyQuery(query)}`);
  }

  createPayrollInvoice(command: CreatePayrollInvoiceCommand): Observable<Result> {
    return this.post(`/invoices/payrolls`, command);
  }

  updatePayrollInvoice(command: UpdatePayrollInvoiceCommand): Observable<Result> {
    return this.put(`/invoices/payrolls/${command.id}`, command);
  }

  //#endregion
}
