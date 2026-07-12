/**
 * `ui-button`'s contract. Everything pinned here fails SILENTLY — build, lint and every other spec
 * stay green while the button is broken:
 *
 *   1. the intent x appearance matrix — a hole emits no CSS and no warning, so the button just
 *      renders unstyled;
 *   2. `type` defaults to "button" and survives being set to "submit" (a dropped `submit` is a save
 *      button that does nothing), proven against a real <form>;
 *   3. `loading` disables AND swaps the glyph for a spinner — miss the disable and the button
 *      double-submits, miss the swap and it looks idle while it works;
 *   4. icon-only goes square and is named only by `ariaLabel`;
 *   5. `buttonClass` lands on the INNER button — putting it on the host looks right in review and
 *      breaks every call site's layout;
 *   6. `(click)` bubbles — there is no output, so every call site's `(click)` rests on it.
 */
import { Component, provideZonelessChangeDetection, signal } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { UiButton } from "./button";
import {
  INTENT_APPEARANCE,
  uiButtonClass,
  type UiButtonAppearance,
  type UiButtonIntent,
  type UiButtonSize,
} from "./button-variants";

const INTENTS: readonly UiButtonIntent[] = [
  "primary",
  "secondary",
  "success",
  "info",
  "warn",
  "danger",
  "help",
  "contrast",
];

const APPEARANCES: readonly UiButtonAppearance[] = ["solid", "outlined", "text", "link"];

/** The default cell, for the class-level assertions that need no fixture. */
const base = {
  intent: "primary",
  appearance: "solid",
  size: "md",
  iconOnly: false,
  rounded: false,
  block: false,
} as const;

@Component({
  selector: "ui-host-button",
  imports: [UiButton],
  template: `
    <ui-button
      [label]="label()"
      [icon]="icon()"
      [intent]="intent()"
      [appearance]="appearance()"
      [size]="size()"
      [type]="type()"
      [loading]="loading()"
      [disabled]="disabled()"
      [rounded]="rounded()"
      [ariaLabel]="ariaLabel()"
      [buttonClass]="buttonClass()"
      (click)="clicks.set(clicks() + 1)"
    />
  `,
})
class HostButton {
  readonly label = signal<string | undefined>("Save");
  readonly icon = signal<"check" | "trash" | undefined>(undefined);
  readonly intent = signal<UiButtonIntent>("primary");
  readonly appearance = signal<UiButtonAppearance>("solid");
  readonly size = signal<UiButtonSize>("md");
  readonly type = signal<"button" | "submit" | "reset">("button");
  readonly loading = signal(false);
  readonly disabled = signal(false);
  readonly rounded = signal(false);
  readonly ariaLabel = signal<string | undefined>(undefined);
  readonly buttonClass = signal("");
  readonly clicks = signal(0);
}

/** A real <form>, because "does type=submit still submit" is the question that matters. */
@Component({
  selector: "ui-host-form",
  imports: [UiButton],
  template: `
    <form (submit)="submits.set(submits() + 1); $event.preventDefault()">
      <ui-button label="Save" type="submit" />
      <ui-button label="Cancel" />
    </form>
  `,
})
class HostForm {
  readonly submits = signal(0);
}

async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

function host(fixture: ComponentFixture<unknown>): HTMLElement {
  return fixture.nativeElement.querySelector("ui-button") as HTMLElement;
}

function button(fixture: ComponentFixture<unknown>): HTMLButtonElement {
  return fixture.nativeElement.querySelector("button") as HTMLButtonElement;
}

async function render(): Promise<ComponentFixture<HostButton>> {
  const fixture = TestBed.createComponent(HostButton);
  await settle(fixture);
  return fixture;
}

