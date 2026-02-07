import { Component } from "@angular/core";
import { SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

@Component({
  selector: "web-story",
  templateUrl: "./story.html",
  imports: [SectionContainer, SectionHeader, ScrollAnimateDirective],
})
export class Story {}
