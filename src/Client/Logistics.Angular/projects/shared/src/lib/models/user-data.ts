import type { UserIdentity } from "./user-identity";

export class UserData {
  public readonly id: string;
  public readonly name: string;
  public readonly firstName: string;
  public readonly lastName: string;
  public readonly role: string | null;
  public readonly tenant?: string;

  constructor(userIdentity: UserIdentity) {
    this.id = userIdentity.sub;
    this.name = userIdentity.name;
    this.firstName = userIdentity.given_name;
    this.lastName = userIdentity.family_name;
    this.tenant = userIdentity.tenant;
    this.role = userIdentity.role ?? null;
  }

  public getFullName(): string {
    return `${this.firstName} ${this.lastName}`;
  }
}
