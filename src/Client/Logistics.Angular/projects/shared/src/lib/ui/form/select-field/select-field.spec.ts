/**
 * Proves the wrapper contract that the whole `ui-*-field` layer rests on.
 *
 * `UiSelectField` implements ONLY `FormValueControl` — no value-accessor glue of any kind. It must work:
 *   1. under Signal Forms `[formField]`,
 *   2. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors.
 *
 * The inner spartan `hlm-select` (brain `BrnSelect` + `BrnPopover`) is driven with plain
 * `[value]` / `(valueChange)`. `uiDetachedControl` severs the ambient `NgControl` so brain's
 * `BrnFieldControl` does not crash on Signal Forms' `InteropNgControl`. The user-interaction
 * direction is exercised through the `BrnSelect` value model (what a click on an option drives).
 *
 * If any of these break, every `ui-*-field` wrapper breaks with them.
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { disabled, form, FormField, required } from "@angular/forms/signals";
import { By } from "@angular/platform-browser";
import { BrnSelect } from "@spartan-ng/brain/select";
import { UiFormField } from "../form-field/form-field";
import { UiSelectField } from "./select-field";

const OPTIONS = [
  { label: "Red", value: "red" },
  { label: "Green", value: "green" },
];

/** Signal Forms host: the SAME wrapper bound with [formField]. */
@Component({
  selector: "ui-host-signal-select",
  imports: [UiSelectField, UiFormField, FormField],
  template: `
    <ui-form-field label="Color" for="color" [required]="true">
      <ui-select-field
        id="color"
        [formField]="f.color"
        [options]="options"
        optionLabel="label"
        optionValue="value"
      />
    </ui-form-field>
  `,
})
class HostSignalSelect {
  /** Flips the schema's disabled rule — proves the wrapper REACTS, not just reads once. */
  readonly lock = signal(false);
  readonly options = OPTIONS;
  readonly model = signal<{ color: string | null }>({ color: null });
  readonly f = form(this.model, (p) => {
    required(p.color, { message: "This field is required." });
    // Reactive disabled rule — the shape every real form uses:
    //   disabled(p.truckId, { when: () => this.mode() === "edit" })
    disabled(p.color, { when: () => this.lock() });
  });
  readonly field = viewChild.required(UiSelectField);
}

/**
 * Same host, but SEEDED with a value — so "the initial model value is rendered on first paint" is
 * still covered. (It used to be covered by the Reactive-Forms host, whose FormControl was seeded
 * with "green"; that host is gone, and the plain signal host starts null.)
 */
@Component({
  selector: "ui-host-signal-select-seeded",
  imports: [UiSelectField, UiFormField, FormField],
  template: `
    <ui-form-field label="Color" for="color" [required]="true">
      <ui-select-field
        id="color"
        [formField]="f.color"
        [options]="options"
        optionLabel="label"
        optionValue="value"
      />
    </ui-form-field>
  `,
})
class HostSignalSelectSeeded {
  readonly options = OPTIONS;
  readonly model = signal<{ color: string | null }>({ color: "green" });
  readonly f = form(this.model, (p) => {
    required(p.color, { message: "This field is required." });
  });
  readonly field = viewChild.required(UiSelectField);
}

async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

/** The inner brain BrnSelect instance (host-directed by hlm-select). */
function brnSelect(fixture: ComponentFixture<unknown>): BrnSelect<string> {
  return fixture.debugElement
    .query(By.css("hlm-select"))
    .injector.get(BrnSelect) as BrnSelect<string>;
}

/** Text rendered inside the trigger button (the selected value's label). */
function triggerText(fixture: ComponentFixture<unknown>): string {
  return (fixture.nativeElement.querySelector("hlm-select-trigger")?.textContent ?? "").trim();
}

/** Simulate a user picking an option — a click drives the BrnSelect value model. */
function pick(fixture: ComponentFixture<unknown>, value: string | null): void {
  brnSelect(fixture).value.set(value);
}

/** The trigger the user actually clicks — spartan renders it as a real <button>. */
function triggerButton(fixture: ComponentFixture<unknown>): HTMLButtonElement {
  return fixture.nativeElement.querySelector("hlm-select-trigger button") as HTMLButtonElement;
}

describe("UiSelectField — a FormValueControl-only wrapper", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("renders the select and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostSignalSelectSeeded);
    await settle(fixture);
    expect(fixture.componentInstance.field().value()).toBe("green");
    expect(triggerText(fixture)).toContain("Green");
  });

  describe("under Signal Forms ([formField])", () => {
    it("syncs field -> view", async () => {
      const fixture = TestBed.createComponent(HostSignalSelect);
      await settle(fixture);

      fixture.componentInstance.f.color().value.set("green");
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe("green");
      expect(triggerText(fixture)).toContain("Green");
    });

    it("syncs view -> field (selecting an option)", async () => {
      const fixture = TestBed.createComponent(HostSignalSelect);
      await settle(fixture);

      pick(fixture, "red");
      await settle(fixture);

      expect(fixture.componentInstance.model().color).toBe("red");
    });

    it("ui-form-field renders the required error once touched — with NO transitional code", async () => {
      const fixture = TestBed.createComponent(HostSignalSelect);
      await settle(fixture);

      // The wrapper marks the field touched when the panel closes; drive that state directly.
      fixture.componentInstance.f.color().markAsTouched();
      await settle(fixture);

      expect(fixture.componentInstance.f.color().invalid()).toBe(true);
      expect(fixture.nativeElement.textContent).toContain("This field is required.");
    });
    /**
     * REGRESSION GUARD. `disabled` is a first-class part of the FormValueControl contract and is
     * load-bearing in production (`disabled(p.salary, { when: ... })` in employee-edit,
     * `disabled(p.truckId, ...)` in the expense forms, tax-rates, timesheets, ...).
     *
     * It used to be covered ONLY by the legacy Reactive-Forms host ("propagates disabled state from
     * the control"). That host was deleted, and the assertion went with it — leaving the
     * whole `disabled` dimension of all 10 wrappers untested. This is its Signal Forms twin.
     */
    it("propagates the schema's disabled rule to the control — and reacts when it flips", async () => {
      const fixture = TestBed.createComponent(HostSignalSelect);
      await settle(fixture);

      expect(fixture.componentInstance.f.color().disabled()).toBe(false);
      expect(fixture.componentInstance.field().disabled()).toBe(false);
      expect(triggerButton(fixture).disabled).toBe(false);

      fixture.componentInstance.lock.set(true);
      await settle(fixture);

      expect(fixture.componentInstance.f.color().disabled()).toBe(true);
      expect(fixture.componentInstance.field().disabled()).toBe(true);
      expect(triggerButton(fixture).disabled).toBe(true);
    });
  });
});
