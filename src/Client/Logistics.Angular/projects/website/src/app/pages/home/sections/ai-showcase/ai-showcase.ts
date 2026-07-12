import { Component } from "@angular/core";
import type { IconName } from "@logistics/shared/ui";
import { BrowserFrame, IconCircle, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface WorkflowStep {
  title: string;
  description: string;
  icon: IconName;
}

interface CapabilityCard {
  title: string;
  description: string;
  icon: IconName;
}

@Component({
  selector: "web-ai-showcase",
  templateUrl: "./ai-showcase.html",
  imports: [SectionContainer, SectionHeader, BrowserFrame, IconCircle, ScrollAnimateDirective],
})
export class AiShowcase {
  protected readonly workflowSteps: WorkflowStep[] = [
    {
      title: "Pull fleet state",
      description: "Reads unassigned loads, truck locations, and who's available",
      icon: "search",
    },
    {
      title: "Compare loads to trucks",
      description: "Looks at truck-type compatibility and revenue per mile",
      icon: "chart-column",
    },
    {
      title: "Run HOS checks",
      description: "Confirms each driver has the hours to take the trip",
      icon: "shield",
    },
    {
      title: "Score matches",
      description: "Ranks every feasible load-to-truck pairing",
      icon: "calculator",
    },
    {
      title: "Assign and dispatch",
      description: "Sends suggestions for approval, or runs them itself",
      icon: "circle-check",
    },
  ];

  protected readonly capabilities: CapabilityCard[] = [
    {
      title: "Human-in-the-loop",
      description:
        "The agent suggests assignments. You approve, reject, or hand back context to re-plan. You stay in control.",
      icon: "user-pen",
    },
    {
      title: "Autonomous",
      description:
        "The agent runs on its own - assigns loads, creates trips, and dispatches without a human in the loop.",
      icon: "zap",
    },
    {
      title: "Full audit trail",
      description:
        "Every assignment is logged with the agent's reasoning - what it saw, why it picked that truck, and who approved it.",
      icon: "history",
    },
  ];
}
