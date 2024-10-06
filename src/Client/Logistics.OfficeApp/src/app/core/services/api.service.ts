import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {MessageService} from "primeng/api";
import {catchError, Observable, of} from "rxjs";
import {TenantService} from "@/core/services";
import {GLOBAL_CONFIG} from "@/configs";
import {
  Result,
  EmployeeDto,
  PagedResponseResult,
  TenantDto,
  TruckDto,
  UserDto,
  LoadDto,
  RoleDto,
  DailyGrossesDto,
  MonthlyGrossesDto,
  CompanyStatsDto,
  UpdateEmployeeCommand,
  CreateEmployeeCommand,
  RemoveEmployeeRoleCommand,
  TruckDriverDto,
  UpdateTruckCommand,
  CreateTruckCommand,
  UpdateLoadCommand,
  CreateLoadCommand,
  TruckStatsDto,
  NotificationDto,
  UpdateNotificationCommand,
  SearchableQuery,
  CustomerDto,
  UpdateCustomerCommand,
  CreateCustomerCommand,
  PaymentDto,
  CreatePaymentCommand,
  UpdatePaymentCommand,
  PagedIntervalQuery,
  InvoiceDto,
  CreateInvoiceCommand,
  UpdateInvoiceCommand,
  PayrollDto,
  UpdatePayrollCommand,
  CreatePayrollCommand,
  ProcessPaymentCommand,
  GetPayrollsQuery,
} from "../models";

@Injectable({providedIn: "root"})
export class ApiService {
  private host: string;
  private headers: Record<string, string>;

  constructor(
    private readonly httpClient: HttpClient,
    private readonly tenantService: TenantService,
    private readonly messageService: MessageService
  ) {
    this.host = GLOBAL_CONFIG.apiHost;
    this.headers = {"content-type": "application/json"};
  }

  // #region Tenant API

  getTenant(): Observable<Result<TenantDto>> {
    const tenantId = this.tenantService.getTenantName();
    const url = `/tenants/${tenantId}`;
    return this.get(url);
  }

  // #endregion

  // #region User API

