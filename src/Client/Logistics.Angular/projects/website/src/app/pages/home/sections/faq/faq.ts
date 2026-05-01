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
        "A Transportation Management System is software for running a trucking company - dispatching, tracking, invoicing, and compliance, in one place instead of five spreadsheets.",
    },
    {
      question: "How long does setup take?",
      answer:
        "Usually 1-2 days. Our onboarding team helps you import data, set up the account, and train your staff. Support is on hand during the transition.",
    },
    {
      question: "Do you integrate with ELD providers?",
      answer:
        "Yes - Samsara and Motive (KeepTruckin). Hours of service tracking and FMCSA compliance pull through automatically.",
    },
    {
      question: "Is there a mobile app for drivers?",
      answer:
        "Yes, native apps for iOS and Android. Drivers can capture proof of delivery, scan documents, navigate, and chat with dispatch from their phone.",
    },
    {
      question: "How is pricing calculated?",
      answer:
        "Three plans: Starter ($29/mo + $12/truck), Professional ($79/mo + $9/truck), and Enterprise ($169/mo + $6/truck). It's a base fee plus a per-truck charge. The more trucks you add, the lower the per-truck price.",
    },
    {
      question: "Do you offer a free trial?",
      answer:
        "We start with a demo using your own data. If it looks like a fit, we'll set up a trial so you can see how it works before committing.",
    },
    {
      question: "What kind of support do you offer?",
      answer:
        "Support is available by phone, email, and chat. The team has people who've worked in trucking, not just generic SaaS support.",
    },
    {
      question: "Is my data secure?",
      answer:
        "Yes. Encryption at rest and in transit, regular security audits, daily backups, and data stored in secure data centers.",
    },
    {
      question: "How does AI Dispatch work?",
      answer:
        "The agent looks at unassigned loads, available trucks, HOS compliance, truck-type compatibility, and revenue per mile, and proposes assignments. In human-in-the-loop mode it suggests; in autonomous mode it acts. Either way, every decision is logged with the reasoning.",
    },
    {
      question: "Which AI models does it support?",
      answer:
        "Anthropic (Claude Sonnet, Haiku, Opus), OpenAI (GPT-5.4 series), and DeepSeek. You pick the model per session. Access is tiered: Starter is base models, Professional unlocks premium, Enterprise gets everything including Opus.",
    },
    {
      question: "Is AI Dispatch safe to use?",
      answer:
        "Yes. Human-in-the-loop is the default - you review every decision before it runs. Each action is in the audit trail with the agent's reasoning. Autonomous mode is opt-in, and a per-session iteration cap keeps it from running away.",
    },
  ];
}
