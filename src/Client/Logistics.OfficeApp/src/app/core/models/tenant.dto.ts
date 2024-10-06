import {AddressDto} from "./address.dto";

export interface TenantDto {
  id: string;
  name: string;
  companyName: string;
  companyAddress: AddressDto;
}
