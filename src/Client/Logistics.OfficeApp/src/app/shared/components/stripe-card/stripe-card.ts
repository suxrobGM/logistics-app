import {
  type AfterViewInit,
  Component,
  ElementRef,
  type OnDestroy,
  inject,
  output,
  viewChild,
} from "@angular/core";
import  type { StripeCardNumberElement, StripeElementBase } from "@stripe/stripe-js";
import { StripeService } from "@/core/services";

interface StripeCardElementsReady {
  cardNumber: StripeCardNumberElement;
}

@Component({
  selector: "app-stripe-card",
  templateUrl: "./stripe-card.html",
})
export class StripeCard implements OnDestroy, AfterViewInit {
  private readonly stripeService = inject(StripeService);

  public readonly ready = output<StripeCardElementsReady>();
  private readonly cardNumberElement = viewChild.required<ElementRef<HTMLElement>>("cardNumber");
  private readonly cardExpiryElement = viewChild.required<ElementRef<HTMLElement>>("cardExpiry");
  private readonly cardCvcElement = viewChild.required<ElementRef<HTMLElement>>("cardCvc");
  private readonly mountedElements: StripeElementBase[] = [];

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
    this.ready.emit({ cardNumber });
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
