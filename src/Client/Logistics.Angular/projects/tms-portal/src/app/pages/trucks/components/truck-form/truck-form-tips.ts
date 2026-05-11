import { Component, input } from "@angular/core";
import { Icon, Stack, Surface, Typography } from "@logistics/shared/components";

@Component({
  selector: "app-truck-form-tips",
  templateUrl: "./truck-form-tips.html",
  imports: [Icon, Stack, Surface, Typography],
})
export class TruckFormTips {
  public readonly mode = input.required<"create" | "edit">();
}
