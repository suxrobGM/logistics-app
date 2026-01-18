import { Component, inject } from "@angular/core";
import { RouterLink } from "@angular/router";
import { DataContainer, PageHeader, SearchInput } from "@logistics/shared/components";
import { CardModule } from "primeng/card";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { UsersListStore } from "../store/users-list.store";

@Component({
  selector: "adm-users-list",
  templateUrl: "./users-list.html",
  providers: [UsersListStore],
  imports: [
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    DataContainer,
    PageHeader,
    SearchInput,
    TagModule,
  ],
})
export class UsersList {
  protected readonly store = inject(UsersListStore);

  protected search(value: string): void {
    this.store.setSearch(value);
  }

  protected getRoleSeverity(role?: string | null): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" {
    switch (role?.toLowerCase()) {
      case "superadmin":
        return "danger";
      case "owner":
        return "warn";
      case "manager":
        return "info";
      case "dispatcher":
        return "secondary";
      default:
        return "secondary";
    }
  }

  protected getFullName(user: { firstName?: string | null; lastName?: string | null }): string {
    const first = user.firstName ?? "";
    const last = user.lastName ?? "";
    return `${first} ${last}`.trim() || "N/A";
  }
}
