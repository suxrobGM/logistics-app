import type { SelectOption } from "@/shared/models";

/**
 * Finds an option by value or label in a list of options.
 * @param options - The list of options to search in.
 * @param valueOrLabel - The value or label to search for.
 * @returns The option if found, otherwise null.
 */
export function findOption(options: SelectOption[], valueOrLabel: string): SelectOption | null {
  return (
    options.find((option) => option.label === valueOrLabel || option.value === valueOrLabel) ?? null
  );
}
