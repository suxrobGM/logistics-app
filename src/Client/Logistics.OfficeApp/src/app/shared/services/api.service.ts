import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';
import { catchError, Observable, of } from 'rxjs';
import { AppConfig } from '../../configs/app.config';
import { TenantService  } from './tenant.service';
import {
  ResponseResult,
  Employee,
  PagedResponseResult,
  Tenant,
  Truck,
  User,
  Load,
  Role,
  DailyGrosses,
  MonthlyGrosses,
  OverallStats,
  UpdateEmployee,
  CreateEmployee,
  RemoveEmployeeRole
} from '../models';

@Injectable()
export class ApiService {
  private host = AppConfig.apiHost;

  constructor(
    private httpClient: HttpClient,
    private tenantService: TenantService,
    private messageService: MessageService) 
  {
  }

  //#region Tenant API

  getTenant(): Observable<ResponseResult<Tenant>> {
    const tenantId = this.tenantService.getTenantId();
    const url = `${this.host}/tenant/getDisplayName?id=${tenantId}`;

    return this.httpClient
      .get<ResponseResult<Tenant>>(url)
      .pipe(catchError((err) => this.handleError(err)));
  }

  //#endregion


  //#region User API

    getUsers(searchQuery = '', orderBy = '', page = 1, pageSize = 10): Observable<PagedResponseResult<User>> { 
      if (!searchQuery) {
        searchQuery = ''
      }

      if (!orderBy) {
        orderBy = ''
      }
      
      const url = `${this.host}/user/list?search=${searchQuery}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
      return this.httpClient
        .get<PagedResponseResult<User>>(url)
        .pipe(catchError((err) => this.handleError(err)));
    }

  //#endregion


  //#region Load API
  
  getLoad(id: string): Observable<ResponseResult<Load>> {  
    const url = `${this.host}/load/${id}`;
    return this.httpClient
      .get<ResponseResult<Load>>(url)
      .pipe(catchError((err) => this.handleError(err)));
  }

  getLoads(searchQuery = '', orderBy = '', page = 1, pageSize = 10): Observable<PagedResponseResult<Load>> { 
    if (!searchQuery) {
      searchQuery = ''
    }

    if (!orderBy) {
      orderBy = ''
    }
    
    const url = `${this.host}/load/list?search=${searchQuery}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
    return this.httpClient
      .get<PagedResponseResult<Load>>(url)
      .pipe(catchError((err) => this.handleError(err)));
  }

  createLoad(load: Load): Observable<ResponseResult> {  
    const url = `${this.host}/load/create`;
    const headers = {'content-type': 'application/json'}  
    const body = JSON.stringify(load);

    return this.httpClient
      .post<ResponseResult>(url, body, { headers: headers })
      .pipe(catchError((err) => this.handleError(err)));
  }

  updateLoad(load: Load): Observable<ResponseResult> {  
    const url = `${this.host}/load/update/${load.id}`;
    const headers = {'content-type': 'application/json'}  
    const body = JSON.stringify(load);

    return this.httpClient
      .put<ResponseResult>(url, body, { headers: headers })
      .pipe(catchError((err) => this.handleError(err)));
  }

  deleteLoad(loadId: string): Observable<ResponseResult> {  
    const url = `${this.host}/load/delete/${loadId}`; 

    return this.httpClient
      .delete<ResponseResult>(url)
      .pipe(catchError((err) => this.handleError(err)));
  }

  //#endregion


  //#region Truck API
  
  getTruck(id: string): Observable<ResponseResult<Truck>> {  
    const url = `${this.host}/truck/${id}`;
    return this.httpClient
      .get<ResponseResult<Truck>>(url)
      .pipe(catchError((err) => this.handleError(err)));
  }

  getTrucks(searchQuery = '', orderBy = '', page = 1, pageSize = 10): Observable<PagedResponseResult<Truck>> {  
    if (!searchQuery) {
      searchQuery = ''
    }

    if (!orderBy) {
      orderBy = ''
    }

    const url = `${this.host}/truck/list?search=${searchQuery}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
    return this.httpClient
      .get<PagedResponseResult<Truck>>(url)
      .pipe(catchError((err) => this.handleError(err)));
  }

  createTruck(truck: Truck): Observable<ResponseResult> {  
    const url = `${this.host}/truck/create`;
    const headers = {'content-type': 'application/json'}  
    const body = JSON.stringify(truck);

    return this.httpClient
      .post<ResponseResult>(url, body, { headers: headers })
      .pipe(catchError((err) => this.handleError(err)));
  }

  updateTruck(truck: Truck): Observable<ResponseResult> {  
    const url = `${this.host}/truck/update/${truck.id}`;
    const headers = {'content-type': 'application/json'}  
    const body = JSON.stringify(truck);

    return this.httpClient
      .put<ResponseResult>(url, body, { headers: headers })
      .pipe(catchError((err) => this.handleError(err)));
  }

  deleteTruck(truckId: string): Observable<ResponseResult> {  
    const url = `${this.host}/truck/delete/${truckId}`; 

    return this.httpClient
      .delete<ResponseResult>(url)
      .pipe(catchError((err) => this.handleError(err)));
  }

  //#endregion


  //#region Employee API
  
  getEmployee(id: string): Observable<ResponseResult<Employee>> {  
    const url = `${this.host}/employee/${id}`;
    return this.httpClient
      .get<ResponseResult<Employee>>(url)
      .pipe(catchError((err) => this.handleError(err)));
  }

  getEmployees(searchQuery = '', orderBy = '', page = 1, pageSize = 10): Observable<PagedResponseResult<Employee>> {
    if (!searchQuery) {
      searchQuery = ''
    }

    if (!orderBy) {
      orderBy = ''
    }

    const url = `${this.host}/employee/list?search=${searchQuery}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
    return this.httpClient
      .get<PagedResponseResult<Employee>>(url)
      .pipe(catchError((err) => this.handleError(err)));
  }

