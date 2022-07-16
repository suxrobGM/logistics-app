import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AppConfig } from '@app/configs/app.config';
import { catchError, Observable, retry, throwError } from 'rxjs';
import { DataResult } from '../models/data-result';
import { Load } from '../models/load';
import { PagedDataResult } from '../models/paged-data-result';
import { Tenant } from '../models/tenant';
import { TenantService } from './tenant.service';

@Injectable({
  providedIn: 'root'
})
export class ApiClientService {
  private host = AppConfig.apiHost;

  constructor(
    private httpClient: HttpClient,
    private tenantService: TenantService) 
  {
  }

  //#region Tenant API

  getTenant(): Observable<DataResult<Tenant>> {
    const tenantId = this.tenantService.getTenant();
    const url = `${this.host}/tenant/displayName/${tenantId}`;
    return this.httpClient
      .get<DataResult<any>>(url)
      .pipe(retry(3));
  }

  //#endregion

  //#region Load API
  
  getLoad(id: string): Observable<DataResult<Load>> {  
    const url = `${this.host}/load/id/${id}`;
    return this.httpClient
      .get<DataResult<Load>>(url)
      .pipe(retry(3));
  }

  getLoads(searchQuery = '', page = 1, pageSize = 10): Observable<PagedDataResult<Load>> {  
    const url = `${this.host}/load/list?search=${searchQuery}page=${page}&pageSize=${pageSize}`;
    return this.httpClient
      .get<PagedDataResult<Load>>(url)
      .pipe(retry(3));
  }

  createLoad(load: Load): Observable<DataResult<any>> {  
    const url = `${this.host}/load/create`;
    const headers = { 'content-type': 'application/json'}  
    const body = JSON.stringify(load);

    return this.httpClient
      .post<DataResult<any>>(url, body, { headers: headers })
      .pipe(retry(3));
  }

  updateLoad(load: Load): Observable<DataResult<any>> {  
    const url = `${this.host}/load/update/${load.id}`;
    const headers = { 'content-type': 'application/json'}  
    const body = JSON.stringify(load);

    return this.httpClient
      .put<DataResult<any>>(url, body, { headers: headers })
      .pipe(retry(3));
  }

  //#endregion

  private handleError(responseData: any): Observable<any> {
    let errorMessage = '';
    if (responseData.error instanceof ErrorEvent) {
      // Get client-side error
      errorMessage = responseData.error.message;
    } else {
      // Get server-side error
      errorMessage = `Error Code: ${responseData.status}\nMessage: ${responseData.message}`;
    }
    window.alert(errorMessage);
    return throwError(() => {
      return errorMessage;
    });
  }
}
