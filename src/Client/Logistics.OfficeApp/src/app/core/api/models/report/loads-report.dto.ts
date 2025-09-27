export interface StatusDto {
  status: string;
  count: number;
  totalRevenue: number;
}

export interface TypeDto {
  type: string;
  count: number;
  totalRevenue: number;
}

export interface LoadTrendDto {
  period: string;
  loadCount: number;
  revenue: number;
  distance: number;
}

export interface TopCustomerLoadDto {
  customerName: string;
  loadCount: number;
  totalRevenue: number;
  totalDistance: number;
  averageDistance: number;
}

export interface LoadPerformanceDto {
  metric: string;
  value: number;
  unit: string;
  trend: number;
}

export interface LoadsReportDto {
  totalRevenue: number;
  totalDistance: number;
  averageRevenuePerLoad: number;
  averageDistancePerLoad: number;
  statusBreakdown: StatusDto[];
  totalLoads: number;
  typeBreakdown: TypeDto[];
  cancelledLoadsRevenue: number;
  cancellationRate: number;
  loadTrends: LoadTrendDto[];
  topCustomers: TopCustomerLoadDto[];
  performanceMetrics: LoadPerformanceDto[];
}