  getDrivers(searchQuery = '', page = 1, pageSize = 10): Observable<PagedResponseResult<Employee>> {  
    if (!searchQuery) {
      searchQuery = ''
    }

    const url = `${this.host}/employee/drivers?search=${searchQuery}&page=${page}&pageSize=${pageSize}`;
    return this.httpClient
      .get<PagedResponseResult<Employee>>(url)
      .pipe(catchError((err) => this.handleError(err)));
  }

  createEmployee(employee: CreateEmployee): Observable<ResponseResult> {  
    const url = `${this.host}/employee/create`;
    const headers = {'content-type': 'application/json'}  
    const body = JSON.stringify(employee);

    return this.httpClient
      .post<ResponseResult>(url, body, { headers: headers })
      .pipe(catchError((err) => this.handleError(err)));
  }

  removeRoleFromEmployee(employee: RemoveEmployeeRole): Observable<ResponseResult> {  
    const url = `${this.host}/employee/removeRole`;
    const headers = {'content-type': 'application/json'}  
    const body = JSON.stringify(employee);

    return this.httpClient
      .post<ResponseResult>(url, body, { headers: headers })
      .pipe(catchError((err) => this.handleError(err)));
  }

  updateEmployee(employee: UpdateEmployee): Observable<ResponseResult> {  
    const url = `${this.host}/employee/update/${employee.id}`;
    const headers = {'content-type': 'application/json'}  
    const body = JSON.stringify(employee);

    return this.httpClient
      .put<ResponseResult>(url, body, { headers: headers })
      .pipe(catchError((err) => this.handleError(err)));
  }

  deleteEmployee(employeeId: string): Observable<ResponseResult> {  
    const url = `${this.host}/employee/delete/${employeeId}`; 

    return this.httpClient
      .delete<ResponseResult>(url)
      .pipe(catchError((err) => this.handleError(err)));
  }

  //#endregion


  //#region Tenant Role API

  getRoles(searchQuery = '', page = 1, pageSize = 10): Observable<PagedResponseResult<Role>> {
    if (!searchQuery) {
      searchQuery = ''
    }
    
    const url = `${this.host}/tenantRole/list?search=${searchQuery}&page=${page}&pageSize=${pageSize}`;
    return this.httpClient
      .get<PagedResponseResult<Role>>(url)
      .pipe(catchError((err) => this.handleError(err)));
  }

  //#endregion

  //#region Report API

  getOverallStats(): Observable<ResponseResult<OverallStats>> {
    const url = `${this.host}/report/getOverallStats`; 

    return this.httpClient
      .get<ResponseResult<OverallStats>>(url)
      .pipe(catchError((err) => this.handleError(err)));
  }

  getDailyGrosses(startDate: Date, endDate?: Date, truckId?: string): Observable<ResponseResult<DailyGrosses>> {
    let url = `${this.host}/report/getDailyGrosses?startDate=${startDate.toJSON()}`;

    if (endDate) {
      url += `&endDate=${endDate.toJSON()}`;
    }
    if (truckId) {
      url += `&truckId=${truckId}`;
    }

    return this.httpClient
      .get<ResponseResult<DailyGrosses>>(url)
      .pipe(catchError((err) => this.handleError(err)));
  }

  getMonthlyGrosses(startDate: Date, endDate?: Date, truckId?: string): Observable<ResponseResult<MonthlyGrosses>> {
    let url = `${this.host}/report/getMonthlyGrosses?startDate=${startDate.toJSON()}`;

    if (endDate) {
      url += `&endDate=${endDate.toJSON()}`;
    }
    if (truckId) {
      url += `&truckId=${truckId}`;
    }

    return this.httpClient
      .get<ResponseResult<MonthlyGrosses>>(url)
      .pipe(catchError((err) => this.handleError(err)));
  }

  //#endregion
  
  parseSortProperty(sortField?: string, sortOrder?: number) {
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
    const errorMessage = responseData.error?.error ?? responseData.error;
    
    this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: errorMessage});
    this.messageService.add({key: 'errorMsg', severity: 'error', summary: 'Error', detail: errorMessage});
    console.error(errorMessage ?? responseData);
    return of({error: errorMessage, success: false});
  }
}
