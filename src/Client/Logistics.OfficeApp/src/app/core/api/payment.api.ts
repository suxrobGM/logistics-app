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

export class PaymentApi extends ApiBase {
  constructor(apiUrl: string, http: HttpClient) {
    super(apiUrl, http);
  }

  getPaymentMethod(id: string): Observable<Result<PaymentMethodDto>> {
    return this.get<Result<PaymentMethodDto>>(`/payments/methods/${id}`);
  }

  getPaymentMethods(orderBy?: string): Observable<Result<PaymentMethodDto[]>> {
    let query = "";

    if (orderBy) {
      query = `?orderBy=${orderBy}`;
    }

    return this.get<Result<PaymentMethodDto[]>>(`/payments/methods${query}`);
  }

  createPaymentMethod(command: CreatePaymentMethodCommand): Observable<Result> {
    return this.post<Result, CreatePaymentMethodCommand>(`/payments/methods`, command);
  }

  updatePaymentMethod(command: UpdatePaymentMethodCommand): Observable<Result> {
    return this.put<Result, UpdatePaymentMethodCommand>(`/payments/methods/${command.id}`, command);
  }

  setDefaultPaymentMethod(command: SetDefaultPaymentMethodCommand): Observable<Result> {
    return this.put<Result, SetDefaultPaymentMethodCommand>(`/payments/methods/default`, command);
  }

  deletePaymentMethod(command: DeletePaymentMethodCommand): Observable<Result> {
    return this.delete<Result>(`/payments/methods/${command.paymentMethodId}`);
  }
}
