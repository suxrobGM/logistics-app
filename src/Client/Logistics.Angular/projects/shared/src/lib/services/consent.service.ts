import { computed, inject, Injectable, signal } from "@angular/core";
import type { ConsentType } from "../api/generated/models/consent-type";
import { StorageService } from "./storage.service";

const STORAGE_KEY = "cookie-consent";
const STORAGE_VERSION = 1;

/** Persisted shape of the cookie-consent decision in localStorage. */
export interface ConsentChoices {
  strictly_necessary: true; // always on, narrowed for type clarity
  functional: boolean;
  analytics: boolean;
  marketing: boolean;
}

interface PersistedConsent {
  version: number;
  choices: ConsentChoices;
  timestamp: string;
}

const NEUTRAL_CHOICES: ConsentChoices = {
  strictly_necessary: true,
  functional: false,
  analytics: false,
  marketing: false,
};

const ACCEPT_ALL_CHOICES: ConsentChoices = {
  strictly_necessary: true,
  functional: true,
  analytics: true,
  marketing: true,
};

/**
 * Tracks the user's cookie / processing-consent decisions in localStorage.
 *
 * Analytics, marketing, and functional SDK initializers must call
 * {@link hasConsent} before loading any third-party scripts.
 */
@Injectable({ providedIn: "root" })
export class ConsentService {
  private readonly storage = inject(StorageService);

  private readonly state = signal<PersistedConsent | null>(this.load());

  /** True after the visitor has explicitly accepted or rejected something. */
  public readonly hasDecided = computed(() => this.state() !== null);

  /** Per-category consent state — drives `<ui-cookie-banner>` and analytics gating. */
  public readonly choices = computed<ConsentChoices>(
    () => this.state()?.choices ?? NEUTRAL_CHOICES,
  );

  /**
   * Reactive check for callers like the analytics initializer:
   * `effect(() => { if (consent.hasConsent('analytics')()) initGa(); });`
   */
  public hasConsent(category: ConsentType): () => boolean {
    return computed(() => this.choices()[category as keyof ConsentChoices] === true);
  }

  /** Accept everything (banner "Accept all"). */
  public acceptAll(): void {
    this.persist(this.applyDoNotTrack(ACCEPT_ALL_CHOICES));
  }

  /** Decline non-essential cookies (banner "Reject all"). */
  public rejectAll(): void {
    this.persist({ ...NEUTRAL_CHOICES });
  }

  /** Custom selection from the banner's "Customize" view. */
  public saveChoices(partial: Partial<Omit<ConsentChoices, "strictly_necessary">>): void {
    const current = this.choices();
    const merged: ConsentChoices = {
      strictly_necessary: true,
      functional: partial.functional ?? current.functional,
      analytics: partial.analytics ?? current.analytics,
      marketing: partial.marketing ?? current.marketing,
    };
    this.persist(this.applyDoNotTrack(merged));
  }

  private load(): PersistedConsent | null {
    const raw = this.storage.get<PersistedConsent>(STORAGE_KEY);
    if (raw === null || raw.version !== STORAGE_VERSION) return null;
    return raw;
  }

  private persist(choices: ConsentChoices): void {
    const next: PersistedConsent = {
      version: STORAGE_VERSION,
      choices,
      timestamp: new Date().toISOString(),
    };
    this.storage.set(STORAGE_KEY, next);
    this.state.set(next);
  }

  private applyDoNotTrack(choices: ConsentChoices): ConsentChoices {
    if (typeof navigator !== "undefined" && navigator.doNotTrack === "1") {
      return { ...choices, analytics: false, marketing: false };
    }
    return choices;
  }
}
