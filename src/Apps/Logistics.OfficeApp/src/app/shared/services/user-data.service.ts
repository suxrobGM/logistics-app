import { Injectable } from '@angular/core';
import { UserData, UserIdentity } from '@shared/models';

@Injectable({
  providedIn: 'root'
})
export class UserDataService {
  private _userData: UserData | null;

  constructor() {
    this._userData = null;
  }

  public setUser(userIdentity: UserIdentity | null) {
    if (userIdentity) {
      this._userData = new UserData(userIdentity);
    }
  }

  public getUser(): UserData | null {
    return this._userData;
  }
}
