import {Injectable} from '@angular/core';
import {map, Observable} from 'rxjs';
import {Role, User, UserIdentity} from '@shared/models';
import {ApiService, UserDataService} from '@shared/services';
import {UserRole} from '@shared/types';

@Injectable()
export class UserService {
  private userRoles?: string[];

  constructor(
    private apiService: ApiService,
    userDataService: UserDataService)
  {
    const user = userDataService.getUser();
    this.userRoles = user?.roles;
  }

  public searchUser(searchQuery: string): Observable<User[] | undefined> {
    const users$ = this.apiService.getUsers(searchQuery);
    return users$.pipe(map((i) => i.items));
  }

  public fetchRoles(): Observable<Role[]> {
    const dummyRole: Role = {name: '', displayName: ' '};
    const roles$ = this.apiService.getRoles();

    return roles$.pipe(
        map((result) => {
          if (result.success && result.items) {
            const roles = result.items;
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
