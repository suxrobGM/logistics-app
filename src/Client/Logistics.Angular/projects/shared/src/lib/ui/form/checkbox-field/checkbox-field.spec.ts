/**
 * Proves the wrapper contract that the whole PrimeNG -> spartan migration rests on, for a
 * boolean `FormValueControl`.
 *
 * `UiCheckboxField` implements ONLY `FormValueControl<boolean>`. It must therefore work:
 *   1. under Signal Forms `[formField]`,
 *   2. under legacy Reactive Forms `formControlName` (Angular 22's automatic bridge),
 *   3. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors — under BOTH form systems.
 *
 * Required-error note: a boolean field needs a "must be true" validator to be invalid while
 * `false`. Reactive Forms has a built-in one (`Validators.requiredTrue`). For Signal Forms,
 * `required()` treats `false` as empty (`isEmpty(false) === true` in @angular/forms/signals),
 * so `required(p.terms)` DOES report a `false` boolean as invalid — no custom validator needed.
 * Both paths surface the classic `required` error key, which `ui-form-field` renders as
 * "This field is required.".
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { form, FormField, required } from "@angular/forms/signals";
import { provideIcons } from "@ng-icons/core";
import { BASE_NG_ICONS } from "../../icons/lucide-icons";
import { UiFormField } from "../form-field/form-field";
import { UiCheckboxField } from "./checkbox-field";

/** Reactive Forms host: wrapper bound with formControlName, wrapped in ui-form-field chrome. */
@Component({
  selector: "ui-host-reactive-checkbox",
  imports: [UiCheckboxField, UiFormField, ReactiveFormsModule],
  template: `
    <form [formGroup]="fg">
      <ui-form-field label="Terms" for="terms" [required]="true">
        <ui-checkbox-field inputId="terms" label="I agree" formControlName="terms" />
      </ui-form-field>
    </form>
  `,
})
class HostReactiveCheckbox {
  readonly fg = new FormGroup({
    terms: new FormControl(false, { nonNullable: true, validators: [Validators.requiredTrue] }),
  });
  readonly field = viewChild.required(UiCheckboxField);
}

/** Signal Forms host: the SAME wrapper bound with [formField]. */
@Component({
  selector: "ui-host-signal-checkbox",
  imports: [UiCheckboxField, UiFormField, FormField],
  template: `
    <ui-form-field label="Terms" for="terms" [required]="true">
      <ui-checkbox-field inputId="terms" label="I agree" [formField]="f.terms" />
    </ui-form-field>
  `,
})
class HostSignalCheckbox {
  readonly model = signal({ terms: false });
  readonly f = form(this.model, (p) => {
    // `required()` treats `false` as empty, so this makes an unchecked box invalid.
    required(p.terms);
  });
  readonly field = viewChild.required(UiCheckboxField);
}

async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

function checkbox(fixture: ComponentFixture<unknown>): HTMLInputElement {
  return fixture.nativeElement.querySelector('input[type="checkbox"]') as HTMLInputElement;
}

/** Click the checkbox to toggle it (drives p-checkbox's inner NgModel). */
function toggle(fixture: ComponentFixture<unknown>): void {
  checkbox(fixture).click();
}

describe("UiCheckboxField — a FormValueControl<boolean>-only wrapper", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection(), provideIcons(BASE_NG_ICONS)],
    });
  });

  it("renders the PrimeNG checkbox and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostReactiveCheckbox);
    await settle(fixture);
    expect(checkbox(fixture).checked).toBe(false);

    fixture.componentInstance.fg.controls.terms.setValue(true);
    await settle(fixture);
    expect(checkbox(fixture).checked).toBe(true);
  });

  describe("under legacy Reactive Forms (formControlName)", () => {
    it("syncs control -> view", async () => {
      const fixture = TestBed.createComponent(HostReactiveCheckbox);
      await settle(fixture);

      fixture.componentInstance.fg.controls.terms.setValue(true);
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe(true);
      expect(checkbox(fixture).checked).toBe(true);
    });

    it("syncs view -> control (clicking)", async () => {
      const fixture = TestBed.createComponent(HostReactiveCheckbox);
      await settle(fixture);

      toggle(fixture);
      await settle(fixture);

      expect(fixture.componentInstance.fg.controls.terms.value).toBe(true);
    });

    it("propagates disabled state from the control", async () => {
      const fixture = TestBed.createComponent(HostReactiveCheckbox);
      await settle(fixture);

      fixture.componentInstance.fg.controls.terms.disable();
      await settle(fixture);

      expect(checkbox(fixture).disabled).toBe(true);
    });
  });

  describe("under Signal Forms ([formField])", () => {
    it("syncs field -> view", async () => {
      const fixture = TestBed.createComponent(HostSignalCheckbox);
      await settle(fixture);

      fixture.componentInstance.f.terms().value.set(true);
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe(true);
      expect(checkbox(fixture).checked).toBe(true);
    });

    it("syncs view -> field (clicking)", async () => {
      const fixture = TestBed.createComponent(HostSignalCheckbox);
      await settle(fixture);

      toggle(fixture);
      await settle(fixture);

      expect(fixture.componentInstance.model().terms).toBe(true);
    });

    it("ui-form-field renders the required error once touched — with NO transitional code", async () => {
      const fixture = TestBed.createComponent(HostSignalCheckbox);
      await settle(fixture);

      // value stays false; blur raises `touch`, which Signal Forms uses to mark it touched
      checkbox(fixture).dispatchEvent(new Event("blur"));
      await settle(fixture);

      expect(fixture.componentInstance.f.terms().invalid()).toBe(true);
      expect(fixture.nativeElement.textContent).toContain("This field is required.");
    });
  });
});
