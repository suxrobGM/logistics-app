import {HttpClient} from "@angular/common/http";
import {inject} from "@angular/core";
import {Observable, catchError, of} from "rxjs";
import {PagedIntervalQuery, SearchableQuery} from "../models";
import {ToastService} from "../services";

export abstract class ApiBase {
  private readonly headers = {"content-type": "application/json"};
  private readonly toastService = inject(ToastService);
  protected readonly http: HttpClient;
  protected readonly apiUrl: string;

  constructor(apiUrl: string, http: HttpClient) {
    this.apiUrl = apiUrl;
    this.http = http;
  }

  /**
   * Utility function to parse the sort property from the query string.
   * @param sortField Sort field name
   * @param sortOrder Sort order (1 for ascending, -1 for descending)
   * @returns The parsed sort property string.
   * @example
    ```typescript
      const sortProperty = this.parseSortProperty("name", -1); // returns "-name"
    ``` 
   */
  parseSortProperty(sortField?: string | null, sortOrder?: number | null): string {
    if (!sortOrder) {
      sortOrder = 1;
    }

    if (!sortField) {
      sortField = "";
    }

    return sortOrder <= -1 ? `-${sortField}` : sortField;
  }

  protected get<TResponse>(endpoint: string): Observable<TResponse> {
    return this.http
      .get<TResponse>(this.apiUrl + endpoint)
      .pipe(catchError((err) => this.handleError(err)));
  }

  protected post<TResponse, TBody>(endpoint: string, body: TBody): Observable<TResponse> {
    const bodyJson = JSON.stringify(body);

    return this.http
      .post<TResponse>(this.apiUrl + endpoint, bodyJson, {headers: this.headers})
      .pipe(catchError((err) => this.handleError(err)));
  }

  protected put<TResponse, TBody>(endpoint: string, body: TBody): Observable<TResponse> {
    const bodyJson = JSON.stringify(body);

    return this.http
      .put<TResponse>(this.apiUrl + endpoint, bodyJson, {headers: this.headers})
      .pipe(catchError((err) => this.handleError(err)));
  }

  protected delete<TResponse>(endpoint: string): Observable<TResponse> {
    return this.http
      .delete<TResponse>(this.apiUrl + endpoint)
      .pipe(catchError((err) => this.handleError(err)));
  }

  protected stringfySearchableQuery(query?: SearchableQuery): string {
    const search = query?.search ?? "";
    const orderBy = query?.orderBy ?? "";
    const page = query?.page ?? 1;
    const pageSize = query?.pageSize ?? 10;
    return `search=${search}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
  }

  protected stringfyPagedIntervalQuery(query?: PagedIntervalQuery): string {
    const startDate = query?.startDate.toJSON() ?? new Date().toJSON();
    const endDate = query?.endDate?.toJSON();
    const orderBy = query?.orderBy ?? "";
    const page = query?.page ?? 1;
    const pageSize = query?.pageSize ?? 10;
    let queryStr = `startDate=${startDate}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;

    if (endDate) {
      queryStr += `&endDate=${endDate}`;
    }

    return queryStr;
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  protected handleError(responseData: any): Observable<any> {
    const errorMessage = responseData.error?.error ?? responseData.error;

    this.toastService.showError(errorMessage);
    console.error(errorMessage ?? responseData);
    return of({error: errorMessage, success: false});
  }
}
