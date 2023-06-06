export interface UserIdentity {
  sub: string;
  name: string;
  role?: string[];
  permission?: string | string[];
}
