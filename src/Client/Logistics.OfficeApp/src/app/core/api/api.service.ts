import { Injectable } from "@angular/core";
import { ApiBase } from "./api-base";
import {
  CustomerApiService,
  DocumentApiService,
  EmployeeApiService,
  InvoiceApiService,
  LoadApiService,
  NotificationApiService,
  PaymentApiService,
  ReportApiService,
  RoleApiService,
  StatsApiService,
  SubscriptionApiService,
  TenantApiService,
  TripApiService,
  TruckApiService,
  UserApiService,
} from "./services";

/**
 * Facade service that provides access to various API services.
 * This service aggregates multiple API services for easier access throughout the application.
 */
@Injectable()
export class ApiService extends ApiBase {
  public readonly paymentApi = new PaymentApiService();
  public readonly tenantApi = new TenantApiService();
  public readonly userApi = new UserApiService();
  public readonly subscriptionApi = new SubscriptionApiService();
  public readonly invoiceApi = new InvoiceApiService();
  public readonly tripApi = new TripApiService();
  public readonly loadApi = new LoadApiService();
  public readonly reportApi = new ReportApiService();
  public readonly truckApi = new TruckApiService();
  public readonly customerApi = new CustomerApiService();
  public readonly documentApi = new DocumentApiService();
  public readonly employeeApi = new EmployeeApiService();
  public readonly roleApi = new RoleApiService();
  public readonly statsApi = new StatsApiService();
  public readonly notificationApi = new NotificationApiService();
}
