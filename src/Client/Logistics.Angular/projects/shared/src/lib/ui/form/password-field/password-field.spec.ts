/**
 * Proves the wrapper contract that the whole `ui-*-field` layer rests on.
 *
 * `UiPasswordField` implements ONLY `FormValueControl` — no value-accessor glue of any kind. It must work:
 *   1. under Signal Forms `[formField]`,
 *   2. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors.
 *
 * `p-password` extends `BaseInput`, which declares a `pattern` input that collides with
 * Signal Forms' `pattern` state input — the exact reason `[formField]` must land on THIS
 * wrapper and never on the `p-password` element. See the class doc comment.
 *
 * If any of these break, every `ui-*-field` wrapper breaks with them.
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { disabled, form, FormField, required } from "@angular/forms/signals";
import { UiFormField } from "../form-field/form-field";
import { UiPasswordField } from "./password-field";

/** Signal Forms host: the SAME wrapper bound with [formField]. */
@Component({
  selector: "ui-host-signal-password",
  imports: [UiPasswordField, UiFormField, FormField],
  template: `
    <ui-form-field label="Secret" for="secret" [required]="true">
      <ui-password-field id="secret" [formField]="f.secret" [feedback]="false" />
    </ui-form-field>
  `,
})
class HostSignalPassword {
  /** Flips the schema's disabled rule — proves the wrapper REACTS, not just reads once. */
  readonly lock = signal(false);
  readonly model = signal({ secret: "initial" });
  readonly f = form(this.model, (p) => {
    required(p.secret, { message: "This field is required." });
    // Reactive disabled rule — the shape every real form uses:
    //   disabled(p.truckId, { when: () => this.mode() === "edit" })
    disabled(p.secret, { when: () => this.lock() });
  });
  readonly field = viewChild.required(UiPasswordField);
}

async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

/** p-password renders a native <input> internally; grab it to assert view state. */
function input(fixture: ComponentFixture<unknown>): HTMLInputElement {
  return fixture.nativeElement.querySelector("input") as HTMLInputElement;
}

function type(fixture: ComponentFixture<unknown>, text: string): void {
  const el = input(fixture);
  el.value = text;
  el.dispatchEvent(new Event("input"));
}

describe("UiPasswordField — a FormValueControl-only wrapper", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()],
    });
  });

  it("renders the password input and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostSignalPassword);
    await settle(fixture);
    expect(input(fixture).value).toBe("initial");
  });
  describe("under Signal Forms ([formField])", () => {
    it("syncs field -> view", async () => {
      const fixture = TestBed.createComponent(HostSignalPassword);
      await settle(fixture);

      fixture.componentInstance.f.secret().value.set("from-field");
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe("from-field");
      expect(input(fixture).value).toBe("from-field");
    });

    it("syncs view -> field (typing)", async () => {
      const fixture = TestBed.createComponent(HostSignalPassword);
      await settle(fixture);

      type(fixture, "typed");
      await settle(fixture);

      expect(fixture.componentInstance.model().secret).toBe("typed");
    });

    it("ui-form-field renders the required error once touched — with NO transitional code", async () => {
      const fixture = TestBed.createComponent(HostSignalPassword);
      await settle(fixture);

      type(fixture, "");
      // p-password re-emits its inner input's blur as the `onBlur` output, which raises
      // `touch`; Signal Forms uses that to mark the field touched.
      input(fixture).dispatchEvent(new Event("blur"));
      await settle(fixture);

      expect(fixture.componentInstance.f.secret().invalid()).toBe(true);
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
      const fixture = TestBed.createComponent(HostSignalPassword);
      await settle(fixture);

      expect(fixture.componentInstance.f.secret().disabled()).toBe(false);
      expect(fixture.componentInstance.field().disabled()).toBe(false);
      expect(input(fixture).disabled).toBe(false);

      fixture.componentInstance.lock.set(true);
      await settle(fixture);

      expect(fixture.componentInstance.f.secret().disabled()).toBe(true);
      expect(fixture.componentInstance.field().disabled()).toBe(true);
      expect(input(fixture).disabled).toBe(true);
    });
  });
});
