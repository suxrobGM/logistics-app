import {AfterViewInit, Component, ElementRef, OnDestroy, output, viewChild} from "@angular/core";
import {StripeCardNumberElement, StripeElementBase} from "@stripe/stripe-js";
import {StripeService} from "@/core/services";

interface StripeCardElementsReady {
  cardNumber: StripeCardNumberElement;
}

@Component({
  selector: "app-stripe-card",
  templateUrl: "./stripe-card.component.html",
})
export class StripeCardComponent implements OnDestroy, AfterViewInit {
  public readonly ready = output<StripeCardElementsReady>();
  private readonly cardNumberElement = viewChild.required<ElementRef>("cardNumber");
  private readonly cardExpiryElement = viewChild.required<ElementRef>("cardExpiry");
  private readonly cardCvcElement = viewChild.required<ElementRef>("cardCvc");
  private readonly mountedElements: StripeElementBase[] = [];

  constructor(private readonly stripeService: StripeService) {}

  ngAfterViewInit(): void {
    this.mountElements();
  }

  ngOnDestroy(): void {
    this.unmountElements();
  }

  /**
   * Mounts the Stripe Elements to the DOM.
   */
  private async mountElements(): Promise<void> {
    if (this.mountedElements.length > 0) {
      console.log("Stripe Elements already mounted");
      return;
    }

    const elements = await this.stripeService.getElements();
    const cardNumber = elements.getElement("cardNumber") ?? elements.create("cardNumber");
    const cardExpiry = elements.getElement("cardExpiry") ?? elements.create("cardExpiry");
    const cardCvc = elements.getElement("cardCvc") ?? elements.create("cardCvc");

    cardNumber.mount(this.cardNumberElement().nativeElement);
    cardExpiry.mount(this.cardExpiryElement().nativeElement);
    cardCvc.mount(this.cardCvcElement().nativeElement);

    this.mountedElements.push(cardNumber, cardExpiry, cardCvc);
    this.ready.emit({cardNumber});
    console.log("Mounted Stripe Elements");
  }

  private async unmountElements(): Promise<void> {
    for (const element of this.mountedElements) {
      element.unmount();
    }
    this.mountedElements.splice(0, this.mountedElements.length);
    console.log("Unmounted Stripe Elements");
  }
}
