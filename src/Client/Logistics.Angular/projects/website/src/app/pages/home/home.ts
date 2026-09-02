import { Component, inject, type OnInit } from "@angular/core";
import { MetaService, SchemaService } from "@/shared/services";
import {
  AIShowcase,
  Faq,
  Features,
  Hero,
  HOME_FAQS,
  HowItWorks,
  Integrations,
  Pricing,
  ProductShowcase,
  Segments,
} from "./sections";

@Component({
  selector: "web-home",
  templateUrl: "./home.html",
  imports: [
    Hero,
    AIShowcase,
    Features,
    Segments,
    ProductShowcase,
    Integrations,
    HowItWorks,
    Pricing,
    Faq,
  ],
})
export class Home implements OnInit {
  private readonly metaService = inject(MetaService);
  private readonly schemaService = inject(SchemaService);

  ngOnInit(): void {
    this.metaService.updateMeta({
      title: "AI-Powered Fleet Management Platform",
      description:
        "AI-powered fleet dispatch that analyzes loads and trucks, verifies HOS compliance, and proposes assignments for dispatcher approval. Real-time GPS tracking, invoicing, and payroll for modern trucking companies.",
      keywords:
        "AI dispatch, AI fleet management, human-in-the-loop dispatch, TMS, trucking software, GPS tracking, load matching, HOS compliance, logistics",
      canonicalUrl: "https://logisticsx.app/",
    });

    this.schemaService.setOrganizationSchema();
    this.schemaService.setSoftwareApplicationSchema();

    this.schemaService.setFaqSchema(HOME_FAQS);
  }
}
