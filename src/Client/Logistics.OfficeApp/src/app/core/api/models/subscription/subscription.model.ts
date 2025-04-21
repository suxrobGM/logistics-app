import {SubscriptionStatus} from "./enums";
import {SubscriptionPlanDto} from "./subscription-plan.model";

export interface SubscriptionDto {
  id: string;
  status: SubscriptionStatus;
  plan?: SubscriptionPlanDto;
  startDate: Date;
  endDate?: Date;
  nextBillingDate?: Date;
  trialEndDate?: Date;
  stripeSubscriptionId?: string;
  stripeCustomerId?: string;
}
