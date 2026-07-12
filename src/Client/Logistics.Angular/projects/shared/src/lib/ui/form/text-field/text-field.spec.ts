/**
 * Proves the wrapper contract the whole `ui-*-field` layer rests on.
 *
 * `UiTextField` implements ONLY `FormValueControl` — it has no value-accessor glue of any kind.
 * It must therefore work:
 *   1. under Signal Forms `[formField]`,
 *   2. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors.
 *
 * (2) is the reason `ui-form-field` needs no transitional code: `[formField]` provides
 * NgControl as an `InteropNgControl` whose `errors` getter returns the classic
 * `ValidationErrors` object shape.
 *
 * If any of these break, every `ui-*-field` wrapper breaks with them.
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { disabled, form, FormField, required } from "@angular/forms/signals";
import { UiFormField } from "../form-field/form-field";
import { UiTextField } from "./text-field";

/** Signal Forms host: the SAME wrapper bound with [formField]. */
@Component({
  selector: "ui-host-signal-text",
  imports: [UiTextField, UiFormField, FormField],
  template: `
    <ui-form-field label="Name" for="name" [required]="true">
      <ui-text-field id="name" [formField]="f.name" />
    </ui-form-field>
  `,
})
class HostSignalText {
  /** Flips the schema's disabled rule — proves the wrapper REACTS, not just reads once. */
  readonly lock = signal(false);
  readonly model = signal({ name: "initial" });
  readonly f = form(this.model, (p) => {
    required(p.name, { message: "This field is required." });
    // Reactive disabled rule — the shape every real form uses:
    //   disabled(p.truckId, { when: () => this.mode() === "edit" })
    disabled(p.name, { when: () => this.lock() });
  });
  readonly field = viewChild.required(UiTextField);
}

/** Signal Forms host whose required field starts EMPTY — so it is invalid while still pristine. */
@Component({
  selector: "ui-host-signal-required-empty",
  imports: [UiTextField, UiFormField, FormField],
  template: `
    <ui-form-field label="Name" for="name" [required]="true">
      <ui-text-field id="name" [formField]="f.name" />
    </ui-form-field>
  `,
})
class HostSignalRequiredEmpty {
  readonly model = signal({ name: "" });
  readonly f = form(this.model, (p) => {
    required(p.name, { message: "This field is required." });
  });
  readonly field = viewChild.required(UiTextField);
}

async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

function input(fixture: ComponentFixture<unknown>): HTMLInputElement {
  return fixture.nativeElement.querySelector("input") as HTMLInputElement;
}

function type(fixture: ComponentFixture<unknown>, text: string): void {
  const el = input(fixture);
  el.value = text;
  el.dispatchEvent(new Event("input"));
}

describe("UiTextField — a FormValueControl-only wrapper", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("renders the input and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostSignalText);
    await settle(fixture);
    expect(input(fixture).value).toBe("initial");
  });
  describe("under Signal Forms ([formField])", () => {
    it("syncs field -> view", async () => {
      const fixture = TestBed.createComponent(HostSignalText);
      await settle(fixture);

      fixture.componentInstance.f.name().value.set("from-field");
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe("from-field");
      expect(input(fixture).value).toBe("from-field");
    });

    it("syncs view -> field (typing)", async () => {
      const fixture = TestBed.createComponent(HostSignalText);
      await settle(fixture);

      type(fixture, "typed");
      await settle(fixture);

      expect(fixture.componentInstance.model().name).toBe("typed");
    });

    it("ui-form-field renders the required error once touched — with NO transitional code", async () => {
      const fixture = TestBed.createComponent(HostSignalText);
      await settle(fixture);

      type(fixture, "");
      // blur raises `touch`, which Signal Forms uses to mark the field touched
      input(fixture).dispatchEvent(new Event("blur"));
      await settle(fixture);

      expect(fixture.componentInstance.f.name().invalid()).toBe(true);
      expect(fixture.nativeElement.textContent).toContain("This field is required.");
    });

    it("does not report aria-invalid until the field is touched", async () => {
      const fixture = TestBed.createComponent(HostSignalRequiredEmpty);
      await settle(fixture);

      // The field is required and empty, so Signal Forms already reports it invalid — but while
      // pristine the wrapper must NOT surface that on the inner input.
      expect(fixture.componentInstance.f.name().invalid()).toBe(true);
      expect(input(fixture).getAttribute("aria-invalid")).not.toBe("true");

      fixture.componentInstance.f.name().markAsTouched();
      await settle(fixture);

      expect(input(fixture).getAttribute("aria-invalid")).toBe("true");
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
      const fixture = TestBed.createComponent(HostSignalText);
      await settle(fixture);

      expect(fixture.componentInstance.f.name().disabled()).toBe(false);
      expect(fixture.componentInstance.field().disabled()).toBe(false);
      expect(input(fixture).disabled).toBe(false);

      fixture.componentInstance.lock.set(true);
      await settle(fixture);

      expect(fixture.componentInstance.f.name().disabled()).toBe(true);
      expect(fixture.componentInstance.field().disabled()).toBe(true);
      expect(input(fixture).disabled).toBe(true);
    });
  });
});
