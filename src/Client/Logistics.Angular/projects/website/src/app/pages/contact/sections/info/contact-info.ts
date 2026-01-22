import { Component } from "@angular/core";
import { IconCircle, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface ContactCard {
  icon: string;
  title: string;
  lines: string[];
  link?: { href: string; text: string };
}

@Component({
  selector: "web-contact-info",
  templateUrl: "./contact-info.html",
  imports: [SectionContainer, SectionHeader, IconCircle, ScrollAnimateDirective],
})
export class ContactInfo {
  protected readonly contacts: ContactCard[] = [
    {
      icon: "pi-envelope",
      title: "Email Us",
      lines: ["General inquiries", "Sales questions"],
      link: { href: "mailto:hello@logisticstms.com", text: "hello@logisticstms.com" },
    },
    {
      icon: "pi-phone",
      title: "Call Us",
      lines: ["Monday - Friday", "9:00 AM - 6:00 PM CT"],
      link: { href: "tel:+18005551234", text: "(800) 555-1234" },
    },
    {
      icon: "pi-map-marker",
      title: "Visit Us",
      lines: ["123 Fleet Street, Suite 500", "Houston, TX 77001"],
    },
  ];

  protected readonly socialLinks = [
    { icon: "pi-twitter", href: "#", label: "Twitter" },
    { icon: "pi-linkedin", href: "#", label: "LinkedIn" },
    { icon: "pi-facebook", href: "#", label: "Facebook" },
  ];
}
