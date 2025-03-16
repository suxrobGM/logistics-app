export interface SubscriptionPlanDto {
  name: string;
  description?: string;
  price: number;
  createdDate: Date;
  stripePriceId?: string;
  hasTrial: boolean;
}
