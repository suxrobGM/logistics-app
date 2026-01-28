import { Component, computed, input } from "@angular/core";
import { type EmployeeDto } from "@logistics/shared/api";
import { type AvatarColors, type AvatarSize, UserAvatar } from "@/shared/components";
import { Converters } from "@/shared/utils";

@Component({
  selector: "app-employee-avatar",
  templateUrl: "./employee-avatar.html",
  imports: [UserAvatar],
})
export class EmployeeAvatar {
  public readonly employee = input.required<EmployeeDto | null>();
  public readonly size = input<AvatarSize>("normal");

  protected readonly initials = computed(() => Converters.getInitials(this.employee()?.fullName));

  protected readonly colors = computed<AvatarColors>(() => {
    const role = this.employee()?.role?.name?.toLowerCase();
    switch (role) {
      case "owner":
        return {
          bg: "color-mix(in srgb, var(--warning) 15%, transparent)",
          text: "var(--warning)",
        };
      case "manager":
        return { bg: "color-mix(in srgb, var(--info) 15%, transparent)", text: "var(--info)" };
      case "dispatcher":
        return {
          bg: "color-mix(in srgb, var(--primary-500) 15%, transparent)",
          text: "var(--primary-500)",
        };
      case "driver":
        return {
          bg: "color-mix(in srgb, var(--success) 15%, transparent)",
          text: "var(--success)",
        };
      default:
        return { bg: "var(--bg-hover)", text: "var(--text-secondary)" };
    }
  });
}
