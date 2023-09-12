import {Injectable} from '@angular/core';
import {UserData, UserIdentity} from 'src/app/core/models';

@Injectable({
  providedIn: 'root',
})
export class UserDataService {
  private userData: UserData | null;

  constructor() {
    this.userData = null;
  }

  public setUser(userIdentity: UserIdentity | null) {
    if (userIdentity) {
      this.userData = new UserData(userIdentity);
    }
  }

  public getUser(): UserData | null {
    return this.userData;
  }
}
