import { createRoleGuard } from "@logistics/shared/auth";

/**
 * Guard that checks if the user is authenticated.
 * Allows any authenticated user through - CustomerUser (no role) has portal access verified
 * afterwards by tenantGuard via the Portal.Access permission.
 */
export const authGuard = createRoleGuard({
  unauthenticatedRedirect: "/login",
  checkRoutePermission: false,
});
