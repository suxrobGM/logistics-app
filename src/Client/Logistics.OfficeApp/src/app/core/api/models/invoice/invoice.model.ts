import {CustomerDto} from "../customer/customer.model";
import {PaymentDto} from "../payment/payment.model";

export interface InvoiceDto {
  id: string;
  loadId: string;
  loadRef: number;
  createdDate: string;
  customer: CustomerDto;
  payment: PaymentDto;
}
