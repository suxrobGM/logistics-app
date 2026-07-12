/**
 * Proves the wrapper contract that the whole `ui-*-field` layer rests on, for a boolean
 * `FormValueControl`.
 *
 * `UiCheckboxField` implements ONLY `FormValueControl<boolean>`. It must therefore work:
 *   1. under Signal Forms `[formField]`,
 *   2. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors.
 *
 * Required-error note: a boolean field needs a "must be true" validator to be invalid while
 * `false`. Reactive Forms has a built-in one (`Validators.requiredTrue`). For Signal Forms,
 * `required()` treats `false` as empty (`isEmpty(false) === true` in @angular/forms/signals),
 * so `required(p.terms)` DOES report a `false` boolean as invalid — no custom validator needed.
 * The validator supplies the message; `ui-form-field` renders it verbatim (it no longer maps error
 * `kind`s to copy), so this fixture states "This field is required." explicitly.
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { disabled, form, FormField, required } from "@angular/forms/signals";
import { UiFormField } from "../form-field/form-field";
import { UiCheckboxField } from "./checkbox-field";

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
  /** Flips the schema's disabled rule — proves the wrapper REACTS, not just reads once. */
  readonly lock = signal(false);
  readonly model = signal({ terms: false });
  readonly f = form(this.model, (p) => {
    // `required()` treats `false` as empty, so this makes an unchecked box invalid.
    required(p.terms, { message: "This field is required." });
    // Reactive disabled rule — the shape every real form uses:
    //   disabled(p.truckId, { when: () => this.mode() === "edit" })
    disabled(p.terms, { when: () => this.lock() });
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
      providers: [provideZonelessChangeDetection()],
    });
  });

  it("renders the checkbox and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostSignalCheckbox);
    await settle(fixture);
    expect(checkbox(fixture).checked).toBe(false);

    fixture.componentInstance.f.terms().value.set(true);
    await settle(fixture);
    expect(checkbox(fixture).checked).toBe(true);
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
      const fixture = TestBed.createComponent(HostSignalCheckbox);
      await settle(fixture);

      expect(fixture.componentInstance.f.terms().disabled()).toBe(false);
      expect(fixture.componentInstance.field().disabled()).toBe(false);
      expect(checkbox(fixture).disabled).toBe(false);

      fixture.componentInstance.lock.set(true);
      await settle(fixture);

      expect(fixture.componentInstance.f.terms().disabled()).toBe(true);
      expect(fixture.componentInstance.field().disabled()).toBe(true);
      expect(checkbox(fixture).disabled).toBe(true);
    });
  });
});
