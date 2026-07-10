/**
 * Proves the wrapper contract that the whole PrimeNG -> spartan migration rests on, for the
 * date-picker wrapper.
 *
 * `UiDateField` implements ONLY `FormValueControl<Date | null>`. It must therefore work:
 *   1. under Signal Forms `[formField]`,
 *   2. under legacy Reactive Forms `formControlName` (Angular 22's automatic bridge),
 *   3. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors — under BOTH form systems.
 *
 * The inner `<p-datepicker>` is driven with a standalone `ngModel` (it is a ControlValueAccessor);
 * it never carries `formControlName`/`[formField]`, so it does not collide with Signal Forms'
 * `pattern` state input. See `forms/signal-forms-compat-probe.spec.ts`.
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { form, FormField, required } from "@angular/forms/signals";
import { By } from "@angular/platform-browser";
import { DatePicker } from "primeng/datepicker";
import { UiFormField } from "../form-field/form-field";
import { UiDateField } from "./date-field";

const INITIAL = new Date(2024, 0, 15); // Jan 15 2024 -> "01/15/2024" with dateFormat "mm/dd/yy"

/** Reactive Forms host: wrapper bound with formControlName, wrapped in ui-form-field chrome. */
@Component({
  selector: "ui-host-reactive-date",
  imports: [UiDateField, UiFormField, ReactiveFormsModule],
  template: `
    <form [formGroup]="fg">
      <ui-form-field label="Ship date" for="shipDate" [required]="true">
        <ui-date-field id="shipDate" formControlName="shipDate" />
      </ui-form-field>
    </form>
  `,
})
class HostReactiveDate {
  readonly fg = new FormGroup({
    shipDate: new FormControl<Date | null>(INITIAL, { validators: [Validators.required] }),
  });
  readonly field = viewChild.required(UiDateField);
}

/** Signal Forms host: the SAME wrapper bound with [formField]. */
@Component({
  selector: "ui-host-signal-date",
  imports: [UiDateField, UiFormField, FormField],
  template: `
    <ui-form-field label="Ship date" for="shipDate" [required]="true">
      <ui-date-field id="shipDate" [formField]="f.shipDate" />
    </ui-form-field>
  `,
})
class HostSignalDate {
  readonly model = signal<{ shipDate: Date | null }>({ shipDate: INITIAL });
  readonly f = form(this.model, (p) => {
    required(p.shipDate);
  });
  readonly field = viewChild.required(UiDateField);
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
 * Simulate a user typing a formatted date into the datepicker's input.
 * `p-datepicker.onUserInput` early-returns unless a keydown was seen first (`isKeydown`),
 * so a keydown must precede the input event.
 */
function typeDate(fixture: ComponentFixture<unknown>, text: string): void {
  const el = input(fixture);
  el.dispatchEvent(new KeyboardEvent("keydown", { bubbles: true }));
  el.value = text;
  el.dispatchEvent(new Event("input", { bubbles: true }));
}

describe("UiDateField — a FormValueControl-only wrapper", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("renders the PrimeNG datepicker and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostReactiveDate);
    await settle(fixture);
    expect(input(fixture).value).toBe("01/15/2024");
  });

  describe("under legacy Reactive Forms (formControlName)", () => {
    it("syncs control -> view", async () => {
      const fixture = TestBed.createComponent(HostReactiveDate);
      await settle(fixture);

      const next = new Date(2024, 5, 20); // Jun 20 2024
      fixture.componentInstance.fg.controls.shipDate.setValue(next);
      await settle(fixture);

      expect(fixture.componentInstance.field().value()?.getTime()).toBe(next.getTime());
      expect(input(fixture).value).toBe("06/20/2024");
    });

    it("syncs view -> control (typing)", async () => {
      const fixture = TestBed.createComponent(HostReactiveDate);
      await settle(fixture);

      typeDate(fixture, "03/10/2024");
      await settle(fixture);

      const value = fixture.componentInstance.fg.controls.shipDate.value;
      expect(value).toBeInstanceOf(Date);
      expect(value?.getFullYear()).toBe(2024);
      expect(value?.getMonth()).toBe(2); // March
      expect(value?.getDate()).toBe(10);
    });

    it("propagates disabled state from the control", async () => {
      const fixture = TestBed.createComponent(HostReactiveDate);
      await settle(fixture);

      fixture.componentInstance.fg.controls.shipDate.disable();
      await settle(fixture);

      expect(input(fixture).disabled).toBe(true);
    });

    it("ui-form-field renders the required error once touched", async () => {
      const fixture = TestBed.createComponent(HostReactiveDate);
      await settle(fixture);

      fixture.componentInstance.fg.controls.shipDate.setValue(null);
      fixture.componentInstance.fg.controls.shipDate.markAsTouched();
      await settle(fixture);

      expect(fixture.nativeElement.textContent).toContain("This field is required.");
    });
  });

  describe("under Signal Forms ([formField])", () => {
    it("syncs field -> view", async () => {
      const fixture = TestBed.createComponent(HostSignalDate);
      await settle(fixture);

      const next = new Date(2024, 7, 5); // Aug 5 2024
      fixture.componentInstance.f.shipDate().value.set(next);
      await settle(fixture);

      expect(fixture.componentInstance.field().value()?.getTime()).toBe(next.getTime());
      expect(input(fixture).value).toBe("08/05/2024");
    });

    it("syncs view -> field (typing)", async () => {
      const fixture = TestBed.createComponent(HostSignalDate);
      await settle(fixture);

      typeDate(fixture, "04/12/2024");
      await settle(fixture);

      const value = fixture.componentInstance.model().shipDate;
      expect(value).toBeInstanceOf(Date);
      expect(value?.getFullYear()).toBe(2024);
      expect(value?.getMonth()).toBe(3); // April
      expect(value?.getDate()).toBe(12);
    });

    it("re-emits the inner DatePicker's (onSelect) as (dateSelected) with the picked Date", async () => {
      const fixture = TestBed.createComponent(HostSignalDate);
      await settle(fixture);

      const picked = new Date(2024, 8, 3); // Sep 3 2024
      let emitted: Date | undefined;
      fixture.componentInstance.field().dateSelected.subscribe((d) => (emitted = d));

      // Emit from the real inner PrimeNG DatePicker instance — its `onSelect` carries a Date
      // (verified against primeng-datepicker.mjs: `this.onSelect.emit(date)`).
      const picker = fixture.debugElement.query(By.directive(DatePicker))
        .componentInstance as DatePicker;
      picker.onSelect.emit(picked);
      await settle(fixture);

      expect(emitted).toBe(picked);
    });

    it("ui-form-field renders the required error once touched — with NO transitional code", async () => {
      const fixture = TestBed.createComponent(HostSignalDate);
      await settle(fixture);

      // Clear to an invalid (null) value, then blur — blur raises `touch`, which Signal Forms
      // uses to mark the field touched.
      fixture.componentInstance.f.shipDate().value.set(null);
      input(fixture).dispatchEvent(new Event("blur"));
      await settle(fixture);

      expect(fixture.componentInstance.f.shipDate().invalid()).toBe(true);
      expect(fixture.nativeElement.textContent).toContain("This field is required.");
    });
  });
});
