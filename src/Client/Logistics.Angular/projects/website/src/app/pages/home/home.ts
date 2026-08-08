import { Component, inject, type OnInit } from "@angular/core";
import { MetaService, SchemaService } from "@/shared/services";
import {
  AIShowcase,
  Faq,
  Features,
  Hero,
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

    this.schemaService.setFaqSchema([
      {
        question: "What is a TMS?",
        answer:
          "A Transportation Management System (TMS) is software that helps trucking companies manage their fleet operations, including dispatching, tracking, invoicing, and compliance.",
      },
      {
        question: "How long does setup take?",
        answer:
          "You set it up yourself, usually in an afternoon. After sign-up a checklist appears on the dashboard covering the company profile, your first truck, your team, a customer, your first load, payouts, and your ELD. Each item links to the page that completes it, and it ticks itself off as you go. There is no onboarding call to book first.",
      },
      {
        question: "Does this work for a one-truck owner-operator?",
        answer:
          "Yes. Solo mode hides the parts of the app built for a crew - team invites, dispatcher assignment, driver messaging - and the AI dispatch agent plans around your single truck and your hours instead of comparing trucks. The Starter plan costs $29/mo plus $12 for the truck, so $41/mo for one truck.",
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
          "The AI agent analyzes unassigned loads, available trucks, HOS compliance, truck type compatibility, and revenue per mile, then proposes assignments in a chat-based dispatch session. A dispatcher reviews and approves each suggestion before anything is dispatched.",
      },
      {
        question: "What happens if I reject an AI suggestion?",
        answer:
          "You can reject any suggestion and optionally hand back context, and the agent re-plans with your feedback. In human-in-the-loop mode nothing is dispatched until you approve it.",
      },
      {
        question: "Is AI Dispatch safe to use?",
        answer:
          "Yes. Every suggestion goes through you before it's dispatched, and every action has a full audit trail with the agent's reasoning.",
      },
    ]);
  }
}
