import { Observable } from "rxjs";
import { ApiBase } from "../api-base";
import {
  CompanyStatsDto,
  DailyGrossesDto,
  MonthlyGrossesDto,
  PagedIntervalQuery,
  PagedResult,
  Result,
  TruckStatsDto,
} from "../models";

export class StatsApiService extends ApiBase {
  getCompanyStats(): Observable<Result<CompanyStatsDto>> {
    const url = `/stats/company`;
    return this.get(url);
  }

  getDailyGrosses(
    startDate: Date,
    endDate?: Date,
    truckId?: string,
  ): Observable<Result<DailyGrossesDto>> {
    let url = `/stats/daily-grosses?startDate=${startDate.toJSON()}`;

    if (endDate) {
      url += `&endDate=${endDate.toJSON()}`;
    }
    if (truckId) {
      url += `&truckId=${truckId}`;
    }

    return this.get(url);
  }

  getMonthlyGrosses(
    startDate: Date,
    endDate?: Date,
    truckId?: string,
  ): Observable<Result<MonthlyGrossesDto>> {
    let url = `/stats/monthly-grosses?startDate=${startDate.toJSON()}`;

    if (endDate) {
      url += `&endDate=${endDate.toJSON()}`;
    }
    if (truckId) {
      url += `&truckId=${truckId}`;
    }

    return this.get<Result<MonthlyGrossesDto>>(url);
  }

  getTrucksStats(query: PagedIntervalQuery): Observable<PagedResult<TruckStatsDto>> {
    const url = `/stats/trucks?${this.stringfyPagedIntervalQuery(query)}`;
    return this.get(url);
  }
}
