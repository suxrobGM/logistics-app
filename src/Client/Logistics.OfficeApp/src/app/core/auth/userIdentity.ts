export interface UserIdentity {
  sub: string;
  name: string;
  given_name: string;
  family_name: string;
  role?: string | string[];
  permission?: string | string[];
}
