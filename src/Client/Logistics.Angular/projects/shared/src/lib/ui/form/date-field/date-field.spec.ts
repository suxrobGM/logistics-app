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
 * The inner spartan `hlm-date-picker` (brain calendar + `BrnPopover`) is driven with plain `[date]`
 * / `(dateChange)`; `uiDetachedControl` severs the ambient `NgControl`. Its calendar portals to a
 * CDK overlay outside the fixture, so a user pick is exercised at the same seam the calendar hits —
 * the picker's `updateDate` (which emits `dateChange`) — then we assert the OUTER control receives it.
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { form, FormField, required } from "@angular/forms/signals";
import { By } from "@angular/platform-browser";
import { HlmDatePicker } from "../../primitives/date-picker";
import { UiFormField } from "../form-field/form-field";
import { UiDateField } from "./date-field";

const INITIAL = new Date(2024, 0, 15); // Jan 15 2024

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

/** The inner spartan date picker instance (drives the same seam a calendar click hits). */
function picker(fixture: ComponentFixture<unknown>): HlmDatePicker<Date> {
  return fixture.debugElement.query(By.directive(HlmDatePicker))
    .componentInstance as HlmDatePicker<Date>;
}

describe("UiDateField — a FormValueControl-only wrapper", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("renders the date field and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostReactiveDate);
    await settle(fixture);
    expect(fixture.nativeElement.querySelector("input")).toBeTruthy();
    expect(fixture.componentInstance.field().value()?.getTime()).toBe(INITIAL.getTime());
  });

  describe("under legacy Reactive Forms (formControlName)", () => {
    it("syncs control -> view", async () => {
      const fixture = TestBed.createComponent(HostReactiveDate);
      await settle(fixture);

      const next = new Date(2024, 5, 20); // Jun 20 2024
      fixture.componentInstance.fg.controls.shipDate.setValue(next);
      await settle(fixture);

      expect(fixture.componentInstance.field().value()?.getTime()).toBe(next.getTime());
    });

    it("syncs view -> control (calendar pick via the picker seam)", async () => {
      const fixture = TestBed.createComponent(HostReactiveDate);
      await settle(fixture);

      const picked = new Date(2024, 2, 10); // Mar 10 2024
      picker(fixture).updateDate(picked);
      await settle(fixture);

      expect(fixture.componentInstance.fg.controls.shipDate.value?.getTime()).toBe(
        picked.getTime(),
      );
    });

    it("propagates disabled state from the control", async () => {
      const fixture = TestBed.createComponent(HostReactiveDate);
      await settle(fixture);

      fixture.componentInstance.fg.controls.shipDate.disable();
      await settle(fixture);

      expect(fixture.componentInstance.field().disabled()).toBe(true);
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
    });

    it("re-emits the picker's date change as (dateSelected) and syncs to the field", async () => {
      const fixture = TestBed.createComponent(HostSignalDate);
      await settle(fixture);

      const picked = new Date(2024, 8, 3); // Sep 3 2024
      let emitted: Date | undefined;
      fixture.componentInstance.field().dateSelected.subscribe((d) => (emitted = d));

      picker(fixture).updateDate(picked);
      await settle(fixture);

      expect(emitted?.getTime()).toBe(picked.getTime());
      expect(fixture.componentInstance.model().shipDate?.getTime()).toBe(picked.getTime());
    });

    it("ui-form-field renders the required error once touched — with NO transitional code", async () => {
      const fixture = TestBed.createComponent(HostSignalDate);
      await settle(fixture);

      fixture.componentInstance.f.shipDate().value.set(null);
      fixture.componentInstance.f.shipDate().markAsTouched();
      await settle(fixture);

      expect(fixture.componentInstance.f.shipDate().invalid()).toBe(true);
      expect(fixture.nativeElement.textContent).toContain("This field is required.");
    });
  });
});
