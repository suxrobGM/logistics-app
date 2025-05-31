import {SelectOption} from "@/shared/models";

export enum InvoiceType {
  Load = "load",
  Subscription = "subscription",
  Payroll = "payroll",
  Other = "other",
}

export enum InvoiceStatus {
  Draft = "draft",
  Issued = "issued",
  Paid = "paid",
  PartiallyPaid = "partially_paid",
  Cancelled = "cancelled",
}

export const invoiceStatusOptions: SelectOption<InvoiceStatus>[] = [
  {label: "Draft", value: InvoiceStatus.Draft},
  {label: "Issued", value: InvoiceStatus.Issued},
  {label: "Paid", value: InvoiceStatus.Paid},
  {label: "Partially Paid", value: InvoiceStatus.PartiallyPaid},
  {label: "Cancelled", value: InvoiceStatus.Cancelled},
] as const;

export const invoiceTypeOptions: SelectOption<InvoiceType>[] = [
  {label: "Load", value: InvoiceType.Load},
  {label: "Subscription", value: InvoiceType.Subscription},
  {label: "Payroll", value: InvoiceType.Payroll},
  {label: "Other", value: InvoiceType.Other},
] as const;
