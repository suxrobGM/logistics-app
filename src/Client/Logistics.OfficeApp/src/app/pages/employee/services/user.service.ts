/* eslint-disable indent */
import {Injectable} from '@angular/core';
import {map, Observable} from 'rxjs';
import {Role, User} from '@core/models';
import {ApiService} from '@core/services';
import {UserRole} from '@core/enums';
import {AuthService} from '@core/auth';


@Injectable()
export class UserService {
  private userRoles?: string[];

  constructor(
    authService: AuthService,
    private apiService: ApiService)
  {
    const user = authService.getUserData();
    this.userRoles = user?.roles;
  }

  public searchUser(searchQuery: string): Observable<User[] | undefined> {
    const users$ = this.apiService.getUsers(searchQuery);
    return users$.pipe(map((i) => i.data));
  }

  public fetchRoles(): Observable<Role[]> {
    const dummyRole: Role = {name: '', displayName: ' '};
    const roles$ = this.apiService.getRoles();

    return roles$.pipe(
        map((result) => {
          if (result.isSuccess && result.data) {
            const roles = result.data;
            const roleNames = roles.map((i) => i.name);

            if (this.userRoles?.includes(UserRole.Owner)) {
              roles.splice(roleNames.indexOf(UserRole.Owner), 1);
            }
            else if (this.userRoles?.includes(UserRole.Manager)) {
              roles.splice(roleNames.indexOf(UserRole.Owner), 1);
              roles.splice(roleNames.indexOf(UserRole.Manager), 1);
            }

            return [dummyRole, ...roles];
          }

          return [dummyRole];
        }),
    );
  }
}
