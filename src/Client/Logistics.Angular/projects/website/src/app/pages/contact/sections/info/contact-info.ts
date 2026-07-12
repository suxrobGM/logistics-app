import { Component } from "@angular/core";
import { Icon, type IconName } from "@logistics/shared/ui";
import { IconCircle, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface ContactCard {
  icon: IconName;
  title: string;
  lines: string[];
  link?: { href: string; text: string };
}

@Component({
  selector: "web-contact-info",
  templateUrl: "./contact-info.html",
  imports: [Icon, IconCircle, ScrollAnimateDirective, SectionContainer, SectionHeader],
})
export class ContactInfo {
  protected readonly contacts: ContactCard[] = [
    {
      icon: "mail",
      title: "Email Us",
      lines: ["General inquiries", "Sales questions"],
      link: { href: "mailto:hello@logisticsx.app", text: "hello@logisticsx.app" },
    },
    {
      icon: "phone",
      title: "Call Us",
      lines: ["Monday - Friday", "9:00 AM - 6:00 PM CT"],
      link: { href: "tel:+18578671942", text: "(857) 867-1942" },
    },
    {
      icon: "map-pin",
      title: "Visit Us",
      lines: ["10016 Sandmeyer Ln", "Philadelphia, PA 19116"],
    },
  ];

  protected readonly socialLinks: { icon: IconName; href: string; label: string }[] = [
    { icon: "brand-x", href: "#", label: "Twitter" },
    { icon: "brand-linkedin", href: "#", label: "LinkedIn" },
    { icon: "brand-facebook", href: "#", label: "Facebook" },
  ];
}
