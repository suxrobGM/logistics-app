import { Observable } from "rxjs";
import { ApiBase } from "../api-base";
import {
  CreateCustomerCommand,
  CustomerDto,
  PagedResult,
  Result,
  SearchableQuery,
  UpdateCustomerCommand,
} from "../models";

export class CustomerApiService extends ApiBase {
  getCustomer(id: string): Observable<Result<CustomerDto>> {
    return this.get(`/customers/${id}`);
  }

  getCustomers(query?: SearchableQuery): Observable<PagedResult<CustomerDto>> {
    return this.get(`/customers?${this.stringfySearchableQuery(query)}`);
  }

  createCustomer(command: CreateCustomerCommand): Observable<Result<CustomerDto>> {
    return this.post("/customers", command);
  }

  updateCustomer(command: UpdateCustomerCommand): Observable<Result> {
    return this.put(`/customers/${command.id}`, command);
  }

  deleteCustomer(customerId: string): Observable<Result> {
    return this.delete(`/customers/${customerId}`);
  }
}
