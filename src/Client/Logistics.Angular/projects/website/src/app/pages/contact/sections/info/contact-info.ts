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
      link: { href: "mailto:hello@logisticsx.app", text: "hello@logisticsx.app" },
    },
    {
      icon: "pi-phone",
      title: "Call Us",
      lines: ["Monday - Friday", "9:00 AM - 6:00 PM CT"],
      link: { href: "tel:+18578671942", text: "(857) 867-1942" },
    },
    {
      icon: "pi-map-marker",
      title: "Visit Us",
      lines: ["10016 Sandmeyer Ln", "Philadelphia, PA 19116"],
    },
  ];

  protected readonly socialLinks = [
    { icon: "pi-twitter", href: "#", label: "Twitter" },
    { icon: "pi-linkedin", href: "#", label: "LinkedIn" },
    { icon: "pi-facebook", href: "#", label: "Facebook" },
  ];
}