  getUsers(query?: SearchableQuery): Observable<PagedResponseResult<UserDto>> {
    const url = `/users?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  // #endregion

  // #region Load API

  getLoad(id: string): Observable<Result<LoadDto>> {
    const url = `/loads/${id}`;
    return this.get(url);
  }

  getLoads(query?: SearchableQuery, onlyActiveLoads = false): Observable<PagedResponseResult<LoadDto>> {
    let url = `/loads?${this.stringfySearchableQuery(query)}`;

    if (onlyActiveLoads) {
      url += "&onlyActiveLoads=true";
    }
    return this.get(url);
  }

  createLoad(command: CreateLoadCommand): Observable<Result> {
    const url = `/loads`;
    return this.post(url, command);
  }

  updateLoad(command: UpdateLoadCommand): Observable<Result> {
    const url = `/loads/${command.id}`;
    return this.put(url, command);
  }

  deleteLoad(loadId: string): Observable<Result> {
    const url = `/loads/${loadId}`;
    return this.delete(url);
  }

  // #endregion

  // #region Truck API

  getTruck(truckId: string): Observable<Result<TruckDto>> {
    const url = `/trucks/${truckId}`;
    return this.get(url);
  }

  getTrucks(query?: SearchableQuery): Observable<PagedResponseResult<TruckDto>> {
    const url = `/trucks?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  getTruckDrivers(query?: SearchableQuery): Observable<PagedResponseResult<TruckDriverDto>> {
    const url = `/trucks/drivers?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  createTruck(command: CreateTruckCommand): Observable<Result> {
    const url = `/trucks`;
    return this.post(url, command);
  }

  updateTruck(command: UpdateTruckCommand): Observable<Result> {
    const url = `/trucks/${command.id}`;
    return this.put(url, command);
  }

  deleteTruck(truckId: string): Observable<Result> {
    const url = `/trucks/${truckId}`;
    return this.delete(url);
  }

  // #endregion

  // #region Employee API

  getEmployee(userId: string): Observable<Result<EmployeeDto>> {
    const url = `/employees/${userId}`;
    return this.get(url);
  }

  getEmployees(query?: SearchableQuery): Observable<PagedResponseResult<EmployeeDto>> {
    const url = `/employees?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  getDrivers(query?: SearchableQuery): Observable<PagedResponseResult<EmployeeDto>> {
    const url = `/employees?${this.stringfySearchableQuery(query)}&role=tenant.driver`;
    return this.get(url);
  }

  createEmployee(command: CreateEmployeeCommand): Observable<Result> {
    const url = `/employees`;
    return this.post(url, command);
  }

  removeRoleFromEmployee(command: RemoveEmployeeRoleCommand): Observable<Result> {
    const url = `/employees/${command.userId}/remove-role`;
    return this.post(url, command);
  }

  updateEmployee(command: UpdateEmployeeCommand): Observable<Result> {
    const url = `/employees/${command.userId}`;
    return this.put(url, command);
  }

  deleteEmployee(employeeId: string): Observable<Result> {
    const url = `/employees/${employeeId}`;
    return this.delete(url);
  }

  // #endregion

  // #region Tenant Role API

  getRoles(query?: SearchableQuery): Observable<PagedResponseResult<RoleDto>> {
    const url = `/tenant-roles?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  // #endregion

  // #region Stats API

  getCompanyStats(): Observable<Result<CompanyStatsDto>> {
    const url = `/stats/company`;
    return this.get(url);
  }

  getDailyGrosses(startDate: Date, endDate?: Date, truckId?: string): Observable<Result<DailyGrossesDto>> {
    let url = `/stats/daily-grosses?startDate=${startDate.toJSON()}`;

    if (endDate) {
      url += `&endDate=${endDate.toJSON()}`;
    }
    if (truckId) {
      url += `&truckId=${truckId}`;
    }

    return this.get(url);
  }

  getMonthlyGrosses(startDate: Date, endDate?: Date, truckId?: string): Observable<Result<MonthlyGrossesDto>> {
    let url = `/stats/monthly-grosses?startDate=${startDate.toJSON()}`;

    if (endDate) {
      url += `&endDate=${endDate.toJSON()}`;
    }
    if (truckId) {
      url += `&truckId=${truckId}`;
    }

    return this.get<Result<MonthlyGrossesDto>>(url);
  }

  getTrucksStats(query: PagedIntervalQuery): Observable<PagedResponseResult<TruckStatsDto>> {
    const url = `/stats/trucks?${this.stringfyPagedIntervalQuery(query)}`;
    return this.get(url);
  }

  // #endregion

  // #region Notifications API

  getNotifications(startDate: Date, endDate: Date): Observable<Result<NotificationDto[]>> {
    const url = `/notifications?startDate=${startDate.toJSON()}&endDate=${endDate.toJSON()}`;
    return this.get(url);
  }

  updateNotification(commad: UpdateNotificationCommand): Observable<Result> {
    const url = `/notifications/${commad.id}`;
    return this.put(url, commad);
  }

  // #endregion

  // #region Customers API

  getCustomer(id: string): Observable<Result<CustomerDto>> {
    const url = `/customers/${id}`;
    return this.get(url);
  }

  getCustomers(query?: SearchableQuery): Observable<PagedResponseResult<CustomerDto>> {
    const url = `/customers?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  createCustomer(command: CreateCustomerCommand): Observable<Result> {
    const url = `/customers`;
    return this.post(url, command);
  }

  updateCustomer(command: UpdateCustomerCommand): Observable<Result> {
    const url = `/customers/${command.id}`;
    return this.put(url, command);
  }

  deleteCustomer(customerId: string): Observable<Result> {
    const url = `/customers/${customerId}`;
    return this.delete(url);
  }

  // #endregion

  // #region Payments API

  getPayment(id: string): Observable<Result<PaymentDto>> {
    const url = `/payments/${id}`;
    return this.get(url);
  }

  getPayments(query?: PagedIntervalQuery): Observable<PagedResponseResult<PaymentDto>> {
    const url = `/payments?${this.stringfyPagedIntervalQuery(query)}`;
    return this.get(url);
  }

  processPayment(command: ProcessPaymentCommand): Observable<Result> {
    const url = `/payments/process-payment`;
    return this.post(url, command);
  }

  createPayment(command: CreatePaymentCommand): Observable<Result> {
    const url = `/payments`;
    return this.post(url, command);
  }

  updatePayment(command: UpdatePaymentCommand): Observable<Result> {
    const url = `/payments/${command.id}`;
    return this.put(url, command);
  }

  deletePayment(paymentId: string): Observable<Result> {
    const url = `/payments/${paymentId}`;
    return this.delete(url);
  }

  // #endregion

  // #region Invoices API

  getInvoice(id: string): Observable<Result<InvoiceDto>> {
    const url = `/invoices/${id}`;
    return this.get(url);
  }

  getInvoices(query?: PagedIntervalQuery): Observable<PagedResponseResult<InvoiceDto>> {
    const url = `/invoices?${this.stringfyPagedIntervalQuery(query)}`;
    return this.get(url);
  }

  createInvoice(command: CreateInvoiceCommand): Observable<Result> {
    const url = `/invoices`;
    return this.post(url, command);
  }

  updateInvoice(command: UpdateInvoiceCommand): Observable<Result> {
    const url = `/invoices/${command.id}`;
    return this.put(url, command);
  }

  deleteInvoice(invoiceId: string): Observable<Result> {
    const url = `/invoices/${invoiceId}`;
    return this.delete(url);
  }

  // #endregion

  // #region Payrolls API

  getPayroll(id: string): Observable<Result<PayrollDto>> {
    const url = `/payrolls/${id}`;
    return this.get(url);
  }

  getPayrolls(query?: GetPayrollsQuery): Observable<PagedResponseResult<PayrollDto>> {
    let url = `/payrolls?${this.stringfySearchableQuery(query)}`;

    if (query?.employeeId) {
      url += `&employeeId=${query.employeeId}`;
    }

    return this.get(url);
  }

  calculateEmployeePayroll(query: CreatePayrollCommand): Observable<Result<PayrollDto>> {
    const url = `/payrolls/calculate?employeeId=${query.employeeId}&startDate=${query.startDate.toJSON()}&endDate=${query.endDate.toJSON()}`;
    return this.get(url);
  }

  createPayroll(command: CreatePayrollCommand): Observable<Result> {
    const url = `/payrolls`;
    return this.post(url, command);
  }

  updatePayroll(command: UpdatePayrollCommand): Observable<Result> {
    const url = `/payrolls/${command.id}`;
    return this.put(url, command);
  }

  deletePayroll(payrollId: string): Observable<Result> {
    const url = `/payrolls/${payrollId}`;
    return this.delete(url);
  }

  // #endregion

  parseSortProperty(sortField?: string | null, sortOrder?: number | null): string {
    if (!sortOrder) {
      sortOrder = 1;
    }

    if (!sortField) {
      sortField = "";
    }

    return sortOrder <= -1 ? `-${sortField}` : sortField;
  }

  private get<TResponse>(endpoint: string): Observable<TResponse> {
    return this.httpClient.get<TResponse>(this.host + endpoint).pipe(catchError((err) => this.handleError(err)));
  }

  private post<TResponse, TBody>(endpoint: string, body: TBody): Observable<TResponse> {
    const bodyJson = JSON.stringify(body);

    return this.httpClient
      .post<TResponse>(this.host + endpoint, bodyJson, {headers: this.headers})
      .pipe(catchError((err) => this.handleError(err)));
  }

  private put<TResponse, TBody>(endpoint: string, body: TBody): Observable<TResponse> {
    const bodyJson = JSON.stringify(body);

    return this.httpClient
      .put<TResponse>(this.host + endpoint, bodyJson, {headers: this.headers})
      .pipe(catchError((err) => this.handleError(err)));
  }

  private delete<TResponse>(endpoint: string): Observable<TResponse> {
    return this.httpClient.delete<TResponse>(this.host + endpoint).pipe(catchError((err) => this.handleError(err)));
  }

  private stringfySearchableQuery(query?: SearchableQuery): string {
    const search = query?.search ?? "";
    const orderBy = query?.orderBy ?? "";
    const page = query?.page ?? 1;
    const pageSize = query?.pageSize ?? 10;
    return `search=${search}&orderBy=${orderBy}&page=${page}&pageSize=${pageSize}`;
  }

  private stringfyPagedIntervalQuery(query?: PagedIntervalQuery): string {
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
  private handleError(responseData: any): Observable<any> {
    const errorMessage = responseData.error?.error ?? responseData.error;

    this.messageService.add({key: "notification", severity: "error", summary: "Error", detail: errorMessage});
    // this.messageService.add({key: 'errorMsg', severity: 'error', summary: 'Error', detail: errorMessage});
    console.error(errorMessage ?? responseData);
    return of({error: errorMessage, success: false});
  }
}
