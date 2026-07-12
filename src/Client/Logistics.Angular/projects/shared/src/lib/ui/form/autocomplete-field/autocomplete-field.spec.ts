/**
 * Proves the wrapper contract that the whole `ui-*-field` layer rests on.
 *
 * `UiAutocompleteField` implements ONLY `FormValueControl` — no value-accessor glue of any kind. It must work:
 *   1. under Signal Forms `[formField]`,
 *   2. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors.
 *
 * The value is an object (`Driver`) resolved to a display label via `optionLabel`, exactly like
 * every real call site (drivers, employees, users).
 *
 * The inner spartan `hlm-autocomplete` (brain `BrnAutocomplete` + `BrnPopover`) portals its list to
 * a CDK overlay outside the fixture. jsdom cannot open that overlay or run the option-click path, so
 * view -> control is exercised at the same seam the overlay ultimately hits — the wrapper's `value`
 * model — then we assert the OUTER form control receives it. That is the real bridge path.
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { disabled, form, FormField, required } from "@angular/forms/signals";
import { UiFormField } from "../form-field/form-field";
import { UiAutocompleteField } from "./autocomplete-field";

interface Driver {
  fullName: string;
}

const ALICE: Driver = { fullName: "Alice" };
const BOB: Driver = { fullName: "Bob" };

/** Signal Forms host: the SAME wrapper bound with [formField]. */
@Component({
  selector: "ui-host-signal-ac",
  imports: [UiAutocompleteField, UiFormField, FormField],
  template: `
    <ui-form-field label="Driver" for="driver" [required]="true">
      <ui-autocomplete-field
        id="driver"
        [formField]="f.driver"
        optionLabel="fullName"
        [suggestions]="suggestions"
      />
    </ui-form-field>
  `,
})
class HostSignalAc {
  /** Flips the schema's disabled rule — proves the wrapper REACTS, not just reads once. */
  readonly lock = signal(false);
  readonly suggestions: Driver[] = [ALICE, BOB];
  readonly model = signal<{ driver: Driver | null }>({ driver: ALICE });
  readonly f = form(this.model, (p) => {
    required(p.driver, { message: "This field is required." });
    // Reactive disabled rule — the shape every real form uses:
    //   disabled(p.truckId, { when: () => this.mode() === "edit" })
    disabled(p.driver, { when: () => this.lock() });
  });
  readonly field = viewChild.required(UiAutocompleteField);
}

async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

function input(fixture: ComponentFixture<unknown>): HTMLInputElement {
  return fixture.nativeElement.querySelector("input") as HTMLInputElement;
}

describe("UiAutocompleteField — a FormValueControl-only wrapper", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("renders the autocomplete input and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostSignalAc);
    await settle(fixture);
    expect(input(fixture)).toBeTruthy();
    expect(fixture.componentInstance.field().value()).toBe(ALICE);
  });
  describe("under Signal Forms ([formField])", () => {
    it("syncs field -> view", async () => {
      const fixture = TestBed.createComponent(HostSignalAc);
      await settle(fixture);

      fixture.componentInstance.f.driver().value.set(BOB);
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe(BOB);
    });

    it("syncs view -> field (option pick simulated via the value seam)", async () => {
      const fixture = TestBed.createComponent(HostSignalAc);
      await settle(fixture);

      fixture.componentInstance.field().value.set(BOB);
      await settle(fixture);

      expect(fixture.componentInstance.model().driver).toBe(BOB);
    });

    it("ui-form-field renders the required error once touched — with NO transitional code", async () => {
      const fixture = TestBed.createComponent(HostSignalAc);
      await settle(fixture);

      fixture.componentInstance.f.driver().value.set(null);
      fixture.componentInstance.f.driver().markAsTouched();
      await settle(fixture);

      expect(fixture.componentInstance.f.driver().invalid()).toBe(true);
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
      const fixture = TestBed.createComponent(HostSignalAc);
      await settle(fixture);

      expect(fixture.componentInstance.f.driver().disabled()).toBe(false);
      expect(fixture.componentInstance.field().disabled()).toBe(false);
      expect(input(fixture).disabled).toBe(false);

      fixture.componentInstance.lock.set(true);
      await settle(fixture);

      expect(fixture.componentInstance.f.driver().disabled()).toBe(true);
      expect(fixture.componentInstance.field().disabled()).toBe(true);
      expect(input(fixture).disabled).toBe(true);
    });
  });
});
