import { Observable } from "rxjs";
import { ApiBase } from "../api-base";
import { PagedIntervalQuery, PagedResult, Result } from "../models";
import { DriverDashboardDto, DriverReportDto } from "../models/report/drivers-report.dto";
import { FinancialsReportDto } from "../models/report/financials-report.dto";
import { LoadsReportDto } from "../models/report/loads-report.dto";
import { SearchableIntervalQuery } from "../models/searchable-interval.query";

export class ReportApiService extends ApiBase {
  getLoadsReport(query?: PagedIntervalQuery): Observable<Result<LoadsReportDto>> {
    return this.get(`/reports/loads?${this.stringfyPagedIntervalQuery(query)}`);
  }
  getDriversReport(query?: SearchableIntervalQuery): Observable<PagedResult<DriverReportDto>> {
    return this.get(
      `/reports/drivers?${this.stringfyPagedIntervalQuery(query, { search: query?.search })}`,
    );
  }
  getFinancialsReport(query?: PagedIntervalQuery): Observable<Result<FinancialsReportDto>> {
    return this.get(`/reports/financials?${this.stringfyPagedIntervalQuery(query)}`);
  }
  getDriverDashboard(query?: PagedIntervalQuery): Observable<Result<DriverDashboardDto>> {
    return this.get(`/reports/drivers/dashboard?${this.stringfyPagedIntervalQuery(query)}`);
  }
}
