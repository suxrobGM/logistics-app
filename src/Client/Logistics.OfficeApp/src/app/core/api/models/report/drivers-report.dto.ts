import { PagedResult } from "../paged-result";

export interface DriverReportDto {
  driverId : number;
  driverName : string;
  loadsDelivered : number;
  distanceDriven : number;
  grossEarnings : number;
}
