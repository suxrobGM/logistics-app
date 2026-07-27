import { Component } from "@angular/core";
import { IconCard, SectionContainer, SectionHeader, type IconCardItem } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

@Component({
  selector: "web-solo-mode",
  templateUrl: "./solo-mode.html",
  imports: [IconCard, ScrollAnimateDirective, SectionContainer, SectionHeader],
})
export class SoloMode {
  protected readonly changes: IconCardItem[] = [
    {
      icon: "sparkles",
      title: "The agent stops picking trucks",
      description:
        "In solo mode the dispatch agent compares loads instead of trucks. It ranks what's on the board by rate per mile against your hours and where you'll be sitting when you're done, and gives you one line per load.",
    },
    {
      icon: "user",
      title: "You're the owner and the driver",
      description:
        "Your login carries both roles, so the driver app works from the same account. Assignments, POD photos, DVIR inspections, and navigation on the phone; loads, invoices, and reports on the laptop.",
    },
    {
      icon: "check-square",
      title: "A checklist, not an implementation",
      description:
        "Company profile, your truck, a customer, your first load, payouts, and your ELD. The invite-your-team step never appears - it can't complete for one person, so solo mode leaves it out.",
    },
    {
      icon: "credit-card",
      title: "Getting paid without a bookkeeper",
      description:
        "Invoices generate off the delivered load and go out as a payment link. Stripe Connect drops the money in your bank. Fuel card transactions import overnight so IFTA builds itself from your own miles.",
    },
    {
      icon: "search",
      title: "Load boards with a credit check",
      description:
        "DAT, Truckstop, and 123Loadboard from one search box, with the broker's credit score and days-to-pay on every listing. A bad-credit booking gets blocked before you haul it.",
    },
    {
      icon: "wallet",
      title: "Priced for one truck",
      description:
        "$29 base plus $12 for the truck. Month to month, no setup fee, and no minimum fleet size hiding in the contract. Add a second truck and it's $53.",
    },
  ];
}
