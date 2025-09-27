import { InvoiceStatus } from "./enums";

export interface UpdateInvoiceCommand {
  id: string;
  invoiceStatus: InvoiceStatus;
}
