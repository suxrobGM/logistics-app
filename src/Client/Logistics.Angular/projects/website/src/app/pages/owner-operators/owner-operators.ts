import { Component, inject, type OnInit } from "@angular/core";
import { MetaService, SchemaService } from "@/shared/services";
import { InTheApp, OwnerOperatorsCta, OwnerOperatorsHero, SoloMode } from "./sections";

@Component({
  selector: "web-owner-operators",
  templateUrl: "./owner-operators.html",
  imports: [OwnerOperatorsHero, SoloMode, InTheApp, OwnerOperatorsCta],
})
export class OwnerOperators implements OnInit {
  private readonly metaService = inject(MetaService);
  private readonly schemaService = inject(SchemaService);

  ngOnInit(): void {
    this.metaService.updateMeta({
      title: "TMS for Owner-Operators",
      description:
        "Fleet software built to run on one truck. Solo mode drops the crew-shaped parts, the AI agent ranks loads instead of trucks, and the whole thing costs $41/mo for a single truck.",
      keywords:
        "owner operator TMS, one truck trucking software, solo trucker dispatch software, owner operator load board, small carrier TMS",
      canonicalUrl: "https://logisticsx.app/owner-operators",
    });

    this.schemaService.setBreadcrumbSchema([
      { name: "Home", url: "https://logisticsx.app/" },
      { name: "Owner-Operators", url: "https://logisticsx.app/owner-operators" },
    ]);

    this.schemaService.setFaqSchema([
      {
        question: "Is there a TMS for a single owner-operator?",
        answer:
          "Yes. LogisticsX runs in solo mode for a one-truck carrier: team invites, dispatcher assignment, and driver messaging drop out, and the AI dispatch agent ranks available loads by rate per mile against your hours instead of comparing trucks.",
      },
      {
        question: "What does it cost for one truck?",
        answer:
          "$41 a month - the $29 Starter base plus $12 for the truck. Month to month, no setup fee, and no minimum fleet size.",
      },
      {
        question: "Can the owner also be the driver?",
        answer:
          "Yes. The owner login carries driver permissions, so the same account works in the driver mobile app for assignments, proof of delivery, DVIR inspections, and navigation.",
      },
    ]);
  }
}
