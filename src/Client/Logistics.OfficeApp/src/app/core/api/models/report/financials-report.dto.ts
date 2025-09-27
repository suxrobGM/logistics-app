export interface FinancialsReportDto {
  totalPaid: number;
  totalDue: number;
  totalInvoiced: number;
  fullyPaidInvoices: number;
  partiallyPaidInvoices: number;
  unpaidInvoices: number;
  averageInvoiceValue: number;
  collectionRate: number;
  outstandingAmount: number;
  overdueAmount: number;
  overdueInvoices: number;
  paymentTrends: PaymentTrendDto[];
  topCustomers: TopCustomerDto[];
  statusDistribution: StatusDistributionDto;
  revenueTrends: RevenueTrendDto[];
  financialMetrics: FinancialMetricDto[];
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

export interface RevenueTrendDto {
  period: string;
  revenue: number;
  profit: number;
  expenses: number;
  profitMargin: number;
}

export interface FinancialMetricDto {
  metric: string;
  value: number;
  unit: string;
  trend: number;
  category: string;
}
