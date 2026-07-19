import { UserRole } from "@logistics/shared";
import { createRoleGuard } from "@logistics/shared/auth";

// Only app roles can access Admin Portal
export const authGuard = createRoleGuard({
  allowedRoles: [UserRole.AppSuperAdmin, UserRole.AppAdmin],
  unauthenticatedRedirect: "/",
});
