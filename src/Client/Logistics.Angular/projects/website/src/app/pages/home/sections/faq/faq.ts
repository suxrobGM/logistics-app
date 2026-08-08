import { Component } from "@angular/core";
import { UiAccordionImports } from "@logistics/shared/ui";
import { SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface FaqItem {
  question: string;
  answer: string;
}

@Component({
  selector: "web-faq",
  templateUrl: "./faq.html",
  imports: [UiAccordionImports, SectionContainer, SectionHeader, ScrollAnimateDirective],
})
export class Faq {
  protected readonly faqs: FaqItem[] = [
    {
      question: "What is a TMS?",
      answer:
        "A Transportation Management System is software for running a trucking company - dispatching, tracking, invoicing, and compliance, in one place instead of five spreadsheets.",
    },
    {
      question: "Does this work for a one-truck owner-operator?",
      answer:
        "Yes. Switch the account to solo mode and the parts built for a crew - team invites, dispatcher assignment, driver messaging - get out of the way. The dispatch agent plans around your truck and your hours instead of comparing trucks you don't have. One truck on Starter is $41/mo.",
    },
    {
      question: "How long does setup take?",
      answer:
        "You do it yourself, usually in an afternoon. A checklist appears on the dashboard after sign-up: company profile, first truck, your team, a customer, your first load, payouts, and your ELD. Each item links to the page that finishes it and ticks itself off. Nothing waits on us.",
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
        "There's no time-limited trial. You sign up, set the account up yourself, and pay month to month from $29/mo plus $12 per truck - cancel whenever. If you'd rather see it against your own data before you commit, book a demo and we'll load some of it in.",
    },
    {
      question: "What kind of support do you offer?",
      answer:
        "Email and in-app chat on every plan, Starter included. Enterprise adds priority support. The people answering have worked in trucking, so you won't be explaining what a BOL is.",
    },
    {
      question: "Is my data secure?",
      answer:
        "Yes. Encryption at rest and in transit, regular security audits, daily backups, and data stored in secure data centers.",
    },
    {
      question: "How does AI Dispatch work?",
      answer:
        "The agent looks at unassigned loads, available trucks, HOS compliance, truck-type compatibility, and revenue per mile, and proposes assignments in a chat session. You approve, reject, or send it back with feedback - nothing dispatches until you say so. Every decision is logged with the reasoning.",
    },
    {
      question: "What happens if I reject an AI suggestion?",
      answer:
        "You can reject any suggestion and optionally hand back context, and the agent re-plans with your feedback. In human-in-the-loop mode nothing is dispatched until you approve it.",
    },
    {
      question: "Is AI Dispatch safe to use?",
      answer:
        "Yes. Nothing is dispatched without your approval - you review every suggestion before it runs. Each decision is in the audit trail with the agent's reasoning.",
    },
    {
      question: "What can the AI copilot do?",
      answer:
        "It's a chat drawer across the whole TMS. Ask it about loads, expenses, or which trucks are due for service, or have it invoice delivered loads and send payment links. It only sees the data your role permits, answers come from your live data, and anything that changes the books waits for your approval.",
    },
  ];
}
