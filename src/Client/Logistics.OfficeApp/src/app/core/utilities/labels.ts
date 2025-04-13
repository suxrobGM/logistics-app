import {SubscriptionDto, SubscriptionStatus} from "../api/models";

export type SeverityLevel = "success" | "warn" | "danger" | "info";

export abstract class Labels {
  /**
   * Get the severity of the subscription status.
   * @param subscription The subscription object.
   * @returns The severity of the subscription status.
   */
  static subscriptionStatusSeverity(subscription: SubscriptionDto): SeverityLevel {
    switch (subscription.status) {
      case SubscriptionStatus.Active:
        return "success";
      case SubscriptionStatus.Inactive:
        return "warn";
      case SubscriptionStatus.Cancelled:
        return "danger";
      default:
        return "info";
    }
  }

  /**
   * Get the label of the subscription status.
   * @param subscription The subscription object.
   * @returns The label of the subscription status.
   */
  static subscriptionStatus(subscription: SubscriptionDto): string {
    switch (subscription.status) {
      case SubscriptionStatus.Active:
        return "Active";
      case SubscriptionStatus.Inactive:
        return "Inactive";
      case SubscriptionStatus.Cancelled:
        return "Cancelled";
      default:
        return "Unknown";
    }
  }
}
