/** Store-level action a slash command maps to; the drawer translates it into a store call. */
export type CopilotCommandAction = "startNewChat" | "showHistory";

export interface CopilotCommand {
  /** Command name without the leading slash. */
  readonly name: string;
  readonly description: string;
  readonly action: CopilotCommandAction;
}

/**
 * Client-only slash commands. `/clear` maps to startNewChat - conversations are created lazily on
 * first send, so a fresh chat already carries no prior context to the LLM.
 */
export const COPILOT_COMMANDS: readonly CopilotCommand[] = [
  {
    name: "clear",
    description: "Start a fresh chat (drops prior context)",
    action: "startNewChat",
  },
  { name: "history", description: "Browse past conversations", action: "showHistory" },
];
