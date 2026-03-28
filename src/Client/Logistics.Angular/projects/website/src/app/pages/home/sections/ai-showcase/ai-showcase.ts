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
      title: "Gather Fleet State",
      description: "Scans unassigned loads, truck locations, and driver availability",
      icon: "pi-search",
    },
    {
      title: "Analyze Loads & Trucks",
      description: "Evaluates truck type compatibility and revenue per mile",
      icon: "pi-chart-bar",
    },
    {
      title: "Check HOS Compliance",
      description: "Verifies each driver has sufficient hours of service remaining",
      icon: "pi-shield",
    },
    {
      title: "Calculate Optimal Matches",
      description: "Scores every feasible load-to-truck combination",
      icon: "pi-calculator",
    },
    {
      title: "Assign & Dispatch",
      description: "Submits assignments for approval or executes autonomously",
      icon: "pi-check-circle",
    },
  ];

  protected readonly capabilities: CapabilityCard[] = [
    {
      title: "Human-in-the-Loop",
      description:
        "Agent suggests assignments for your review. Approve, reject, or re-plan with context — you stay in control.",
      icon: "pi-user-edit",
    },
    {
      title: "Fully Autonomous",
      description:
        "Agent executes decisions in real-time — assigns loads, creates trips, and dispatches without manual intervention.",
      icon: "pi-bolt",
    },
    {
      title: "Multi-Provider AI",
      description:
        "Choose from Claude, GPT, or DeepSeek models. Tiered access by plan with transparent per-session quota tracking.",
      icon: "pi-sparkles",
    },
  ];
}
