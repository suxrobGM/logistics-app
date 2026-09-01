import { Component } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Icon } from "@logistics/shared/ui";
import { SectionContainer } from "@/shared/components";
import { GITHUB_REPO_URL } from "@/shared/constants";
import { ScrollAnimateDirective } from "@/shared/directives";

interface LicenseType {
  name: string;
  audience: string;
  grant: string;
  excluded: string;
  model: string;
  startsAt: string;
}

@Component({
  selector: "web-license",
  templateUrl: "./license.html",
  imports: [Icon, RouterLink, SectionContainer, ScrollAnimateDirective],
})
export class License {
  protected readonly lastUpdated = "September 1, 2026";
  protected readonly githubUrl = GITHUB_REPO_URL;
  protected readonly licenseFileUrl = `${GITHUB_REPO_URL}/blob/main/COMMERCIAL-LICENSE.md`;
  protected readonly contactHref =
    "mailto:suxrobgm@gmail.com?subject=LogisticsX%20commercial%20license";

  protected readonly sections = [
    { id: "overview", title: "Overview" },
    { id: "types", title: "License Types" },
    { id: "pricing", title: "Pricing" },
    { id: "terms", title: "Common Terms" },
    { id: "key", title: "License Key and Heartbeat" },
    { id: "buy", title: "How to Buy" },
  ];

  protected readonly types: LicenseType[] = [
    {
      name: "Internal Use",
      audience:
        "A carrier, broker, or logistics company running LogisticsX for its own operations.",
      grant: "Run any number of instances for one legal entity.",
      excluded: "Offering the product to third parties.",
      model: "Per truck per month, billed yearly",
      startsAt: "$4 per truck per month, minimum $3,000 per year",
    },
    {
      name: "Hosted / Reseller",
      audience: "A company that hosts LogisticsX and sells access to other businesses.",
      grant: "Run it as a service for up to the number of tenants written into the key.",
      excluded: "Redistributing the source or sublicensing it.",
      model: "Flat yearly fee by tenant count",
      startsAt: "$10,000 per year for up to 25 tenants, $25,000 for up to 100, $50,000 unlimited",
    },
    {
      name: "Perpetual Source",
      audience: "A company that forks LogisticsX and owns its copy.",
      grant: "A perpetual right to use and modify the source as of the purchase date.",
      excluded: "Updates after the first year, unless the update plan is paid.",
      model: "One-time fee plus optional updates",
      startsAt: "$25,000 once, updates at 25% of the fee per year",
    },
  ];

  protected readonly addOns = [
    {
      name: "Support add-on",
      model: "Yearly, email, 2 business day response",
      startsAt: "$2,500 per year",
    },
    {
      name: "Name license",
      model: "Yearly, permits use of the LogisticsX name and logo",
      startsAt: "$1,500 per year",
    },
  ];

  protected readonly terms = [
    "No sublicensing or resale of the source code.",
    "The LogisticsX name and logo are not included. A reseller must rebrand unless they buy the name license.",
    "No warranty. Liability is capped at the fees paid in the previous 12 months.",
    "Support is a separate add-on.",
    "Hosted / Reseller: the tenant count reported by the heartbeat must stay within the cap in the key. The author may verify it once a year with 30 days notice.",
    "The licensee agrees not to remove the license check, the noncommercial notice, or the instance heartbeat.",
    "Non-payment ends the license after 30 days notice. Rights revert to the noncommercial license.",
  ];

  protected readonly heartbeatFields = [
    "a random instance id generated on first start",
    "the server hostname",
    "the product version",
    "the license key id and licensee name, when a valid key is installed",
    "the number of tenants",
  ];
}
