import { inject, Injectable } from "@angular/core";
import { UserRole } from "@logistics/shared";
import {
  Api,
  getTenantRoles,
  getUsers,
  type RoleDto,
  type RoleDtoPagedResponse,
  type UserDto,
} from "@logistics/shared/api";
import { from, map, Observable } from "rxjs";
import { AuthService } from "@/core/auth";

@Injectable({ providedIn: "root" })
export class UserService {
  private readonly api = inject(Api);

  private userRole?: string | null;

  constructor() {
    const authService = inject(AuthService);

    const user = authService.getUserData();
    this.userRole = user?.role;
  }

  searchUser(searchQuery: string): Observable<UserDto[] | undefined> {
    const users$ = from(this.api.invoke(getUsers, { Search: searchQuery }));
    return users$.pipe(map((i) => i.items ?? undefined));
  }

  fetchRoles(): Observable<RoleDto[]> {
    const dummyRole: RoleDto = { name: "", displayName: " " };
    const roles$ = from(this.api.invoke(getTenantRoles, {}));

    return roles$.pipe(
      map((result: RoleDtoPagedResponse) => {
        if (result.items) {
          const roles = [...result.items];
          const roleNames = roles.map((i: RoleDto) => i.name);

          if (this.userRole === UserRole.Owner) {
            const ownerIndex = roleNames.indexOf(UserRole.Owner);
            if (ownerIndex >= 0) roles.splice(ownerIndex, 1);
          } else if (this.userRole === UserRole.Manager) {
            const ownerIndex = roleNames.indexOf(UserRole.Owner);
            if (ownerIndex >= 0) roles.splice(ownerIndex, 1);
            const managerIndex = roles.map((i: RoleDto) => i.name).indexOf(UserRole.Manager);
            if (managerIndex >= 0) roles.splice(managerIndex, 1);
          }

          return [dummyRole, ...roles];
        }

        return [dummyRole];
      }),
    );
  }
}
