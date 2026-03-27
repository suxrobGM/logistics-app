import { InjectionToken } from "@angular/core";

/**
 * Well-known error codes returned by the API for feature/limit blocks.
 * Must match the backend ErrorCodes constants.
 */
export const ErrorCodes = {
  FeatureNotInPlan: "FEATURE_NOT_IN_PLAN",
  FeatureDisabledByAdmin: "FEATURE_DISABLED_BY_ADMIN",
  ResourceLimitReached: "RESOURCE_LIMIT_REACHED",
} as const;

/**
 * Interface for handling upgrade-related errors.
 * Applications should implement this and provide it via the UPGRADE_HANDLER token.
 */
export interface IUpgradePromptHandler {
  /**
   * Show an upgrade prompt triggered by a feature/limit block.
   * @param errorCode The machine-readable error code from the API
   * @param message The user-facing error message from the API
   */
  showUpgradePrompt(errorCode: string, message: string): void;
}

/**
 * Injection token for the upgrade handler.
 * Applications can optionally provide an implementation to show upgrade dialogs.
 */
export const UPGRADE_HANDLER = new InjectionToken<IUpgradePromptHandler>("UpgradeHandler");

/**
 * Check if an error code is an upgrade-related error that should show an upgrade dialog.
 */
export function isUpgradeError(errorCode: string | undefined | null): boolean {
  return errorCode === ErrorCodes.FeatureNotInPlan || errorCode === ErrorCodes.ResourceLimitReached;
}
