import {SubscriptionDto, SubscriptionStatus} from "../api/models";

export type SeverityLevel = "success" | "warn" | "danger" | "info";

export const Labels = {
  /**
   * Get the severity of the subscription status.
   * @param subscription The subscription object.
   * @returns The severity of the subscription status.
   */
  subscriptionStatusSeverity(subscription: SubscriptionDto): SeverityLevel {
    switch (subscription.status) {
      case SubscriptionStatus.Active:
      case SubscriptionStatus.Trialing:
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
      case SubscriptionStatus.Active:
        return "Active";
      case SubscriptionStatus.Incomplete:
        return "Incomplete";
      case SubscriptionStatus.Trialing:
        return "Trialing";
      case SubscriptionStatus.PastDue:
        return "Past Due";
      case SubscriptionStatus.Unpaid:
        return "Unpaid";
      case SubscriptionStatus.IncompleteExpired:
        return "Incomplete Expired";
      case SubscriptionStatus.Paused:
        return "Paused";
      case SubscriptionStatus.Cancelled:
        return "Cancelled";
      default:
        return "Unknown";
    }
  },
} as const;
