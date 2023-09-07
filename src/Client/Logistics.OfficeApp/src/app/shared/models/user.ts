export interface User {
  id: string;
  firstName: string;
  lastName: string
  role?: string | string[];
  email: string;
  phoneNumber?: string;
}
