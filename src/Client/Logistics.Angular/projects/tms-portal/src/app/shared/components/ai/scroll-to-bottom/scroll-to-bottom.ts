import { Component, output } from "@angular/core";
import { UiButton } from "@logistics/shared/ui";

/** The floating "jump to latest" button that pairs with `pinnedScroll`. */
@Component({
  selector: "app-scroll-to-bottom",
  templateUrl: "./scroll-to-bottom.html",
  imports: [UiButton],
})
export class ScrollToBottom {
  public readonly scrollToBottom = output<void>();
}