describe("ui-button", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()],
    });
  });

  describe("the 8 x 4 intent/appearance matrix", () => {
    it("has all 32 cells, every one of them non-empty", () => {
      const cells = INTENTS.flatMap((intent) =>
        APPEARANCES.map((appearance) => ({ intent, appearance })),
      );
      expect(cells).toHaveLength(32);

      for (const { intent, appearance } of cells) {
        const cell = INTENT_APPEARANCE[intent][appearance];
        expect(cell, `${intent}/${appearance} is empty — it would render unstyled`).toBeTruthy();
        expect(cell.trim().length).toBeGreaterThan(0);
      }
    });

    it("gives every intent its own colours — no intent silently collapses into another", () => {
      for (const appearance of APPEARANCES) {
        const rendered = INTENTS.map((intent) => INTENT_APPEARANCE[intent][appearance]);
        expect(new Set(rendered).size, `two intents share one ${appearance} cell`).toBe(
          INTENTS.length,
        );
      }
    });

    it("keeps danger a SOLID red fill and success green — these are money-moving actions", () => {
      // Full opacity, not a tint. Asserted against the split class list rather than the raw string,
      // because the cell also carries a `hover:` class with the same colour name.
      expect(INTENT_APPEARANCE.danger.solid.split(" ")).toContain("bg-danger");
      expect(INTENT_APPEARANCE.success.solid.split(" ")).toContain("bg-success");
      expect(INTENT_APPEARANCE.success.solid).not.toContain("bg-primary");
    });

    it("resolves every cell to real classes on a real button", async () => {
      const fixture = await render();

      for (const intent of INTENTS) {
        for (const appearance of APPEARANCES) {
          fixture.componentInstance.intent.set(intent);
          fixture.componentInstance.appearance.set(appearance);
          await settle(fixture);

          const className = button(fixture).className;
          expect(className.length, `${intent}/${appearance} rendered no classes`).toBeGreaterThan(
            0,
          );

          // Every class the cell declares must survive twMerge and reach the element.
          for (const cls of INTENT_APPEARANCE[intent][appearance].split(" ")) {
            expect(className, `${intent}/${appearance} lost "${cls}"`).toContain(cls);
          }
        }
      }
    });

    it("routes the primary solid fill through the --ui-btn-* variables TMS overrides", () => {
      // If this stops being variable-driven, the TMS gradient dies and nothing else notices.
      expect(INTENT_APPEARANCE.primary.solid).toContain("var(--ui-btn-primary-bg)");
      expect(INTENT_APPEARANCE.primary.solid).toContain("var(--ui-btn-primary-image)");
      expect(INTENT_APPEARANCE.primary.solid).toContain("var(--ui-btn-primary-weight)");
      // ...and only for `solid` — the gradient must not reach outlined/text/link.
      for (const appearance of ["outlined", "text", "link"] as const) {
        expect(INTENT_APPEARANCE.primary[appearance]).not.toContain("--ui-btn-primary");
      }
    });
  });

  describe("type", () => {
    it('defaults to "button"', async () => {
      const fixture = await render();
      expect(button(fixture).type).toBe("button");
    });

    it('carries type="submit" through to the native button', async () => {
      const fixture = await render();
      fixture.componentInstance.type.set("submit");
      await settle(fixture);
      expect(button(fixture).type).toBe("submit");
    });

    it("actually submits the surrounding form — and the default button does not", async () => {
      const fixture = TestBed.createComponent(HostForm);
      await settle(fixture);

      const [submitBtn, cancelBtn] = Array.from(
        fixture.nativeElement.querySelectorAll("button"),
      ) as HTMLButtonElement[];

      cancelBtn.click();
      await settle(fixture);
      expect(fixture.componentInstance.submits()).toBe(0);

      submitBtn.click();
      await settle(fixture);
      expect(fixture.componentInstance.submits()).toBe(1);
    });
  });

  describe("loading", () => {
    it("disables the button and reports aria-busy", async () => {
      const fixture = await render();
      fixture.componentInstance.loading.set(true);
      await settle(fixture);

      expect(button(fixture).disabled).toBe(true);
      expect(button(fixture).getAttribute("aria-busy")).toBe("true");
      expect(host(fixture).className).toContain("pointer-events-none");
    });

    it("swaps the icon for a spinning spinner, and renders only one glyph", async () => {
      const fixture = await render();
      fixture.componentInstance.icon.set("check");
      fixture.componentInstance.loading.set(true);
      await settle(fixture);

      const icons = fixture.nativeElement.querySelectorAll("ng-icon");
      expect(icons).toHaveLength(1);
      expect((icons[0] as HTMLElement).className).toContain("animate-spin");
    });

    it("sizes the spinner from the button — it emits no text-* class of its own", async () => {
      const fixture = await render();
      fixture.componentInstance.loading.set(true);
      await settle(fixture);

      // The predicate the button's own `[&_ng-icon:not([class*='text-'])]` rule matches on.
      const icon = fixture.nativeElement.querySelector("ng-icon") as HTMLElement;
      expect(icon.matches("ng-icon:not([class*='text-'])")).toBe(true);
    });

    it("restores the icon when loading clears", async () => {
      const fixture = await render();
      fixture.componentInstance.icon.set("check");
      fixture.componentInstance.loading.set(true);
      await settle(fixture);
      fixture.componentInstance.loading.set(false);
      await settle(fixture);

      const icon = fixture.nativeElement.querySelector("ng-icon") as HTMLElement;
      expect(icon.className).not.toContain("animate-spin");
      expect(button(fixture).disabled).toBe(false);
    });
  });

  describe("icon-only", () => {
    it("collapses to a square and drops the horizontal padding", async () => {
      const fixture = await render();
      fixture.componentInstance.label.set(undefined);
      fixture.componentInstance.icon.set("trash");
      await settle(fixture);

      const className = button(fixture).className;
      expect(className).toContain("size-9");
      expect(className).not.toContain("px-4");
    });

    it("takes its accessible name from ariaLabel — there is no text to name it", async () => {
      const fixture = await render();
      fixture.componentInstance.label.set(undefined);
      fixture.componentInstance.icon.set("trash");
      fixture.componentInstance.ariaLabel.set("Delete load");
      await settle(fixture);

      expect(button(fixture).getAttribute("aria-label")).toBe("Delete load");
      expect(button(fixture).textContent?.trim()).toBe("");
    });

    it("stays rectangular as soon as it has a label", async () => {
      const fixture = await render();
      fixture.componentInstance.icon.set("check");
      await settle(fixture);

      const className = button(fixture).className;
      expect(className).not.toContain("size-9");
      expect(className).toContain("px-4");
    });

    it("goes square while loading with no label — the spinner is the glyph", () => {
      expect(uiButtonClass({ ...base, iconOnly: true })).toContain("size-9");
    });
  });

  describe("buttonClass", () => {
    it("lands on the INNER button, not on the host", async () => {
      const fixture = await render();
      fixture.componentInstance.buttonClass.set("mt-2 tracking-wide");
      await settle(fixture);

      expect(button(fixture).className).toContain("mt-2");
      expect(button(fixture).className).toContain("tracking-wide");
      expect(host(fixture).className).not.toContain("mt-2");
    });

    it("wins over the variant it collides with — twMerge, last one in", () => {
      const withOverride = uiButtonClass({ ...base, extra: "h-20" });
      expect(withOverride).toContain("h-20");
      expect(withOverride).not.toContain("h-9");
    });
  });

  describe("host", () => {
    it("bubbles (click) from the inner button — the component declares no output", async () => {
      const fixture = await render();
      button(fixture).click();
      await settle(fixture);

      expect(fixture.componentInstance.clicks()).toBe(1);
    });

    it("swallows clicks when disabled, on the host as well as the button", async () => {
      const fixture = await render();
      fixture.componentInstance.disabled.set(true);
      await settle(fixture);

      expect(button(fixture).disabled).toBe(true);
      // pointer-events-none on the HOST is what stops a `routerLink` on the host from navigating.
      expect(host(fixture).className).toContain("pointer-events-none");
    });

    it("stays inline-flex, so it keeps its place in a flex row", async () => {
      const fixture = await render();
      expect(host(fixture).className).toContain("inline-flex");
    });
  });

  describe("size and shape", () => {
    it.each([
      ["sm", "h-8"],
      ["md", "h-9"],
      ["lg", "h-10"],
    ] as const)("%s renders %s", (size, height) => {
      expect(uiButtonClass({ ...base, size })).toContain(height);
    });

    it("rounded swaps rounded-md for rounded-full", () => {
      expect(uiButtonClass({ ...base, rounded: true })).toContain("rounded-full");
      expect(uiButtonClass({ ...base, rounded: true })).not.toContain("rounded-md");
    });

    it("block stretches the inner button too, not just the host", () => {
      expect(uiButtonClass({ ...base, block: true })).toContain("w-full");
    });
  });
});
