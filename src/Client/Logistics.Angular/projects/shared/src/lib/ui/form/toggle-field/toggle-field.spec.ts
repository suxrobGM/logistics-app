/**
 * Proves the wrapper contract that the whole `ui-*-field` layer rests on.
 *
 * `UiToggleField` implements ONLY `FormValueControl` — no value-accessor glue of any kind. It must work:
 *   1. under Signal Forms `[formField]`,
 *   2. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors.
 *
 * `p-toggleswitch` exposes no `onBlur` output, so `touch` is raised from the native, bubbling
 * `(focusout)` on the host. `Validators.required` treats `false` as a present value, so the
 * "required" behaviour is asserted with `requiredTrue` (Reactive) and an equivalent custom
 * `requiredError()` validator (Signal Forms): a toggle that must be ON (true).
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { disabled, form, FormField, requiredError, validate } from "@angular/forms/signals";
import { UiFormField } from "../form-field/form-field";
import { UiToggleField } from "./toggle-field";

/** Signal Forms host: the SAME wrapper bound with [formField]. */
@Component({
  selector: "ui-host-signal-toggle",
  imports: [UiToggleField, UiFormField, FormField],
  template: `
    <ui-form-field label="Accept" for="accept" [required]="true">
      <ui-toggle-field inputId="accept" [formField]="f.accept" />
    </ui-form-field>
  `,
})
class HostSignalToggle {
  /** Flips the schema's disabled rule — proves the wrapper REACTS, not just reads once. */
  readonly lock = signal(false);
  readonly model = signal({ accept: true });
  readonly f = form(this.model, (p) => {
    validate(p.accept, (ctx) =>
      ctx.value() ? null : requiredError({ message: "This field is required." }),
    );
    // Reactive disabled rule — the shape every real form uses:
    //   disabled(p.truckId, { when: () => this.mode() === "edit" })
    disabled(p.accept, { when: () => this.lock() });
  });
  readonly field = viewChild.required(UiToggleField);
}

async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

function input(fixture: ComponentFixture<unknown>): HTMLInputElement {
  return fixture.nativeElement.querySelector("input") as HTMLInputElement;
}

/** Toggles the switch the way a user would — the click handler lives on the host element. */
function toggle(fixture: ComponentFixture<unknown>): void {
  input(fixture).click();
}

describe("UiToggleField — a FormValueControl-only wrapper", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("renders the switch and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostSignalToggle);
    await settle(fixture);
    expect(input(fixture).checked).toBe(true);
  });
  describe("under Signal Forms ([formField])", () => {
    it("syncs field -> view", async () => {
      const fixture = TestBed.createComponent(HostSignalToggle);
      await settle(fixture);

      fixture.componentInstance.f.accept().value.set(false);
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe(false);
      expect(input(fixture).checked).toBe(false);
    });

    it("syncs view -> field (toggling)", async () => {
      const fixture = TestBed.createComponent(HostSignalToggle);
      await settle(fixture);

      toggle(fixture); // true -> false
      await settle(fixture);

      expect(fixture.componentInstance.model().accept).toBe(false);
    });

    it("ui-form-field renders the required error once touched — with NO transitional code", async () => {
      const fixture = TestBed.createComponent(HostSignalToggle);
      await settle(fixture);

      fixture.componentInstance.f.accept().value.set(false);
      // blurring the inner checkbox raises `touch`, which Signal Forms uses to mark it touched
      input(fixture).dispatchEvent(new Event("blur"));
      await settle(fixture);

      expect(fixture.componentInstance.f.accept().invalid()).toBe(true);
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
      const fixture = TestBed.createComponent(HostSignalToggle);
      await settle(fixture);

      expect(fixture.componentInstance.f.accept().disabled()).toBe(false);
      expect(fixture.componentInstance.field().disabled()).toBe(false);
      expect(input(fixture).disabled).toBe(false);

      fixture.componentInstance.lock.set(true);
      await settle(fixture);

      expect(fixture.componentInstance.f.accept().disabled()).toBe(true);
      expect(fixture.componentInstance.field().disabled()).toBe(true);
      expect(input(fixture).disabled).toBe(true);
    });
  });
});
