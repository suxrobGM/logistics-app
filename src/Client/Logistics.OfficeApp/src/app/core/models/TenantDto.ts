import {AddressDto} from "./AddressDto";

export interface TenantDto {
  id: string;
  name: string;
  companyName: string;
  companyAddress: AddressDto;
}
