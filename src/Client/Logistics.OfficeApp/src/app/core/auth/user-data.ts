import { UserIdentity } from "./user-identity";

export class UserData {
  public readonly id: string;
  public readonly name: string;
  public readonly firstName: string;
  public readonly lastName: string;
  public readonly permissions: string[];
  public readonly roles: string[];
  public readonly tenant: string;

  constructor(userIdentity: UserIdentity) {
    this.id = userIdentity.sub;
    this.name = userIdentity.name;
    this.firstName = userIdentity.given_name;
    this.lastName = userIdentity.family_name;
    this.tenant = userIdentity.tenant;
    this.permissions = [];
    this.roles = [];

    this.tryToAddArray(this.permissions, userIdentity.permission);
    this.tryToAddArray(this.roles, userIdentity.role);
  }

  public getFullName(): string {
    return `${this.firstName} ${this.lastName}`;
  }

  private tryToAddArray(arr: string[], data?: string | string[]) {
    if (!data) {
      return;
    }

    if (typeof data === "string") {
      arr.push(data);
    } else if (Array.isArray(data)) {
      arr.push(...data);
    }
  }
}
