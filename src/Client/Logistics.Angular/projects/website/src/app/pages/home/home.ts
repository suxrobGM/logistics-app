import { Component, type OnInit, inject } from "@angular/core";
import { MetaService, SchemaService } from "@/shared/services";
import {
  AiShowcase,
  Faq,
  Features,
  Hero,
  HowItWorks,
  Integrations,
  Pricing,
  ProductShowcase,
} from "./sections";

@Component({
  selector: "web-home",
  templateUrl: "./home.html",
  imports: [Hero, AiShowcase, Features, ProductShowcase, Integrations, HowItWorks, Pricing, Faq],
})
export class Home implements OnInit {
  private readonly metaService = inject(MetaService);
  private readonly schemaService = inject(SchemaService);

  ngOnInit(): void {
    this.metaService.updateMeta({
      title: "AI-Powered Fleet Management Platform",
      description:
        "AI-powered fleet dispatch that autonomously matches loads to trucks, verifies HOS compliance, and optimizes routes. Real-time GPS tracking, invoicing, and payroll for modern trucking companies.",
      keywords:
        "AI dispatch, AI fleet management, autonomous dispatching, TMS, trucking software, GPS tracking, load matching, HOS compliance, logistics",
      canonicalUrl: "https://logisticsx.app/",
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
          "We offer three plans: Starter ($29/mo + $12/truck), Professional ($79/mo + $9/truck), and Enterprise ($169/mo + $6/truck). Each plan includes a base fee plus a per-truck charge. The more trucks you add, the lower your per-truck cost.",
      },
      {
        question: "How does AI Dispatch work?",
        answer:
          "The AI agent analyzes unassigned loads, available trucks, HOS compliance, truck type compatibility, and revenue per mile to find optimal assignments. In human-in-the-loop mode it suggests assignments for your approval; in autonomous mode it executes immediately.",
      },
      {
        question: "Which AI models does it support?",
        answer:
          "We support Anthropic (Claude Sonnet, Haiku, Opus), OpenAI (GPT-5.4 series), and DeepSeek. Model access is tiered by plan — Starter gets base models, Professional unlocks premium, and Enterprise gets all models.",
      },
      {
        question: "Is AI Dispatch safe to use?",
        answer:
          "Yes. Human-in-the-loop mode lets you review every decision before execution. Every action has a full audit trail with reasoning. Autonomous mode is opt-in only.",
      },
    ]);
  }
}
