import { HttpClient } from "@angular/common/http";
import { inject } from "@angular/core";
import { Observable, catchError, of } from "rxjs";
import { ToastService } from "../services";
import { API_CONFIG } from "./api.provider";
import { PagedIntervalQuery, SearchableQuery } from "./models";

export abstract class ApiBase {
  private readonly http = inject(HttpClient);
  private readonly toastService = inject(ToastService);
  private readonly apiConfig = inject(API_CONFIG);
  private readonly headers = { "content-type": "application/json" };
  protected readonly apiUrl = this.apiConfig.baseUrl;

  /**
   * Utility function to parse the sort property from the query string.
   * @param sortField Sort field name
   * @param sortOrder Sort order (1 for ascending, -1 for descending)
   * @returns The parsed sort property string.
   * @example
   * const sortProperty = this.parseSortProperty("name", -1); // returns "-name"
   */
  formatSortField(sortField?: string | null, sortOrder?: number | null): string {
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
      .post<TResponse>(this.apiUrl + endpoint, bodyJson, { headers: this.headers })
      .pipe(catchError((err) => this.handleError(err)));
  }

  protected put<TResponse, TBody>(endpoint: string, body: TBody): Observable<TResponse> {
    const bodyJson = JSON.stringify(body);

    return this.http
      .put<TResponse>(this.apiUrl + endpoint, bodyJson, { headers: this.headers })
      .pipe(catchError((err) => this.handleError(err)));
  }

  protected delete<TResponse>(endpoint: string): Observable<TResponse> {
    return this.http
      .delete<TResponse>(this.apiUrl + endpoint)
      .pipe(catchError((err) => this.handleError(err)));
  }

  /** Multipart/form-data POST helper */
  protected postFormData<TResponse>(endpoint: string, formData: FormData): Observable<TResponse> {
    return this.http
      .post<TResponse>(this.apiUrl + endpoint, formData)
      .pipe(catchError((err) => this.handleError(err)));
  }

  /** Download blob helper */
  protected getBlob(endpoint: string): Observable<Blob> {
    return this.http
      .get(this.apiUrl + endpoint, { responseType: "blob" })
      .pipe(catchError((err) => this.handleError(err)));
  }

  /**
   * Converts a query object to a query string.
   * @param query The query object to convert.
   * @returns The query string representation of the object.
   * @example
   * ```typescript
   * const query = {name: "John", age: 30};
   * const queryString = this.stringfyQuery(query); // returns "name=John&age=30"
   * ```
   */
  protected stringfyQuery(query?: object): string {
    if (!query) {
      return "";
    }

    const params = new URLSearchParams();
    for (const [key, value] of Object.entries(query)) {
      if (value === undefined) {
        continue;
      }

      if (value instanceof Date) {
        params.set(key, value.toJSON());
      } else {
        params.set(key, value.toString());
      }
    }
    return params.toString();
  }

  protected stringfySearchableQuery(query?: SearchableQuery): string {
    const { search = "", orderBy = "", page = 1, pageSize = 10 } = query || {};

    return new URLSearchParams({
      search,
      orderBy: orderBy,
      page: page.toString(),
      pageSize: pageSize.toString(),
    }).toString();
  }

  protected stringfyPagedIntervalQuery(
    query?: PagedIntervalQuery,
    additionalParams: Record<string, string | undefined> = {},
  ): string {
    const { startDate = new Date(), endDate, orderBy = "", page = 1, pageSize = 10 } = query || {};

    // Filter out undefined values from additionalParams
    const filteredAdditionalParams = Object.fromEntries(
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      Object.entries(additionalParams).filter(([_, value]) => value !== undefined),
    );

    const params = new URLSearchParams({
      startDate: startDate.toJSON(),
      orderBy: orderBy,
      page: page.toString(),
      pageSize: pageSize.toString(),
      ...filteredAdditionalParams,
    });

    if (endDate) params.set("endDate", endDate.toJSON());
    return params.toString();
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  protected handleError(responseData: any): Observable<any> {
    const errorMessage = responseData.error?.error ?? responseData.error;

    this.toastService.showError(errorMessage);
    console.error(errorMessage ?? responseData);
    return of({ error: errorMessage, success: false });
  }
}
