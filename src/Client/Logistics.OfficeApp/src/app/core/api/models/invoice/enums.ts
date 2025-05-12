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
