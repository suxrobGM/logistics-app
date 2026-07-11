/**
 * Proves the wrapper contract that the whole PrimeNG -> spartan migration rests on.
 *
 * `UiAutocompleteField` implements ONLY `FormValueControl`. It must therefore work:
 *   1. under Signal Forms `[formField]`,
 *   2. under legacy Reactive Forms `formControlName` (Angular 22's automatic bridge),
 *   3. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors — under BOTH form systems.
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
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { form, FormField, required } from "@angular/forms/signals";
import { UiFormField } from "../form-field/form-field";
import { UiAutocompleteField } from "./autocomplete-field";

interface Driver {
  fullName: string;
}

const ALICE: Driver = { fullName: "Alice" };
const BOB: Driver = { fullName: "Bob" };

/** Reactive Forms host: wrapper bound with formControlName, wrapped in ui-form-field chrome. */
@Component({
  selector: "ui-host-reactive-ac",
  imports: [UiAutocompleteField, UiFormField, ReactiveFormsModule],
  template: `
    <form [formGroup]="fg">
      <ui-form-field label="Driver" for="driver" [required]="true">
        <ui-autocomplete-field
          id="driver"
          formControlName="driver"
          optionLabel="fullName"
          [suggestions]="suggestions"
        />
      </ui-form-field>
    </form>
  `,
})
class HostReactiveAc {
  readonly suggestions: Driver[] = [ALICE, BOB];
  readonly fg = new FormGroup({
    driver: new FormControl<Driver | null>(ALICE, { validators: [Validators.required] }),
  });
  readonly field = viewChild.required(UiAutocompleteField);
}

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
  readonly suggestions: Driver[] = [ALICE, BOB];
  readonly model = signal<{ driver: Driver | null }>({ driver: ALICE });
  readonly f = form(this.model, (p) => {
    required(p.driver);
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
    const fixture = TestBed.createComponent(HostReactiveAc);
    await settle(fixture);
    expect(input(fixture)).toBeTruthy();
    expect(fixture.componentInstance.field().value()).toBe(ALICE);
  });

  describe("under legacy Reactive Forms (formControlName)", () => {
    it("syncs control -> view", async () => {
      const fixture = TestBed.createComponent(HostReactiveAc);
      await settle(fixture);

      fixture.componentInstance.fg.controls.driver.setValue(BOB);
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe(BOB);
    });

    it("syncs view -> control (option pick simulated via the value seam)", async () => {
      const fixture = TestBed.createComponent(HostReactiveAc);
      await settle(fixture);

      fixture.componentInstance.field().value.set(BOB);
      await settle(fixture);

      expect(fixture.componentInstance.fg.controls.driver.value).toBe(BOB);
    });

    it("propagates disabled state from the control", async () => {
      const fixture = TestBed.createComponent(HostReactiveAc);
      await settle(fixture);

      fixture.componentInstance.fg.controls.driver.disable();
      await settle(fixture);

      expect(fixture.componentInstance.field().disabled()).toBe(true);
    });
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
  });
});
