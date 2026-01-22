import { Component } from "@angular/core";
import { SectionContainer } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

@Component({
  selector: "web-privacy",
  templateUrl: "./privacy.html",
  imports: [SectionContainer, ScrollAnimateDirective],
})
export class Privacy {
  protected readonly lastUpdated = "January 15, 2026";

  protected readonly sections = [
    { id: "introduction", title: "Introduction" },
    { id: "information-collected", title: "Information We Collect" },
    { id: "how-we-use", title: "How We Use Your Information" },
    { id: "information-sharing", title: "Information Sharing" },
    { id: "data-security", title: "Data Security" },
    { id: "your-rights", title: "Your Rights" },
    { id: "cookies", title: "Cookies and Tracking" },
    { id: "children", title: "Children's Privacy" },
    { id: "changes", title: "Changes to This Policy" },
    { id: "contact", title: "Contact Us" },
  ];
}
