import type { SubscriptionDto } from "@/core/api/models";

export type SeverityLevel = "success" | "warn" | "danger" | "info";

export const Labels = {
  /**
   * Get the severity of the subscription status.
   * @param subscription The subscription object.
   * @returns The severity of the subscription status.
   */
  subscriptionStatusSeverity(subscription: SubscriptionDto): SeverityLevel {
    switch (subscription.status) {
      case "active":
      case "trialing":
        return "success";
      default:
        return "danger";
    }
  },

  /**
   * Get the label of the subscription status.
   * @param subscription The subscription object.
   * @returns The label of the subscription status.
   */
  subscriptionStatus(subscription: SubscriptionDto): string {
    switch (subscription.status) {
      case "active":
        return "Active";
      case "incomplete":
        return "Incomplete";
      case "trialing":
        return "Trialing";
      case "past_due":
        return "Past Due";
      case "unpaid":
        return "Unpaid";
      case "incomplete_expired":
        return "Incomplete Expired";
      case "paused":
        return "Paused";
      case "cancelled":
        return "Cancelled";
      default:
        return "Unknown";
    }
  },
} as const;
