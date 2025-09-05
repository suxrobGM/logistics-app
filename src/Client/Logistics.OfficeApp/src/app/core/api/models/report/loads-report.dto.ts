
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
export interface LoadsReportDto {
  totalRevenue : number;
  totalDistance : number;
  statusBreakdown : LoadStatusDto[];
  totalLoads : number;
  typeBreakdown: TypeDto[];
  cancelledLoadsRevenue: number;
  cancellationRate: number;
}
export interface LoadStatusDto {
  status: string;
  count: number;
}