/**
 * Proves the wrapper contract that the whole PrimeNG -> spartan migration rests on, for the
 * numeric wrapper. `UiNumberField` implements ONLY `FormValueControl<number | null>`. It must
 * therefore work:
 *   1. under Signal Forms `[formField]`,
 *   2. under legacy Reactive Forms `formControlName` (Angular 22's automatic bridge),
 *   3. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and render
 *      validation errors — under BOTH form systems.
 *
 * The inner `p-inputnumber` is driven by a standalone NgModel and updates its model on blur
 * (`onInputBlur`), which is also the moment it emits `onBlur` -> `touch`. The `enter()` helper
 * below therefore simulates a real "type then leave the field" interaction.
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { form, FormField, required } from "@angular/forms/signals";
import { UiFormField } from "../form-field/form-field";
import { UiNumberField } from "./number-field";

/** Reactive Forms host: wrapper bound with formControlName, wrapped in ui-form-field chrome. */
@Component({
  selector: "ui-host-reactive-number",
  imports: [UiNumberField, UiFormField, ReactiveFormsModule],
  template: `
    <form [formGroup]="fg">
      <ui-form-field label="Rate" for="rate" [required]="true">
        <ui-number-field id="rate" formControlName="rate" />
      </ui-form-field>
    </form>
  `,
})
class HostReactiveNumber {
  readonly fg = new FormGroup({
    rate: new FormControl<number | null>(10, { validators: [Validators.required] }),
  });
  readonly field = viewChild.required(UiNumberField);
}

/** Signal Forms host: the SAME wrapper bound with [formField]. */
@Component({
  selector: "ui-host-signal-number",
  imports: [UiNumberField, UiFormField, FormField],
  template: `
    <ui-form-field label="Rate" for="rate" [required]="true">
      <ui-number-field id="rate" [formField]="f.rate" />
    </ui-form-field>
  `,
})
class HostSignalNumber {
  readonly model = signal<{ rate: number | null }>({ rate: 10 });
  readonly f = form(this.model, (p) => {
    required(p.rate);
  });
  readonly field = viewChild.required(UiNumberField);
}

async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

function input(fixture: ComponentFixture<unknown>): HTMLInputElement {
  return fixture.nativeElement.querySelector("input") as HTMLInputElement;
}

/**
 * Simulate typing `text` and leaving the field. PrimeNG's InputNumber commits and re-formats
 * the value in `onInputBlur`, so a native blur is the realistic way to push a value out.
 */
function enter(fixture: ComponentFixture<unknown>, text: string): void {
  const el = input(fixture);
  el.value = text;
  el.dispatchEvent(new Event("blur"));
}

describe("UiNumberField — a FormValueControl-only wrapper", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("renders the PrimeNG input-number and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostReactiveNumber);
    await settle(fixture);
    expect(input(fixture).value).toContain("10");
    expect(fixture.componentInstance.field().value()).toBe(10);
  });

  describe("under legacy Reactive Forms (formControlName)", () => {
    it("syncs control -> view", async () => {
      const fixture = TestBed.createComponent(HostReactiveNumber);
      await settle(fixture);

      fixture.componentInstance.fg.controls.rate.setValue(99);
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe(99);
      expect(input(fixture).value).toContain("99");
    });

    it("syncs view -> control (typing then blur)", async () => {
      const fixture = TestBed.createComponent(HostReactiveNumber);
      await settle(fixture);

      enter(fixture, "42");
      await settle(fixture);

      expect(fixture.componentInstance.fg.controls.rate.value).toBe(42);
    });

    it("propagates disabled state from the control", async () => {
      const fixture = TestBed.createComponent(HostReactiveNumber);
      await settle(fixture);

      fixture.componentInstance.fg.controls.rate.disable();
      await settle(fixture);

      expect(input(fixture).disabled).toBe(true);
    });

    it("ui-form-field renders the required error once touched", async () => {
      const fixture = TestBed.createComponent(HostReactiveNumber);
      await settle(fixture);

      enter(fixture, "");
      fixture.componentInstance.fg.controls.rate.markAsTouched();
      await settle(fixture);

      expect(fixture.componentInstance.fg.controls.rate.value).toBeNull();
      expect(fixture.nativeElement.textContent).toContain("This field is required.");
    });
  });

  describe("under Signal Forms ([formField])", () => {
    it("syncs field -> view", async () => {
      const fixture = TestBed.createComponent(HostSignalNumber);
      await settle(fixture);

      fixture.componentInstance.f.rate().value.set(99);
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe(99);
      expect(input(fixture).value).toContain("99");
    });

    it("syncs view -> field (typing then blur)", async () => {
      const fixture = TestBed.createComponent(HostSignalNumber);
      await settle(fixture);

      enter(fixture, "42");
      await settle(fixture);

      expect(fixture.componentInstance.model().rate).toBe(42);
    });

    it("ui-form-field renders the required error once touched — with NO transitional code", async () => {
      const fixture = TestBed.createComponent(HostSignalNumber);
      await settle(fixture);

      // Clearing then blurring: the blur both nulls the value and raises `touch`,
      // which Signal Forms uses to mark the field touched.
      enter(fixture, "");
      await settle(fixture);

      expect(fixture.componentInstance.model().rate).toBeNull();
      expect(fixture.componentInstance.f.rate().invalid()).toBe(true);
      expect(fixture.nativeElement.textContent).toContain("This field is required.");
    });
  });
});
