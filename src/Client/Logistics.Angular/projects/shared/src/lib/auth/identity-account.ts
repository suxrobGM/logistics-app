/**
 * Self-service account pages hosted by the IdentityServer (`/Account/Manage/`). They live there
 * because they belong to the user, not a tenant - nothing gates them but being signed in.
 */
export type IdentityAccountPage =
  | "profile"
  | "email"
  | "changepassword"
  | "twofactorauthentication"
  | "privacy";

export function openIdentityAccountPage(
  identityServerUrl: string,
  page: IdentityAccountPage,
): void {
  window.open(`${identityServerUrl}/account/manage/${page}`, "_blank", "noopener,noreferrer");
}
