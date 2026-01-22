import { Component } from "@angular/core";
import { SectionContainer } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

@Component({
  selector: "web-blog-hero",
  templateUrl: "./blog-hero.html",
  imports: [SectionContainer, ScrollAnimateDirective],
})
export class BlogHero {}
