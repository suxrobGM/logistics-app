import { Component } from "@angular/core";
import { BrowserFrame, IconCircle, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface WorkflowStep {
  title: string;
  description: string;
  icon: string;
}

interface CapabilityCard {
  title: string;
  description: string;
  icon: string;
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
      icon: "pi-search",
    },
    {
      title: "Compare loads to trucks",
      description: "Looks at truck-type compatibility and revenue per mile",
      icon: "pi-chart-bar",
    },
    {
      title: "Run HOS checks",
      description: "Confirms each driver has the hours to take the trip",
      icon: "pi-shield",
    },
    {
      title: "Score matches",
      description: "Ranks every feasible load-to-truck pairing",
      icon: "pi-calculator",
    },
    {
      title: "Assign and dispatch",
      description: "Sends suggestions for approval, or runs them itself",
      icon: "pi-check-circle",
    },
  ];

  protected readonly capabilities: CapabilityCard[] = [
    {
      title: "Human-in-the-loop",
      description:
        "The agent suggests assignments. You approve, reject, or hand back context to re-plan. You stay in control.",
      icon: "pi-user-edit",
    },
    {
      title: "Autonomous",
      description:
        "The agent runs on its own - assigns loads, creates trips, and dispatches without a human in the loop.",
      icon: "pi-bolt",
    },
    {
      title: "Full audit trail",
      description:
        "Every assignment is logged with the agent's reasoning - what it saw, why it picked that truck, and who approved it.",
      icon: "pi-history",
    },
  ];
}
