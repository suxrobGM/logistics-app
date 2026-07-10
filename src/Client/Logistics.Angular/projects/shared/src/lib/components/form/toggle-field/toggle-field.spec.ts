/**
 * Proves the wrapper contract that the whole PrimeNG -> spartan migration rests on.
 *
 * `UiToggleField` implements ONLY `FormValueControl`. It must therefore work:
 *   1. under Signal Forms `[formField]`,
 *   2. under legacy Reactive Forms `formControlName` (Angular 22's automatic bridge),
 *   3. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors — under BOTH form systems.
 *
 * `p-toggleswitch` exposes no `onBlur` output, so `touch` is raised from the native, bubbling
 * `(focusout)` on the host. `Validators.required` treats `false` as a present value, so the
 * "required" behaviour is asserted with `requiredTrue` (Reactive) and an equivalent custom
 * `requiredError()` validator (Signal Forms): a toggle that must be ON (true).
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { form, FormField, requiredError, validate } from "@angular/forms/signals";
import { UiFormField } from "../form-field/form-field";
import { UiToggleField } from "./toggle-field";

/** Reactive Forms host: wrapper bound with formControlName, wrapped in ui-form-field chrome. */
@Component({
  selector: "ui-host-reactive-toggle",
  imports: [UiToggleField, UiFormField, ReactiveFormsModule],
  template: `
    <form [formGroup]="fg">
      <ui-form-field label="Accept" for="accept" [required]="true">
        <ui-toggle-field inputId="accept" formControlName="accept" />
      </ui-form-field>
    </form>
  `,
})
class HostReactiveToggle {
  readonly fg = new FormGroup({
    accept: new FormControl(true, { nonNullable: true, validators: [Validators.requiredTrue] }),
  });
  readonly field = viewChild.required(UiToggleField);
}

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
  readonly model = signal({ accept: true });
  readonly f = form(this.model, (p) => {
    validate(p.accept, (ctx) => (ctx.value() ? null : requiredError()));
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
  (fixture.nativeElement.querySelector("p-toggleswitch") as HTMLElement).click();
}

describe("UiToggleField — a FormValueControl-only wrapper", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("renders the PrimeNG switch and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostReactiveToggle);
    await settle(fixture);
    expect(input(fixture).checked).toBe(true);
  });

  describe("under legacy Reactive Forms (formControlName)", () => {
    it("syncs control -> view", async () => {
      const fixture = TestBed.createComponent(HostReactiveToggle);
      await settle(fixture);

      fixture.componentInstance.fg.controls.accept.setValue(false);
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe(false);
      expect(input(fixture).checked).toBe(false);
    });

    it("syncs view -> control (toggling)", async () => {
      const fixture = TestBed.createComponent(HostReactiveToggle);
      await settle(fixture);

      toggle(fixture); // true -> false
      await settle(fixture);

      expect(fixture.componentInstance.fg.controls.accept.value).toBe(false);
    });

    it("propagates disabled state from the control", async () => {
      const fixture = TestBed.createComponent(HostReactiveToggle);
      await settle(fixture);

      fixture.componentInstance.fg.controls.accept.disable();
      await settle(fixture);

      expect(input(fixture).disabled).toBe(true);
    });
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
      // focusout bubbles from the inner input to the host, raising `touch`,
      // which Signal Forms uses to mark the field touched
      input(fixture).dispatchEvent(new Event("focusout", { bubbles: true }));
      await settle(fixture);

      expect(fixture.componentInstance.f.accept().invalid()).toBe(true);
      expect(fixture.nativeElement.textContent).toContain("This field is required.");
    });
  });
});
