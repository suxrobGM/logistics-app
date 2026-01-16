/**
 * Retrieves the authentication access token from session storage.
 * The angular-auth-oidc-client library stores the authentication data in session storage by default.
 * This function retrieves the token from the session storage key "0-logistics.tmsportal".
 * @returns The authentication access token or null if not found.
 */
export function getAccessToken(): string | null {
  const authData = JSON.parse(sessionStorage.getItem("0-logistics.tmsportal") || "{}");
  return authData?.authnResult?.access_token ?? null;
}
