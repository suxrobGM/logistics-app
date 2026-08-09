import { Component, input, output } from "@angular/core";
import { UiButton } from "@logistics/shared/ui";

/**
 * Tells the user the hub is down and offers a manual reconnect. Hidden on the host rather than
 * removed, so a divider class the host puts on the tag disappears with it.
 */
@Component({
  selector: "app-realtime-banner",
  templateUrl: "./realtime-banner.html",
  imports: [UiButton],
  host: {
    class: "block",
    "[class.hidden]": "!visible()",
  },
})
export class RealtimeBanner {
  public readonly visible = input(false);
  public readonly retry = output<void>();
}
