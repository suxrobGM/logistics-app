import {Component, OnDestroy, output} from "@angular/core";
import {StripeCardNumberElement, StripeElements} from "@stripe/stripe-js";
import {StripeService} from "@/core/services";

interface StripeCardElementsReady {
  cardNumber: StripeCardNumberElement;
}

@Component({
  selector: "app-stripe-card",
  templateUrl: "./stripe-card.component.html",
})
export class StripeCardComponent implements OnDestroy {
  readonly ready = output<StripeCardElementsReady>();
  private elements!: StripeElements;
  private mounted = false;

  constructor(private readonly stripeService: StripeService) {}

  ngOnDestroy(): void {
    this.elements.getElement("card")?.unmount();
    this.elements.getElement("cardExpiry")?.unmount();
    this.elements.getElement("cardCvc")?.unmount();
    this.mounted = false;
  }

  /**
   * Mounts the Stripe Elements to the DOM.
   * @returns A promise that resolves to the mounted elements.
   */
  async mountElements(): Promise<StripeCardElementsReady> {
    if (this.mounted) {
      return {
        cardNumber: this.elements.getElement("cardNumber")!,
      };
    }

    this.elements = await this.stripeService.getElements();
    const cardNumber = this.elements.create("cardNumber");
    const cardExpiry = this.elements.create("cardExpiry");
    const cardCvc = this.elements.create("cardCvc");
    cardNumber.mount("#card-number");
    cardExpiry.mount("#card-expiry");
    cardCvc.mount("#card-cvc");
    this.mounted = true;

    this.ready.emit({
      cardNumber,
    });

    return {
      cardNumber,
    };
  }
}
