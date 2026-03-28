import { Component } from "@angular/core";
import { AccordionModule } from "primeng/accordion";
import { SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface FaqItem {
  question: string;
  answer: string;
}

@Component({
  selector: "web-faq",
  templateUrl: "./faq.html",
  imports: [AccordionModule, SectionContainer, SectionHeader, ScrollAnimateDirective],
})
export class Faq {
  protected readonly faqs: FaqItem[] = [
    {
      question: "What is a TMS?",
      answer:
        "A Transportation Management System (TMS) is software that helps trucking companies manage their fleet operations, including dispatching, tracking, invoicing, and compliance. It streamlines workflows and improves efficiency.",
    },
    {
      question: "How long does setup take?",
      answer:
        "Most companies are up and running within 1-2 days. Our onboarding team will help you import your data, set up your account, and train your staff. We also provide 24/7 support during the transition.",
    },
    {
      question: "Do you integrate with ELD providers?",
      answer:
        "Yes! We integrate with major ELD providers including Samsara and Motive (KeepTruckin). This allows automatic hours of service tracking and ensures FMCSA compliance.",
    },
    {
      question: "Is there a mobile app for drivers?",
      answer:
        "Yes, we offer native mobile apps for both iOS and Android. Drivers can capture proof of delivery, scan documents, navigate to destinations, and communicate with dispatchers all from their phone.",
    },
    {
      question: "How is pricing calculated?",
      answer:
        "We offer three plans: Starter ($29/mo + $12/truck), Professional ($79/mo + $9/truck), and Enterprise ($169/mo + $6/truck). Each plan includes a base fee plus a per-truck charge. The more trucks you add, the lower your per-truck cost.",
    },
    {
      question: "Do you offer a free trial?",
      answer:
        "We offer personalized demos where you can see the platform in action with your own data. After the demo, we can set up a trial period to ensure the platform meets your needs before committing.",
    },
    {
      question: "What kind of support do you offer?",
      answer:
        "We provide 24/7 customer support via phone, email, and chat. Our team includes logistics industry experts who understand the unique challenges of trucking companies.",
    },
    {
      question: "Is my data secure?",
      answer:
        "Absolutely. We use enterprise-grade security including encryption at rest and in transit, regular security audits, and compliance with industry standards. Your data is backed up daily and stored in secure data centers.",
    },
    {
      question: "How does AI Dispatch work?",
      answer:
        "The AI agent analyzes unassigned loads, available trucks, HOS compliance, truck type compatibility, and revenue per mile to find optimal assignments. In human-in-the-loop mode it suggests assignments for your approval; in autonomous mode it executes immediately. Every decision is logged with full reasoning.",
    },
    {
      question: "Which AI models does it support?",
      answer:
        "We support multiple providers: Anthropic (Claude Sonnet, Haiku, Opus), OpenAI (GPT-5.4 series), and DeepSeek. You choose the model per session. Model access is tiered by plan — Starter gets base models, Professional unlocks premium, and Enterprise gets all models including Opus.",
    },
    {
      question: "Is AI Dispatch safe to use?",
      answer:
        "Yes. Human-in-the-loop mode lets you review every decision before execution. Every action has a full audit trail with the agent's reasoning. Autonomous mode is opt-in only, and there's a maximum iteration limit per session to prevent runaway usage.",
    },
  ];
}
