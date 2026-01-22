import { Component } from "@angular/core";
import { SectionContainer } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

@Component({
  selector: "web-terms",
  templateUrl: "./terms.html",
  imports: [SectionContainer, ScrollAnimateDirective],
})
export class Terms {
  protected readonly lastUpdated = "January 15, 2026";

  protected readonly sections = [
    { id: "acceptance", title: "Acceptance of Terms" },
    { id: "description", title: "Description of Service" },
    { id: "accounts", title: "User Accounts" },
    { id: "subscriptions", title: "Subscriptions and Payments" },
    { id: "acceptable-use", title: "Acceptable Use" },
    { id: "intellectual-property", title: "Intellectual Property" },
    { id: "limitation", title: "Limitation of Liability" },
    { id: "indemnification", title: "Indemnification" },
    { id: "termination", title: "Termination" },
    { id: "governing-law", title: "Governing Law" },
    { id: "changes", title: "Changes to Terms" },
    { id: "contact", title: "Contact Information" },
  ];
}
