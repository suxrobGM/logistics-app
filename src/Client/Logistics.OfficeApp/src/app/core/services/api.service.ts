import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {MessageService} from 'primeng/api';
import {catchError, Observable, of} from 'rxjs';
import {TenantService} from '@core/services';
import {AppConfig} from '@configs';
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
  CompanyStats,
  UpdateEmployee,
  CreateEmployee,
  RemoveEmployeeRole,
  TruckDriver,
  UpdateTruck,
  CreateTruck,
  UpdateLoad,
  CreateLoad,
  TruckStats,
  Notification,
  UpdateNotification,
} from '../models';


@Injectable()
export class ApiService {
  private host: string;
  private headers: Record<string, string>;

  constructor(
    private httpClient: HttpClient,
    private tenantService: TenantService,
    private messageService: MessageService)
  {
    this.host = AppConfig.apiHost;
    this.headers = {'content-type': 'application/json'};
  }

  // #region Tenant API

  getTenant(): Observable<ResponseResult<Tenant>> {
    const tenantId = this.tenantService.getTenantName();
    const url = `${this.host}/tenants/${tenantId}`;

    return this.httpClient
        .get<ResponseResult<Tenant>>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  // #endregion


  // #region User API

  getUsers(searchQuery = '', orderBy = '', page = 1, pageSize = 10): Observable<PagedResponseResult<User>> {
    const url = `${this.host}/users?search=${searchQuery}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
    return this.httpClient
        .get<PagedResponseResult<User>>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  // #endregion


  // #region Load API

  getLoad(id: string): Observable<ResponseResult<Load>> {
    const url = `${this.host}/loads/${id}`;
    return this.httpClient
        .get<ResponseResult<Load>>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  getLoads(searchQuery = '', onlyActiveLoads = false, orderBy = '', page = 1, pageSize = 10): Observable<PagedResponseResult<Load>> {
    let url = `${this.host}/loads?search=${searchQuery}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;

    if (onlyActiveLoads) {
      url = `${url}&onlyActiveLoads=true`;
    }
    return this.httpClient
        .get<PagedResponseResult<Load>>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  createLoad(command: CreateLoad): Observable<ResponseResult> {
    const url = `${this.host}/loads`;
    const body = JSON.stringify(command);

    return this.httpClient
        .post<ResponseResult>(url, body, {headers: this.headers})
        .pipe(catchError((err) => this.handleError(err)));
  }

  updateLoad(command: UpdateLoad): Observable<ResponseResult> {
    const url = `${this.host}/loads/${command.id}`;
    const body = JSON.stringify(command);

    return this.httpClient
        .put<ResponseResult>(url, body, {headers: this.headers})
        .pipe(catchError((err) => this.handleError(err)));
  }

  deleteLoad(loadId: string): Observable<ResponseResult> {
    const url = `${this.host}/loads/${loadId}`;

    return this.httpClient
        .delete<ResponseResult>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  // #endregion


  // #region Truck API

  getTruck(truckId: string): Observable<ResponseResult<Truck>> {
    const url = `${this.host}/trucks/${truckId}`;
    return this.httpClient
        .get<ResponseResult<Truck>>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  getTrucks(searchQuery = '', orderBy = '', page = 1, pageSize = 10): Observable<PagedResponseResult<Truck>> {
    const url = `${this.host}/trucks?search=${searchQuery}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
    return this.httpClient
        .get<PagedResponseResult<Truck>>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  getTruckDrivers(searchQuery = '', page = 1, pageSize = 10): Observable<PagedResponseResult<TruckDriver>> {
    if (!searchQuery) {
      searchQuery = '';
    }

    const url = `${this.host}/trucks/drivers?search=${searchQuery}&page=${page}&pageSize=${pageSize}`;
    return this.httpClient
        .get<PagedResponseResult<TruckDriver>>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  createTruck(command: CreateTruck): Observable<ResponseResult> {
    const url = `${this.host}/trucks`;
    const body = JSON.stringify(command);

    return this.httpClient
        .post<ResponseResult>(url, body, {headers: this.headers})
        .pipe(catchError((err) => this.handleError(err)));
  }

  updateTruck(command: UpdateTruck): Observable<ResponseResult> {
    const url = `${this.host}/trucks/${command.id}`;
    const body = JSON.stringify(command);

    return this.httpClient
        .put<ResponseResult>(url, body, {headers: this.headers})
        .pipe(catchError((err) => this.handleError(err)));
  }

  deleteTruck(truckId: string): Observable<ResponseResult> {
    const url = `${this.host}/trucks/${truckId}`;

    return this.httpClient
        .delete<ResponseResult>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  // #endregion


  // #region Employee API

  getEmployee(userId: string): Observable<ResponseResult<Employee>> {
    const url = `${this.host}/employees/${userId}`;
    return this.httpClient
        .get<ResponseResult<Employee>>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  getEmployees(searchQuery = '', orderBy = '', page = 1, pageSize = 10): Observable<PagedResponseResult<Employee>> {
    const url = `${this.host}/employees?search=${searchQuery}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
    return this.httpClient
        .get<PagedResponseResult<Employee>>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  getDrivers(searchQuery = '', orderBy = '', page = 1, pageSize = 10): Observable<PagedResponseResult<Employee>> {
    const url = `${this.host}/employees?search=${searchQuery}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}&role=tenant.driver`;
    return this.httpClient
        .get<PagedResponseResult<Employee>>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  createEmployee(employee: CreateEmployee): Observable<ResponseResult> {
    const url = `${this.host}/employees`;
    const body = JSON.stringify(employee);

    return this.httpClient
        .post<ResponseResult>(url, body, {headers: this.headers})
        .pipe(catchError((err) => this.handleError(err)));
  }

  removeRoleFromEmployee(employee: RemoveEmployeeRole): Observable<ResponseResult> {
    const url = `${this.host}/employees/${employee.userId}/remove-role`;
    const body = JSON.stringify(employee);

    return this.httpClient
        .post<ResponseResult>(url, body, {headers: this.headers})
        .pipe(catchError((err) => this.handleError(err)));
  }

  updateEmployee(employee: UpdateEmployee): Observable<ResponseResult> {
    const url = `${this.host}/employees/${employee.userId}`;
    const body = JSON.stringify(employee);

    return this.httpClient
        .put<ResponseResult>(url, body, {headers: this.headers})
        .pipe(catchError((err) => this.handleError(err)));
  }

  deleteEmployee(employeeId: string): Observable<ResponseResult> {
    const url = `${this.host}/employees/${employeeId}`;

    return this.httpClient
        .delete<ResponseResult>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  // #endregion


  // #region Tenant Role API

  getRoles(searchQuery = '', page = 1, pageSize = 10): Observable<PagedResponseResult<Role>> {
    if (!searchQuery) {
      searchQuery = '';
    }

    const url = `${this.host}/tenant-roles?search=${searchQuery}&page=${page}&pageSize=${pageSize}`;
    return this.httpClient
        .get<PagedResponseResult<Role>>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  // #endregion


  // #region Stats API

  getCompanyStats(): Observable<ResponseResult<CompanyStats>> {
    const url = `${this.host}/stats/company`;

    return this.httpClient
        .get<ResponseResult<CompanyStats>>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  getDailyGrosses(startDate: Date, endDate?: Date, truckId?: string): Observable<ResponseResult<DailyGrosses>> {
    let url = `${this.host}/stats/daily-grosses?startDate=${startDate.toJSON()}`;

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
    let url = `${this.host}/stats/monthly-grosses?startDate=${startDate.toJSON()}`;

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

  getTrucksStats(startDate: Date, endDate: Date, orderBy = '', page = 1, pageSize = 10): Observable<PagedResponseResult<TruckStats>> {
    const url = `${this.host}/stats/trucks?startDate=${startDate.toJSON()}&endDate=${endDate.toJSON()}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;

    return this.httpClient
        .get<ResponseResult<MonthlyGrosses>>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  // #endregion


  // #region Notifications API

  getNotifications(startDate: Date, endDate: Date): Observable<ResponseResult<Notification[]>> {
    const url = `${this.host}/notifications?startDate=${startDate.toJSON()}&endDate=${endDate.toJSON()}`;

    return this.httpClient
        .get<ResponseResult<Notification>>(url)
        .pipe(catchError((err) => this.handleError(err)));
  }

  updateNotification(commad: UpdateNotification): Observable<ResponseResult> {
    const url = `${this.host}/notifications/${commad.id}`;
    const body = JSON.stringify(commad);

    return this.httpClient
        .put<ResponseResult>(url, body, {headers: this.headers})
        .pipe(catchError((err) => this.handleError(err)));
  }

  // #endregion


  parseSortProperty(sortField?: string | null, sortOrder?: number | null) {
    if (!sortOrder) {
      sortOrder = 1;
    }

    if (!sortField) {
      sortField = '';
    }

    return sortOrder <= -1 ? `-${sortField}` : sortField;
  }

  private handleError(responseData: any): Observable<any> {
    const errorMessage = responseData.error?.error ?? responseData.error;

    this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: errorMessage});
    // this.messageService.add({key: 'errorMsg', severity: 'error', summary: 'Error', detail: errorMessage});
    console.error(errorMessage ?? responseData);
    return of({error: errorMessage, success: false});
  }
}
