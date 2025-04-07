import {CustomerDto} from "../customer/customer.dto";
import {PaymentDto} from "../payment/payment.dto";

export interface InvoiceDto {
  id: string;
  loadId: string;
  loadRef: number;
  createdDate: string;
  customer: CustomerDto;
  payment: PaymentDto;
}
