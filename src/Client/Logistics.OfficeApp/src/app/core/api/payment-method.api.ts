import {HttpClient} from "@angular/common/http";
import {Observable} from "rxjs";
import {ApiBase} from "./api-base";
import {
  CreatePaymentMethodCommand,
  DeletePaymentMethodCommand,
  PaymentMethodDto,
  Result,
  SetDefaultPaymentMethodCommand,
  UpdatePaymentMethodCommand,
} from "./models";

export class PaymentMethodApi extends ApiBase {
  constructor(apiUrl: string, http: HttpClient) {
    super(apiUrl, http);
  }

  getPaymentMethod(id: string, tenantId: string): Observable<Result<PaymentMethodDto>> {
    return this.get<Result<PaymentMethodDto>>(`/tenants/${tenantId}/payment-methods/${id}`);
  }

  getPaymentMethods(tenantId: string, orderBy?: string): Observable<Result<PaymentMethodDto[]>> {
    let query = "";

    if (orderBy) {
      query = `?orderBy=${orderBy}`;
    }

    return this.get<Result<PaymentMethodDto[]>>(`/tenants/${tenantId}/payment-methods${query}`);
  }

  createPaymentMethod(command: CreatePaymentMethodCommand): Observable<Result> {
    return this.post<Result, CreatePaymentMethodCommand>(
      `/tenants/${command.tenantId}/payment-methods`,
      command
    );
  }

  updatePaymentMethod(command: UpdatePaymentMethodCommand): Observable<Result> {
    return this.put<Result, UpdatePaymentMethodCommand>(
      `/tenants/${command.tenantId}/payment-methods/${command.id}`,
      command
    );
  }

  setDefaultPaymentMethod(command: SetDefaultPaymentMethodCommand): Observable<Result> {
    return this.put<Result, SetDefaultPaymentMethodCommand>(
      `/tenants/${command.tenantId}/payment-methods/default`,
      command
    );
  }

  deletePaymentMethod(command: DeletePaymentMethodCommand): Observable<Result> {
    return this.delete<Result>(
      `/tenants/${command.tenantId}/payment-methods/${command.paymentMethodId}`
    );
  }
}
