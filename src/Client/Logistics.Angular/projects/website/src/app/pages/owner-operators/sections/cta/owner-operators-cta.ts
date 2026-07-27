import { Component, inject } from "@angular/core";
import { RouterLink } from "@angular/router";
import { UiButton } from "@logistics/shared/ui";
import { SectionContainer } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";
import { DemoDialogService } from "@/shared/services";

@Component({
  selector: "web-owner-operators-cta",
  templateUrl: "./owner-operators-cta.html",
  imports: [RouterLink, ScrollAnimateDirective, SectionContainer, UiButton],
})
export class OwnerOperatorsCta {
  private readonly demoDialogService = inject(DemoDialogService);

  protected openDemoDialog(): void {
    this.demoDialogService.open();
  }
}
