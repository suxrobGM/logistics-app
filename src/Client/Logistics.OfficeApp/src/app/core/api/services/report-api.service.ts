import {Observable} from "rxjs";
import {ApiBase} from "../api-base";
import {
  Result,
  PagedResult,
  PagedIntervalQuery,
} from "../models";
import { FinancialsReportDto } from "../models/report/financials-report.dto";
import { LoadsReportDto } from "../models/report/loads-report.dto";
import { DriverReportDto } from "../models/report/drivers-report.dto";
import { SearchableIntervalQuery } from "../models/searchable-interval.query";

export class ReportApiService extends ApiBase {
  getLoadsReport(query?: PagedIntervalQuery): Observable<Result<LoadsReportDto>> {
    return this.get(`/reports/loads?${this.stringfyPagedIntervalQuery(query)}`);
  }
  getDriversReport(query?: SearchableIntervalQuery): Observable<PagedResult<DriverReportDto>> {
    return this.get(`/reports/drivers?${this.stringfyPagedIntervalQuery(query, {search : query?.search})}`);
  }
  getFinancialsReport(query?: PagedIntervalQuery): Observable<Result<FinancialsReportDto>> {
    return this.get(`/reports/financials?${this.stringfyPagedIntervalQuery(query)}`);
  }
}