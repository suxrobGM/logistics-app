import type { PlanTier, SubscriptionDto } from "@logistics/shared/api";

export type SeverityLevel = "success" | "warn" | "danger" | "info" | "secondary";

export const Labels = {
  /**
   * Get the display label for a plan tier.
   */
  planTierLabel(tier: PlanTier): string {
    switch (tier) {
      case "starter":
        return "Starter";
      case "professional":
        return "Professional";
      case "enterprise":
        return "Enterprise";
      default:
        return "Unknown";
    }
  },

  /**
   * Get the severity for a plan tier badge.
   */
  planTierSeverity(tier: PlanTier): SeverityLevel {
    switch (tier) {
      case "starter":
        return "info";
      case "professional":
        return "success";
      case "enterprise":
        return "warn";
      default:
        return "info";
    }
  },

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

  dispatchStatusSeverity(status: string): SeverityLevel {
    switch (status) {
      case "completed":
      case "executed":
      case "approved":
        return "success";
      case "running":
      case "suggested":
        return "info";
      case "failed":
        return "danger";
      case "cancelled":
      case "rejected":
        return "warn";
      default:
        return "secondary";
    }
  },

  decisionTypeLabel(type: string): string {
    switch (type) {
      case "assign_load":
        return "Assign Load";
      case "create_trip":
        return "Create Trip";
      case "dispatch_trip":
        return "Dispatch Trip";
      case "book_load_board_load":
        return "Book Load";
      case "reassign_load":
        return "Reassign";
      default:
        return type;
    }
  },
} as const;
