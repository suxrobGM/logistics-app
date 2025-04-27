import {AddressDto} from "./address.model";
import {SubscriptionDto} from "./subscription";

export interface TenantDto {
  id: string;
  name: string;
  companyName: string;
  dotNumber?: string;
  companyAddress: AddressDto;
  subscription: SubscriptionDto | null;
  employeeCount?: number;
}
