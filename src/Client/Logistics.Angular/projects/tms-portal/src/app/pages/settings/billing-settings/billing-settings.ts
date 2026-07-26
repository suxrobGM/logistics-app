import { Component } from "@angular/core";
import { Container, Stack } from "@logistics/shared/ui";
import { PageHeader } from "@/shared/components";
import { StripeConnectCard, SubscriptionCard, TenantTaxRatesCard } from "../components";

/** Plan, payouts and tax rates in one tab. */
@Component({
  selector: "app-billing-settings",
  templateUrl: "./billing-settings.html",
  imports: [Container, PageHeader, Stack, StripeConnectCard, SubscriptionCard, TenantTaxRatesCard],
})
export class BillingSettings {}
