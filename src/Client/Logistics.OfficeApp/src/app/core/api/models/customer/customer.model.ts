import { InvoiceDto } from "../invoice";

export interface CustomerDto {
  id: string;
  name: string;
  invoices: InvoiceDto[];
}
