import { Injectable, inject } from "@angular/core";
import { UserRole } from "@logistics/shared/models";
import { Observable, from, map } from "rxjs";
import { Api, getTenantRoles, getUsers } from "@/core/api";
import type { RoleDto, RoleDtoPagedResponse, UserDto } from "@/core/api/models";
import { AuthService } from "@/core/auth";

@Injectable({ providedIn: "root" })
export class UserService {
  private readonly api = inject(Api);

  private userRoles?: string[];

  constructor() {
    const authService = inject(AuthService);

    const user = authService.getUserData();
    this.userRoles = user?.roles;
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

          if (this.userRoles?.includes(UserRole.Owner)) {
            const ownerIndex = roleNames.indexOf(UserRole.Owner);
            if (ownerIndex >= 0) roles.splice(ownerIndex, 1);
          } else if (this.userRoles?.includes(UserRole.Manager)) {
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
