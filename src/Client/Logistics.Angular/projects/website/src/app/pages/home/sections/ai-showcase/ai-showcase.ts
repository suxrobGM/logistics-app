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
export class AIShowcase {
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
      title: "Propose assignments",
      description: "Sends ranked suggestions to the dispatcher, who approves or rejects each one",
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
      title: "Learns your preferences",
      description:
        "Every approval and rejection feeds back into the agent's suggestions, so it tracks how your dispatchers actually decide.",
      icon: "trending-up",
    },
    {
      title: "Negotiates by email",
      description:
        "When a load board listing pays below your lane floor, the agent drafts a counter-offer to the broker. You approve it before it sends, and the reply comes back into the same conversation.",
      icon: "mail",
    },
    {
      title: "TMS-wide copilot",
      description:
        "A chat drawer across the whole TMS. Ask about loads, spend, or maintenance - or have it invoice delivered loads and send payment links. Writes wait for your approval.",
      icon: "bot",
    },
    {
      title: "Full audit trail",
      description:
        "Every action either agent takes is logged with its reasoning - what it saw, why it chose it, and who approved it.",
      icon: "history",
    },
  ];
}
