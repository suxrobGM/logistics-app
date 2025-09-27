import { CustomerDto } from "../customer";
import { EmployeeDto } from "../employee";
import { MoneyDto } from "../money.dto";
import { PaymentDto } from "../payment/payment.model";
import { InvoiceStatus, InvoiceType } from "./enums";

export interface InvoiceDto {
  id: string;

  number: number;
  type: InvoiceType;
  status: InvoiceStatus;
  createdDate: Date;

  /**
   * Total inclusive of tax & discounts.
   */
  total: MoneyDto;

  notes?: string;
  dueDate?: Date; // ISO 8601 date string
  stripeInvoiceId?: string;

  payments: PaymentDto[];

  // LoadInvoice fields
  loadNumber: number;
  loadId?: string;
  customerId?: string;
  customer?: CustomerDto;

  // PayrollInvoice fields
  employeeId?: string;
  employee?: EmployeeDto;
  periodStart?: string;
  periodEnd?: string;

  // SubscriptionInvoice fields
  subscriptionId?: string;
  billingPeriodStart?: string;
  billingPeriodEnd?: string;
}
