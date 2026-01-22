import { Component } from "@angular/core";

interface Step {
  number: number;
  title: string;
  description: string;
  icon: string;
}

@Component({
  selector: "web-how-it-works",
  templateUrl: "./how-it-works.html",
})
export class HowItWorks {
  protected readonly steps: Step[] = [
    {
      number: 1,
      title: "Sign Up",
      description:
        "Create your company account in just a few minutes with a simple onboarding process.",
      icon: "pi-user-plus",
    },
    {
      number: 2,
      title: "Add Your Fleet",
      description: "Import your trucks, trailers, and driver information quickly and easily.",
      icon: "pi-truck",
    },
    {
      number: 3,
      title: "Start Dispatching",
      description: "Assign loads, track deliveries, and communicate with drivers in real-time.",
      icon: "pi-play",
    },
    {
      number: 4,
      title: "Get Paid",
      description: "Automated invoicing and payment collection to improve your cash flow.",
      icon: "pi-wallet",
    },
  ];
}
