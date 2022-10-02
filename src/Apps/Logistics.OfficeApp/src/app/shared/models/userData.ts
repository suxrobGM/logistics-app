import { UserIdentity } from "./userIdentity";

export class UserData {
  private readonly _id: string;
  private readonly _name: string;
  private readonly _permissions: string[];
  private readonly _roles: string[];

  constructor(userIdentity: UserIdentity) {
    this._id = userIdentity.sub;
    this._name = userIdentity.name;
    this._permissions = [];
    this._roles = [];

    this.tryToAddArray(this.permissions, userIdentity.permission);
    this.tryToAddArray(this.roles, userIdentity.role);
  }

  public get id(): string {
    return this._id;
  }

  public get name(): string {
    return this._name;
  }

  public get permissions(): string[] {
    return this._permissions;
  }

  public get roles(): string[] {
    return this._roles;
  }

  private tryToAddArray(arr: string[], data?: string | string[],) {
    if (!data) {
      return;
    }

    if (typeof data === 'string') {
      arr.push(data);
    }
    else if (Array.isArray(data)) {
      arr.push(...data);
    }
  }
}