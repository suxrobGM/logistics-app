import { Component } from "@angular/core";
import { UiAccordionImports } from "@logistics/shared/ui";
import { SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";
import type { FaqItem } from "@/shared/services";

/** One list for the FAQ section and the FAQ schema on the home page, so the two cannot drift. */
export const HOME_FAQS: FaqItem[] = [
  {
    question: "What is a TMS?",
    answer:
      "A transportation management system is the software a trucking company runs on. Dispatch, tracking, invoicing, and compliance live in one place instead of five spreadsheets.",
  },
  {
    question: "Does this work for a one-truck owner-operator?",
    answer:
      "Yes. Switch the account to solo mode and the parts built for a crew, like team invites, dispatcher assignment, and driver messaging, get out of the way. The dispatch agent plans around your truck and your hours instead of comparing trucks you don't have. One truck on Starter is $41 a month.",
  },
  {
    question: "How long does setup take?",
    answer:
      "You do it yourself, usually in an afternoon. A checklist appears on the dashboard after sign-up: company profile, first truck, your team, a customer, your first load, payouts, and your ELD. Each item links to the page that finishes it and ticks itself off. Nothing waits on us.",
  },
  {
    question: "Do you integrate with ELD providers?",
    answer:
      "Yes. Samsara, Motive, Geotab, and TT ELD. Hours of service come through on their own, so you can see who has hours left before you dispatch.",
  },
  {
    question: "Does the AI really negotiate with brokers?",
    answer:
      "It drafts the counter-offer and you send it. Set a minimum rate per lane. When a load board listing pays less, the agent writes an email to the broker and puts it in front of a dispatcher. You read the exact message before it goes out. Broker replies land back in the dispatch conversation, and booking the load is still a separate approval. It is included from the Professional plan up.",
  },
  {
    question: "Is there a mobile app for drivers?",
    answer:
      "Yes, for iOS and Android. Drivers see their trips, update load status, capture proof of delivery, fill in inspection reports, and chat with dispatch from their phone.",
  },
  {
    question: "How is pricing calculated?",
    answer:
      "Three plans: Starter ($29/mo + $12/truck), Professional ($79/mo + $9/truck), and Enterprise ($169/mo + $6/truck). Each is a base fee plus a per-truck charge. The more trucks you add, the lower the per-truck price.",
  },
  {
    question: "Do you offer a free trial?",
    answer:
      "Yes. Your first subscription starts with a 30-day free trial, and we do not ask for a card. After that it is month to month, from $29 plus $12 per truck. Cancel whenever you like. If you would rather see it with your own data first, book a demo and we will load some in.",
  },
  {
    question: "What kind of support do you offer?",
    answer:
      "Email and in-app chat on every plan, Starter included. Enterprise adds priority support. The people answering have worked in trucking, so you will not be explaining what a BOL is.",
  },
  {
    question: "Is LogisticsX open source?",
    answer:
      "The source is public on GitHub under the PolyForm Noncommercial license. It is free to read, run, and change for personal projects, research, and evaluation. Running it for a business, hosting it for others, or selling a product built on it needs a commercial license. See the Source License page for the options and prices.",
  },
  {
    question: "Is my data secure?",
    answer:
      "Each company's data lives in its own database, and all traffic is encrypted in transit. You can export or delete your data at any time, and GDPR requests are handled inside the app.",
  },
  {
    question: "How does AI Dispatch work?",
    answer:
      "The agent looks at unassigned loads, available trucks, hours of service, truck type, and revenue per mile, then proposes assignments in a chat session. You approve, reject, or send it back with feedback. Nothing dispatches until you say so, and every decision is logged with the reasoning.",
  },
  {
    question: "What happens if I reject an AI suggestion?",
    answer:
      "Reject it, add a note if you want, and the agent plans again with that in mind. Over time it learns from what you approve and reject, so the suggestions get closer to how your dispatchers actually decide.",
  },
  {
    question: "Is AI Dispatch safe to use?",
    answer:
      "Every suggestion waits for your approval, and every decision is logged with the agent's reasoning. You can always see what it looked at and why it chose what it chose.",
  },
  {
    question: "What can the AI copilot do?",
    answer:
      "It is a chat drawer across the whole TMS. Ask it about loads, expenses, or which trucks are due for service, or have it invoice delivered loads and send payment links. It only sees the data your role permits, answers come from your live data, and anything that changes the books waits for your approval.",
  },
];

@Component({
  selector: "web-faq",
  templateUrl: "./faq.html",
  imports: [UiAccordionImports, SectionContainer, SectionHeader, ScrollAnimateDirective],
})
export class Faq {
  protected readonly faqs = HOME_FAQS;
}
