import {Observable} from "rxjs";
import {ApiBase} from "./api-base";
import {
  CreatePaymentCommand,
  CreatePaymentMethodCommand,
  DeletePaymentMethodCommand,
  GetPaymentsQuery,
  PagedResult,
  PaymentDto,
  PaymentMethodDto,
  ProcessPaymentCommand,
  Result,
  SetDefaultPaymentMethodCommand,
  SetupIntentDto,
  UpdatePaymentCommand,
  UpdatePaymentMethodCommand,
} from "./models";

export class PaymentApi extends ApiBase {
  constructor(apiUrl: string) {
    super(apiUrl);
  }

  getPayment(id: string): Observable<Result<PaymentDto>> {
    return this.get(`/payments/${id}`);
  }

  getPayments(query?: GetPaymentsQuery): Observable<PagedResult<PaymentDto>> {
    const queryStr = this.stringfyPagedIntervalQuery(query, {
      subscriptionId: query?.subscriptionId,
    });

    return this.get(`/payments?${queryStr}`);
  }

  processPayment(command: ProcessPaymentCommand): Observable<Result> {
    return this.post(`/payments/process-payment`, command);
  }

  createPayment(command: CreatePaymentCommand): Observable<Result> {
    return this.post("/payments", command);
  }

  updatePayment(command: UpdatePaymentCommand): Observable<Result> {
    return this.put(`/payments/${command.id}`, command);
  }

  deletePayment(paymentId: string): Observable<Result> {
    return this.delete(`/payments/${paymentId}`);
  }

  //#region Payment Methods

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

  createSetupIntent(): Observable<Result<SetupIntentDto>> {
    return this.post<Result<SetupIntentDto>, object>("/payments/methods/setup-intent", {});
  }

  //#endregion
}
