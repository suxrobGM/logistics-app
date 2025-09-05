export interface FinancialsReportDto{
  totalPaid : number;
  totalDue : number;
  totalInvoiced : number;
  fullyPaidInvoices :number;
  partiallyPaidInvoices : number;
  unpaidInvoices : number;
  averageInvoiceValue : number;
  collectionRate : number;
  paymentTrends: PaymentTrendDto[];
  topCustomers: TopCustomerDto[];
  statusDistribution: StatusDistributionDto;
}

export interface PaymentTrendDto {
  period: string;
  invoiced: number;
  paid: number;
  collectionRate: number;
}

export interface TopCustomerDto {
  customerName: string;
  totalInvoiced: number;
  totalPaid: number;
  invoiceCount: number;
}

export interface StatusDistributionDto {
  paidPercentage: number;
  partialPercentage: number;
  unpaidPercentage: number;
}