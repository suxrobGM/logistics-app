import { Component } from "@angular/core";
import { AccordionModule } from "primeng/accordion";

interface FaqItem {
  question: string;
  answer: string;
}

@Component({
  selector: "web-faq",
  templateUrl: "./faq.html",
  imports: [AccordionModule],
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
        "Our pricing is simple: $30 per employee per month. This includes all features with no hidden fees. Whether you have 5 trucks or 500, you get access to the complete platform.",
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
  ];
}
