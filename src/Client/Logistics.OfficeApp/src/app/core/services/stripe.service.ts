import {Injectable} from "@angular/core";
import {
  SetupIntentResult,
  Stripe,
  StripeCardNumberElement,
  StripeElements,
  loadStripe,
} from "@stripe/stripe-js";
import {firstValueFrom} from "rxjs";
import {environment} from "src/environments/environment";
import {ApiService} from "../api";
import {AddressDto} from "../api/models";
import {COUNTRIES_OPTIONS} from "../constants";
import {findOption} from "../utilities";

@Injectable({providedIn: "root"})
export class StripeService {
  private stripe: Stripe | null = null;
  private elements: StripeElements | null = null;
  private clientSecret: string | null = null;

  constructor(private readonly apiService: ApiService) {}

  /**
   * Creates and returns a Stripe Elements instance with a setup intent client secret.
   * @returns A promise that resolves to the Stripe Elements instance.
   */
  async getElements(): Promise<StripeElements> {
    if (this.elements) {
      return this.elements;
    }

    const stripe = await this.getStripe();
    const result = await firstValueFrom(this.apiService.paymentApi.createSetupIntent());

    if (!result.success) {
      throw new Error("Failed to create setup intent");
    }

    this.clientSecret = result.data!.clientSecret;
    this.elements = stripe.elements({clientSecret: this.clientSecret});
    return this.elements;
  }

  /**
   * Confirms the card setup using Stripe Elements and the provided billing address.
   * @param cardElement The Stripe Card Element to use for the card number.
   * @param cardHolderName The name of the cardholder.
   * @param billingAddress The billing address of the cardholder.
   * @returns A promise that resolves to the SetupIntentResult.
   */
  async confirmCardSetup(
    cardElement: StripeCardNumberElement,
    cardHolderName: string,
    billingAddress: AddressDto
  ): Promise<SetupIntentResult> {
    if (!this.clientSecret) {
      throw new Error("Client secret is not set, call getElements() first");
    }

    const stripe = await this.getStripe();
    const countryOption = findOption(COUNTRIES_OPTIONS, billingAddress.country);

    return stripe.confirmCardSetup(this.clientSecret, {
      payment_method: {
        card: cardElement,
        billing_details: {
          name: cardHolderName,
          address: {
            line1: billingAddress.line1,
            line2: billingAddress.line2,
            city: billingAddress.city,
            state: billingAddress.state,
            postal_code: billingAddress.zipCode,
            country: countryOption?.value, // 2-letter country code
          },
        },
      },
    });
  }

  /**
   * Initializes the Stripe object with the public key from the environment if it hasn't been initialized yet.
   * @returns A promise that resolves to the Stripe object.
   */
  private async getStripe(): Promise<Stripe> {
    if (!this.stripe) {
      const stripe = await loadStripe(environment.stripePubKey);

      if (!stripe) {
        throw new Error("Stripe failed to initialize");
      }

      this.stripe = stripe;
    }

    return this.stripe;
  }
}
