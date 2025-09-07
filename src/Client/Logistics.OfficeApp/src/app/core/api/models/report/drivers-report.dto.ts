import { PagedResult } from "../paged-result";

export interface DriverReportDto {
  driverId: string;
  driverName: string;
  loadsDelivered: number;
  distanceDriven: number;
  grossEarnings: number;
  averageDistancePerLoad: number;
  averageEarningsPerLoad: number;
  efficiency: number;
  truckNumber: string;
  isMainDriver: boolean;
}

export interface DriverPerformanceDto {
  driverName: string;
  loadsDelivered: number;
  earnings: number;
  distance: number;
  efficiency: number;
}

export interface DriverTrendDto {
  period: string;
  activeDrivers: number;
  loadsDelivered: number;
  totalEarnings: number;
  totalDistance: number;
}

export interface DriverEfficiencyDto {
  metric: string;
  value: number;
  unit: string;
  trend: number;
}

export interface DriverDashboardDto {
  totalDrivers: number;
  activeDrivers: number;
  totalEarnings: number;
  totalDistance: number;
  totalLoadsDelivered: number;
  averageEarningsPerDriver: number;
  averageDistancePerDriver: number;
  topPerformers: DriverPerformanceDto[];
  driverTrends: DriverTrendDto[];
  efficiencyMetrics: DriverEfficiencyDto[];
}
