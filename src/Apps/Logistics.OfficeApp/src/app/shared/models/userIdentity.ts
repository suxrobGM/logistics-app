export interface UserIdentity {
  sub: string;
  name: string;
  role?: string | string[];
  permission?: string | string[];
}