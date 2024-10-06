export interface UserDto {
  id: string;
  firstName: string;
  lastName: string;
  role?: string | string[];
  email: string;
  phoneNumber?: string;
}
