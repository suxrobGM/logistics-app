import { Role } from './role';

export interface Employee {
  id?: string;
  userName?: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  phoneNumber?: string;
  roles?: Role[];
  joinedDate?: Date;
}
