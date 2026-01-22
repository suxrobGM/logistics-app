import { Component } from "@angular/core";
import { SectionContainer } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface CookieInfo {
  name: string;
  purpose: string;
  duration: string;
  type: string;
}

@Component({
  selector: "web-cookies",
  templateUrl: "./cookies.html",
  imports: [SectionContainer, ScrollAnimateDirective],
})
export class Cookies {
  protected readonly lastUpdated = "January 15, 2026";

  protected readonly sections = [
    { id: "what-are-cookies", title: "What Are Cookies" },
    { id: "types-of-cookies", title: "Types of Cookies We Use" },
    { id: "cookie-table", title: "Cookie Details" },
    { id: "managing-cookies", title: "Managing Cookies" },
    { id: "third-party", title: "Third-Party Cookies" },
    { id: "changes", title: "Changes to This Policy" },
    { id: "contact", title: "Contact Us" },
  ];

  protected readonly essentialCookies: CookieInfo[] = [
    {
      name: "session_id",
      purpose: "Maintains your login session",
      duration: "Session",
      type: "Essential",
    },
    {
      name: "csrf_token",
      purpose: "Protects against cross-site request forgery",
      duration: "Session",
      type: "Essential",
    },
    {
      name: "tenant_id",
      purpose: "Identifies your organization",
      duration: "1 year",
      type: "Essential",
    },
  ];

  protected readonly analyticsCookies: CookieInfo[] = [
    {
      name: "_ga",
      purpose: "Google Analytics - distinguishes users",
      duration: "2 years",
      type: "Analytics",
    },
    {
      name: "_gid",
      purpose: "Google Analytics - distinguishes users",
      duration: "24 hours",
      type: "Analytics",
    },
    {
      name: "_gat",
      purpose: "Google Analytics - throttles request rate",
      duration: "1 minute",
      type: "Analytics",
    },
  ];

  protected readonly functionalCookies: CookieInfo[] = [
    {
      name: "preferences",
      purpose: "Stores your display preferences",
      duration: "1 year",
      type: "Functional",
    },
    {
      name: "language",
      purpose: "Remembers your language preference",
      duration: "1 year",
      type: "Functional",
    },
    {
      name: "timezone",
      purpose: "Stores your timezone setting",
      duration: "1 year",
      type: "Functional",
    },
  ];

  protected readonly marketingCookies: CookieInfo[] = [
    {
      name: "_fbp",
      purpose: "Facebook Pixel - tracks conversions",
      duration: "3 months",
      type: "Marketing",
    },
    {
      name: "_gcl_au",
      purpose: "Google Ads - conversion tracking",
      duration: "3 months",
      type: "Marketing",
    },
  ];
}
