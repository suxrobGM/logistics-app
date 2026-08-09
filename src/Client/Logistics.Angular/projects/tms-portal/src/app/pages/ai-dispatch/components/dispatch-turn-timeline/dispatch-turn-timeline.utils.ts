import type { AgentDecisionDto } from "@logistics/shared/api";
import type { IconName } from "@logistics/shared/ui";
import { getToolIcon, getToolLabel, isWriteDecision } from "@/shared/utils";

export type TurnEntry =
  | {
      readonly kind: "reads";
      readonly id: string;
      readonly decisions: AgentDecisionDto[];
      readonly summary: ReadGroupSummary;
    }
  | { readonly kind: "write"; readonly id: string; readonly decision: AgentDecisionDto };

export interface ReadChip {
  readonly label: string;
  readonly icon: IconName;
  count: number;
  failed: boolean;
}

export interface ReadGroupSummary {
  readonly chips: ReadChip[];
  readonly keyFigures: string;
}

/**
 * Collapses a turn's decisions into render entries: consecutive read tools merge into one `reads`
 * group so the transcript shows a single "Checked:" row instead of a gray block per lookup. Write
 * tools break the run - each stays standalone so its approve/reject stays prominent.
 */
export function groupTurnEntries(decisions: readonly AgentDecisionDto[]): TurnEntry[] {
  const groups: (AgentDecisionDto[] | AgentDecisionDto)[] = [];
  let openGroup: AgentDecisionDto[] | undefined;

  for (const decision of decisions) {
    if (isWriteDecision(decision)) {
      openGroup = undefined;
      groups.push(decision);
    } else if (openGroup) {
      openGroup.push(decision);
    } else {
      openGroup = [decision];
      groups.push(openGroup);
    }
  }

  return groups.map((group) =>
    Array.isArray(group)
      ? {
          kind: "reads" as const,
          id: group[0].id ?? "",
          decisions: group,
          summary: readGroupSummary(group),
        }
      : { kind: "write" as const, id: group.id ?? "", decision: group },
  );
}

/** Deduped tool chips plus a one-line figure summary for a collapsed `reads` group. */
export function readGroupSummary(decisions: readonly AgentDecisionDto[]): ReadGroupSummary {
  const chipsByTool = new Map<string, ReadChip>();
  let loadCount: number | undefined;
  let availableTrucks: number | undefined;
  let totalTrucks: number | undefined;

  for (const decision of decisions) {
    const output = decision.toolResult;
    const toolName = decision.toolName ?? "";
    const chip = chipsByTool.get(toolName);

    if (chip) {
      chip.count += 1;
      chip.failed = chip.failed || !!output?.error;
    } else {
      chipsByTool.set(toolName, {
        label: getToolLabel(decision.toolName),
        icon: getToolIcon(decision.toolName),
        count: 1,
        failed: !!output?.error,
      });
    }

    loadCount ??= output?.loads?.length;
    availableTrucks ??= output?.fleetSummary?.availableTrucks;
    totalTrucks ??= output?.fleetSummary?.totalTrucks;
  }

  const figures: string[] = [];
  if (loadCount !== undefined) {
    figures.push(`${loadCount} unassigned load${loadCount === 1 ? "" : "s"}`);
  }
  if (availableTrucks !== undefined && totalTrucks !== undefined) {
    figures.push(`${availableTrucks}/${totalTrucks} trucks available`);
  }

  return { chips: [...chipsByTool.values()], keyFigures: figures.join(" · ") };
}
