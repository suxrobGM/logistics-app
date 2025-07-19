import {PagedQuery} from "../paged.query";
import {InvoiceType} from "./enums";

export interface GetInvoicesQuery extends PagedQuery {
  /**
   * Filter payrolls by Employee ID
   */
  employeeId?: string;

  /**
   * Filter payrolls by Employee Name
   */
  employeeName?: string;

  /**
   * Filter payrolls by Load ID
   */
  loadId?: string;

  /**
   * Filter invoices by type
   */
  invoiceType?: InvoiceType;
}
