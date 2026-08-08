import type { AgentDecisionDto } from "@logistics/shared/api";
import type { IconName } from "@logistics/shared/ui";
import { getToolIcon, getToolLabel, isWriteTool, parseToolOutput } from "@/shared/utils";

export type TurnEntry =
  | { readonly kind: "reads"; readonly id: string; readonly decisions: AgentDecisionDto[] }
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
  const entries: TurnEntry[] = [];
  let openGroup: AgentDecisionDto[] | undefined;

  for (const decision of decisions) {
    if (isWriteTool(decision.toolName)) {
      openGroup = undefined;
      entries.push({ kind: "write", id: decision.id ?? "", decision });
      continue;
    }
    if (openGroup) {
      openGroup.push(decision);
    } else {
      openGroup = [decision];
      entries.push({ kind: "reads", id: decision.id ?? "", decisions: openGroup });
    }
  }

  return entries;
}

/** Deduped tool chips plus a one-line figure summary for a collapsed `reads` group. */
export function readGroupSummary(decisions: readonly AgentDecisionDto[]): ReadGroupSummary {
  const chipsByTool = new Map<string, ReadChip>();
  let loadCount: number | undefined;
  let availableTrucks: number | undefined;
  let totalTrucks: number | undefined;

  for (const decision of decisions) {
    const output = parseToolOutput(decision.toolOutput);
    const toolName = decision.toolName ?? "";
    const chip = chipsByTool.get(toolName);

    if (chip) {
      chip.count += 1;
      chip.failed = chip.failed || !!output.error;
    } else {
      chipsByTool.set(toolName, {
        label: getToolLabel(decision.toolName),
        icon: getToolIcon(decision.toolName),
        count: 1,
        failed: !!output.error,
      });
    }

    loadCount ??= output.loads?.length;
    availableTrucks ??= output.availableTrucks;
    totalTrucks ??= output.totalTrucks;
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
