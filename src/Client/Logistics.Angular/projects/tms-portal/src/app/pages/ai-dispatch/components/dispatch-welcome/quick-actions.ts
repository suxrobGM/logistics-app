import type { IconName } from "@logistics/shared/ui";

export const PLAN_ASSIGNMENTS_PROMPT =
  "Plan assignments for all unassigned loads: check HOS feasibility and suggest load-truck assignments for my approval.";

interface QuickAction {
  icon: IconName;
  title: string;
  description: string;
  prompt: string;
}

export const QUICK_ACTIONS: readonly QuickAction[] = [
  {
    icon: "sparkles",
    title: "Plan assignments",
    description: "Match every unassigned load to a truck that can legally run it.",
    prompt: PLAN_ASSIGNMENTS_PROMPT,
  },
  {
    icon: "truck",
    title: "Fill idle trucks",
    description: "Find work for trucks sitting without a load.",
    prompt: "Find loads for my idle trucks",
  },
  {
    icon: "clock",
    title: "Review HOS risks",
    description: "Flag drivers close to running out of hours today.",
    prompt: "Review today's HOS risks",
  },
  {
    icon: "calendar",
    title: "Tomorrow's capacity",
    description: "See which trucks free up tomorrow.",
    prompt: "Any trucks free tomorrow?",
  },
  {
    icon: "circle-help",
    title: "What can you do?",
    description: "Learn what the dispatch agent can help with.",
    prompt: "What can you help me with?",
  },
];
