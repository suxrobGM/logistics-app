import { Component, input } from "@angular/core";

@Component({
  selector: "web-browser-frame",
  templateUrl: "./browser-frame.html",
})
export class BrowserFrame {
  public readonly src = input.required<string>();
  public readonly alt = input.required<string>();
  public readonly variant = input<"browser" | "mobile">("browser");
}
