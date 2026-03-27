import { Injectable, inject } from "@angular/core";
import { Api, createSetupIntent } from "@logistics/shared/api";
import { type Stripe, loadStripe } from "@stripe/stripe-js";
import { environment } from "@/env";

@Injectable({ providedIn: "root" })
export class StripeService {
  private readonly api = inject(Api);

  private stripe: Stripe | null = null;

  /**
   * Creates a setup intent and returns the client secret.
   * Used for public payment link flows.
   */
  async getClientSecret(): Promise<string> {
    const result = await this.api.invoke(createSetupIntent, {});

    if (!result.clientSecret) {
      throw new Error("Failed to create setup intent");
    }

    return result.clientSecret;
  }

  /**
   * Initializes the Stripe object with the public key from the environment.
   */
  async getStripe(): Promise<Stripe> {
    if (!this.stripe) {
      const stripe = await loadStripe(environment.stripePublishableKey);

      if (!stripe) {
        throw new Error("Stripe failed to initialize");
      }

      this.stripe = stripe;
    }

    return this.stripe;
  }
}
