/* eslint-disable indent */
import {Injectable} from '@angular/core';
import {map, Observable} from 'rxjs';
import {RoleDto, UserDto} from '@/core/models';
import {ApiService} from '@/core/services';
import {UserRole} from '@/core/enums';
import {AuthService} from '@/core/auth';


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

  searchUser(searchQuery: string): Observable<UserDto[] | undefined> {
    const users$ = this.apiService.getUsers({search: searchQuery});
    return users$.pipe(map((i) => i.data));
  }

  fetchRoles(): Observable<RoleDto[]> {
    const dummyRole: RoleDto = {name: '', displayName: ' '};
    const roles$ = this.apiService.getRoles();

    return roles$.pipe(
        map((result) => {
          if (result.success && result.data) {
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
