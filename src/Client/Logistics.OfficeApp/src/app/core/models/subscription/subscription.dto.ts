import {SubscriptionPlanDto} from "./subscription-plan.dto";
import {SubscriptionStatus} from "./subscription-status.enum";

export interface SubscriptionDto {
  id?: string;
  status: SubscriptionStatus;
  plan?: SubscriptionPlanDto;
  startDate: Date;
  endDate?: Date;
  nextPaymentDate?: Date;
  trialEndDate?: Date;
  stripeSubscriptionId?: string;
  stripeCustomerId?: string;
}
