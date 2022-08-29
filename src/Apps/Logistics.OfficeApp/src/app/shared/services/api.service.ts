import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';
import { catchError, Observable, of, retry } from 'rxjs';
import { AppConfig } from '../../configs/app.config';
import { TenantService  } from './tenant.service';
import { 
  DataResult, 
  Employee, 
  PagedDataResult, 
  Tenant, 
  Truck, 
  User, 
  Load, 
  Role 
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private host = AppConfig.apiHost;
  private retryCount = 0;

  constructor(
    private httpClient: HttpClient,
    private tenantService: TenantService,
    private messageService: MessageService) 
  {
  }

  //#region Tenant API

  getTenant(): Observable<DataResult<Tenant>> {
    const tenantId = this.tenantService.getTenant();
    const url = `${this.host}/tenant/displayName/${tenantId}`;
    return this.httpClient
      .get<DataResult<any>>(url)
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  //#endregion


  //#region User API

    getUsers(searchQuery = '', orderBy = '', page = 1, pageSize = 10): Observable<PagedDataResult<User>> { 
      if (!searchQuery) {
        searchQuery = ''
      }

      if (!orderBy) {
        orderBy = ''
      }
      
      const url = `${this.host}/user/list?search=${searchQuery}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
      return this.httpClient
        .get<PagedDataResult<User>>(url)
        .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
    }

  //#endregion


  //#region Load API
  
  getLoad(id: string): Observable<DataResult<Load>> {  
    const url = `${this.host}/load/${id}`;
    return this.httpClient
      .get<DataResult<Load>>(url)
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  getLoads(searchQuery = '', orderBy = '', page = 1, pageSize = 10): Observable<PagedDataResult<Load>> { 
    if (!searchQuery) {
      searchQuery = ''
    }

    if (!orderBy) {
      orderBy = ''
    }
    
    const url = `${this.host}/load/list?search=${searchQuery}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
    return this.httpClient
      .get<PagedDataResult<Load>>(url)
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  createLoad(load: Load): Observable<DataResult<any>> {  
    const url = `${this.host}/load/create`;
    const headers = { 'content-type': 'application/json'}  
    const body = JSON.stringify(load);

    return this.httpClient
      .post<DataResult<any>>(url, body, { headers: headers })
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  updateLoad(load: Load): Observable<DataResult<any>> {  
    const url = `${this.host}/load/update/${load.id}`;
    const headers = { 'content-type': 'application/json'}  
    const body = JSON.stringify(load);

    return this.httpClient
      .put<DataResult<any>>(url, body, { headers: headers })
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  deleteLoad(loadId: string): Observable<DataResult<any>> {  
    const url = `${this.host}/load/delete/${loadId}`; 

    return this.httpClient
      .delete<DataResult<any>>(url)
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  //#endregion


  //#region Truck API
  
  getTruck(id: string): Observable<DataResult<Truck>> {  
    const url = `${this.host}/truck/${id}`;
    return this.httpClient
      .get<DataResult<Truck>>(url)
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  getTrucks(searchQuery = '', orderBy = '', page = 1, pageSize = 10): Observable<PagedDataResult<Truck>> {  
    if (!searchQuery) {
      searchQuery = ''
    }

    if (!orderBy) {
      orderBy = ''
    }

    const url = `${this.host}/truck/list?search=${searchQuery}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
    return this.httpClient
      .get<PagedDataResult<Truck>>(url)
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  createTruck(truck: Truck): Observable<DataResult<any>> {  
    const url = `${this.host}/truck/create`;
    const headers = { 'content-type': 'application/json'}  
    const body = JSON.stringify(truck);

    return this.httpClient
      .post<DataResult<any>>(url, body, { headers: headers })
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  updateTruck(truck: Truck): Observable<DataResult<any>> {  
    const url = `${this.host}/truck/update/${truck.id}`;
    const headers = { 'content-type': 'application/json'}  
    const body = JSON.stringify(truck);

    return this.httpClient
      .put<DataResult<any>>(url, body, { headers: headers })
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  deleteTruck(truckId: string): Observable<DataResult<any>> {  
    const url = `${this.host}/truck/delete/${truckId}`; 

    return this.httpClient
      .delete<DataResult<any>>(url)
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  //#endregion


  //#region Employee API
  
  getEmployee(id: string): Observable<DataResult<Employee>> {  
    const url = `${this.host}/employee/${id}`;
    return this.httpClient
      .get<DataResult<Employee>>(url)
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  getEmployees(searchQuery = '', orderBy = '', page = 1, pageSize = 10): Observable<PagedDataResult<Employee>> {
    if (!searchQuery) {
      searchQuery = ''
    }

    if (!orderBy) {
      orderBy = ''
    }

    const url = `${this.host}/employee/list?search=${searchQuery}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
    return this.httpClient
      .get<PagedDataResult<Employee>>(url)
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  getDrivers(searchQuery = '', page = 1, pageSize = 10): Observable<PagedDataResult<Employee>> {  
    if (!searchQuery) {
      searchQuery = ''
    }

    const url = `${this.host}/employee/drivers?search=${searchQuery}&page=${page}&pageSize=${pageSize}`;
    return this.httpClient
      .get<PagedDataResult<Employee>>(url)
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  createEmployee(employee: Employee): Observable<DataResult<any>> {  
    const url = `${this.host}/employee/create`;
    const headers = { 'content-type': 'application/json'}  
    const body = JSON.stringify(employee);

    return this.httpClient
      .post<DataResult<any>>(url, body, { headers: headers })
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  updateEmployee(employee: Employee): Observable<DataResult<any>> {  
    const url = `${this.host}/employee/update/${employee.id}`;
    const headers = { 'content-type': 'application/json'}  
    const body = JSON.stringify(employee);

    return this.httpClient
      .put<DataResult<any>>(url, body, { headers: headers })
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  deleteEmployee(employeeId: string): Observable<DataResult<any>> {  
    const url = `${this.host}/employee/delete/${employeeId}`; 

    return this.httpClient
      .delete<DataResult<any>>(url)
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  //#endregion


  //#region Tenant Role API

  getRoles(searchQuery = '', page = 1, pageSize = 10): Observable<PagedDataResult<Role>> {
    if (!searchQuery) {
      searchQuery = ''
    }
    
    const url = `${this.host}/tenantRole/list?search=${searchQuery}&page=${page}&pageSize=${pageSize}`;
    return this.httpClient
      .get<PagedDataResult<Role>>(url)
      .pipe(retry(this.retryCount), catchError((err) => this.handleError(err)));
  }

  //#endregion
  
  public parseSortProperty(sortField?: string, sortOrder?: number) {
    if (!sortOrder) {
      sortOrder = 1;
    }

    if (!sortField) {
      sortField = '';
    }

    if (sortOrder <= -1) {
      return `-${sortField}`;
    } else {
      return sortField;
    }
  }

  private handleError(responseData: any): Observable<any> {
    const errorMessage = responseData.error.error;
    
    this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: errorMessage});
    this.messageService.add({key: 'errorMsg', severity: 'error', summary: 'Error', detail: errorMessage});
    console.error(errorMessage ?? responseData);
    return of({error: errorMessage, success: false});
  }
}
