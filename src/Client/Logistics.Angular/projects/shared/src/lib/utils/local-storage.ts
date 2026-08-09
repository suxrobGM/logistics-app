/**
 * Reads a UI preference out of localStorage. Never throws: private-mode and disabled-storage both
 * make `localStorage` access itself raise, and a preference is never worth failing a render over.
 *
 * Free functions rather than a service because two call sites read their value in a module-level
 * initial-state constant, where injection is not available.
 */
export function readStoredBoolean(key: string): boolean {
  try {
    return localStorage.getItem(key) === "true";
  } catch {
    return false;
  }
}

export function readStoredNumber(key: string, fallback: number): number {
  try {
    const stored = Number(localStorage.getItem(key));
    return Number.isFinite(stored) ? stored : fallback;
  } catch {
    return fallback;
  }
}

export function persistValue(key: string, value: string | number | boolean): void {
  try {
    localStorage.setItem(key, String(value));
  } catch {
    // A preference that cannot be saved is not worth breaking the interaction that set it.
  }
}
