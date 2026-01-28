import { Component, computed, input } from "@angular/core";
import { type EmployeeDto } from "@logistics/shared/api";
import { AvatarModule } from "primeng/avatar";
import { Converters } from "@/shared/utils";

export type AvatarSize = "normal" | "large" | "xlarge";

@Component({
  selector: "app-employee-avatar",
  templateUrl: "./employee-avatar.html",
  imports: [AvatarModule],
})
export class EmployeeAvatar {
  readonly employee = input.required<EmployeeDto | null>();
  readonly size = input<AvatarSize>("normal");

  protected readonly initials = computed(() => {
    const emp = this.employee();
    return Converters.getInitials(emp?.fullName);
  });

  protected readonly avatarStyle = computed(() => {
    const role = this.employee()?.role?.name?.toLowerCase();
    const colors = this.getRoleColors(role);
    return {
      "background-color": colors.bg,
      color: colors.text,
    };
  });

  private getRoleColors(role?: string): { bg: string; text: string } {
    switch (role) {
      case "owner":
        return { bg: "color-mix(in srgb, var(--warning) 15%, transparent)", text: "var(--warning)" };
      case "manager":
        return { bg: "color-mix(in srgb, var(--info) 15%, transparent)", text: "var(--info)" };
      case "dispatcher":
        return { bg: "color-mix(in srgb, var(--primary-500) 15%, transparent)", text: "var(--primary-500)" };
      case "driver":
        return { bg: "color-mix(in srgb, var(--success) 15%, transparent)", text: "var(--success)" };
      default:
        return { bg: "var(--bg-hover)", text: "var(--text-secondary)" };
    }
  }
}
