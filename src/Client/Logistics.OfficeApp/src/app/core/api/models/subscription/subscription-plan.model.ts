export interface SubscriptionPlanDto {
  id: string;
  name: string;
  description?: string;
  price: number;
  createdDate: Date;
  stripeProductId?: string;
  stripePriceId?: string;
  hasTrial: boolean;
}
