import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AppConfig } from '@app/configs/app.config';
import { catchError, Observable, retry, throwError } from 'rxjs';
import { DataResult } from '../models/data-result';
import { Employee } from '../models/employee';
import { Load } from '../models/load';
import { PagedDataResult } from '../models/paged-data-result';
import { Tenant } from '../models/tenant';
import { Truck } from '../models/truck';
import { TenantService } from './tenant.service';

@Injectable({
  providedIn: 'root'
})
export class ApiClientService {
  private host = AppConfig.apiHost;
  private retryCount = 1;

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
      .pipe(retry(1));
  }

  //#endregion

  //#region Load API
  
  getLoad(id: string): Observable<DataResult<Load>> {  
    const url = `${this.host}/load/${id}`;
    return this.httpClient
      .get<DataResult<Load>>(url)
      .pipe(retry(this.retryCount));
  }

  getLoads(searchQuery = '', page = 1, pageSize = 10): Observable<PagedDataResult<Load>> {  
    const url = `${this.host}/load/list?search=${searchQuery}page=${page}&pageSize=${pageSize}`;
    return this.httpClient
      .get<PagedDataResult<Load>>(url)
      .pipe(retry(this.retryCount));
  }

  createLoad(load: Load): Observable<DataResult<any>> {  
    const url = `${this.host}/load/create`;
    const headers = { 'content-type': 'application/json'}  
    const body = JSON.stringify(load);

    return this.httpClient
      .post<DataResult<any>>(url, body, { headers: headers })
      .pipe(retry(this.retryCount));
  }

  updateLoad(load: Load): Observable<DataResult<any>> {  
    const url = `${this.host}/load/update/${load.id}`;
    const headers = { 'content-type': 'application/json'}  
    const body = JSON.stringify(load);

    return this.httpClient
      .put<DataResult<any>>(url, body, { headers: headers })
      .pipe(retry(this.retryCount));
  }

  deleteLoad(loadId: string): Observable<DataResult<any>> {  
    const url = `${this.host}/load/delete/${loadId}`; 

    return this.httpClient
      .delete<DataResult<any>>(url)
      .pipe(retry(this.retryCount));
  }

  //#endregion


  //#region Truck API
  
  getTruck(id: string): Observable<DataResult<Truck>> {  
    const url = `${this.host}/truck/${id}`;
    return this.httpClient
      .get<DataResult<Truck>>(url)
      .pipe(retry(this.retryCount));
  }

  getTrucks(searchQuery = '', page = 1, pageSize = 10): Observable<PagedDataResult<Truck>> {  
    const url = `${this.host}/truck/list?search=${searchQuery}page=${page}&pageSize=${pageSize}`;
    return this.httpClient
      .get<PagedDataResult<Truck>>(url)
      .pipe(retry(this.retryCount));
  }

  createTruck(truck: Truck): Observable<DataResult<any>> {  
    const url = `${this.host}/truck/create`;
    const headers = { 'content-type': 'application/json'}  
    const body = JSON.stringify(truck);

    return this.httpClient
      .post<DataResult<any>>(url, body, { headers: headers })
      .pipe(retry(this.retryCount));
  }

  updateTruck(truck: Truck): Observable<DataResult<any>> {  
    const url = `${this.host}/truck/update/${truck.id}`;
    const headers = { 'content-type': 'application/json'}  
    const body = JSON.stringify(truck);

    return this.httpClient
      .put<DataResult<any>>(url, body, { headers: headers })
      .pipe(retry(this.retryCount));
  }

  deleteTruck(truckId: string): Observable<DataResult<any>> {  
    const url = `${this.host}/truck/delete/${truckId}`; 

    return this.httpClient
      .delete<DataResult<any>>(url)
      .pipe(retry(this.retryCount));
  }

  //#endregion


  //#region Employee API
  
  getEmployee(id: string): Observable<DataResult<Employee>> {  
    const url = `${this.host}/employee/${id}`;
    return this.httpClient
      .get<DataResult<Employee>>(url)
      .pipe(retry(this.retryCount));
  }

  getEmployees(searchQuery = '', page = 1, pageSize = 10): Observable<PagedDataResult<Employee>> {  
    const url = `${this.host}/employee/list?search=${searchQuery}page=${page}&pageSize=${pageSize}`;
    return this.httpClient
      .get<PagedDataResult<Employee>>(url)
      .pipe(retry(this.retryCount));
  }

  createEmployee(employee: Employee): Observable<DataResult<any>> {  
    const url = `${this.host}/employee/create`;
    const headers = { 'content-type': 'application/json'}  
    const body = JSON.stringify(employee);

    return this.httpClient
      .post<DataResult<any>>(url, body, { headers: headers })
      .pipe(retry(this.retryCount));
  }

  updateEmployee(employee: Employee): Observable<DataResult<any>> {  
    const url = `${this.host}/employee/update/${employee.id}`;
    const headers = { 'content-type': 'application/json'}  
    const body = JSON.stringify(employee);

    return this.httpClient
      .put<DataResult<any>>(url, body, { headers: headers })
      .pipe(retry(this.retryCount));
  }

  deleteEmployee(employeeId: string): Observable<DataResult<any>> {  
    const url = `${this.host}/employee/delete/${employeeId}`; 

    return this.httpClient
      .delete<DataResult<any>>(url)
      .pipe(retry(this.retryCount));
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
