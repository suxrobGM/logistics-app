import { Component, inject, type OnInit } from "@angular/core";
import { MetaService, SchemaService } from "@/shared/services";
import {
  Faq,
  Features,
  Hero,
  HowItWorks,
  Integrations,
  Pricing,
  ProductShowcase,
  Testimonials,
} from "./sections";

@Component({
  selector: "web-home",
  templateUrl: "./home.html",
  imports: [Hero, Features, ProductShowcase, Integrations, HowItWorks, Testimonials, Pricing, Faq],
})
export class Home implements OnInit {
  private readonly metaService = inject(MetaService);
  private readonly schemaService = inject(SchemaService);

  ngOnInit(): void {
    this.metaService.updateMeta({
      title: "Modern Fleet Management Platform",
      description:
        "Streamline your trucking operations with real-time GPS tracking, automated dispatching, and seamless invoicing. Request a demo today.",
      keywords:
        "TMS, fleet management, trucking software, GPS tracking, dispatching, logistics, transportation management",
      canonicalUrl: "https://logisticstms.com/",
    });

    this.schemaService.setOrganizationSchema();
    this.schemaService.setSoftwareApplicationSchema();

    this.schemaService.setFaqSchema([
      {
        question: "What is a TMS?",
        answer:
          "A Transportation Management System (TMS) is software that helps trucking companies manage their fleet operations, including dispatching, tracking, invoicing, and compliance.",
      },
      {
        question: "How long does setup take?",
        answer:
          "Most companies are up and running within 1-2 days. Our onboarding team will help you import your data, set up your account, and train your staff.",
      },
      {
        question: "Do you integrate with ELD providers?",
        answer:
          "Yes! We integrate with major ELD providers including Samsara and Motive (KeepTruckin) for automatic hours of service tracking and FMCSA compliance.",
      },
      {
        question: "Is there a mobile app for drivers?",
        answer:
          "Yes, we offer native mobile apps for both iOS and Android. Drivers can capture proof of delivery, scan documents, navigate, and communicate with dispatchers.",
      },
      {
        question: "How is pricing calculated?",
        answer:
          "We offer three plans: Starter ($19/mo + $12/truck), Professional ($79/mo + $7/truck), and Enterprise ($149/mo + $4/truck). Each plan includes a base fee plus a per-truck charge. The more trucks you add, the lower your per-truck cost.",
      },
    ]);
  }
}
