/**
 * Proves the wrapper contract that the whole `ui-*-field` layer rests on, for the numeric wrapper.
 * `UiNumberField` implements ONLY `FormValueControl<number | null>` — no value-accessor glue of any
 * kind. It must therefore work:
 *   1. under Signal Forms `[formField]`,
 *   2. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and render
 *      validation errors.
 *
 * The inner numeric input commits its value on blur, which is also the moment it emits `touch`.
 * The `enter()` helper below therefore simulates a real "type then leave the field" interaction.
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { disabled, form, FormField, required } from "@angular/forms/signals";
import { UiFormField } from "../form-field/form-field";
import { UiNumberField } from "./number-field";

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
  /** Flips the schema's disabled rule — proves the wrapper REACTS, not just reads once. */
  readonly lock = signal(false);
  readonly model = signal<{ rate: number | null }>({ rate: 10 });
  readonly f = form(this.model, (p) => {
    required(p.rate, { message: "This field is required." });
    // Reactive disabled rule — the shape every real form uses:
    //   disabled(p.truckId, { when: () => this.mode() === "edit" })
    disabled(p.rate, { when: () => this.lock() });
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
 * Simulate typing `text` and leaving the field: the native input commits on `input`, then
 * `blur` clamps/reformats and marks the field touched.
 */
function enter(fixture: ComponentFixture<unknown>, text: string): void {
  const el = input(fixture);
  el.value = text;
  el.dispatchEvent(new Event("input"));
  el.dispatchEvent(new Event("blur"));
}

describe("UiNumberField — a FormValueControl-only wrapper", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("renders the input-number and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostSignalNumber);
    await settle(fixture);
    expect(input(fixture).value).toContain("10");
    expect(fixture.componentInstance.field().value()).toBe(10);
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
      const fixture = TestBed.createComponent(HostSignalNumber);
      await settle(fixture);

      expect(fixture.componentInstance.f.rate().disabled()).toBe(false);
      expect(fixture.componentInstance.field().disabled()).toBe(false);
      expect(input(fixture).disabled).toBe(false);

      fixture.componentInstance.lock.set(true);
      await settle(fixture);

      expect(fixture.componentInstance.f.rate().disabled()).toBe(true);
      expect(fixture.componentInstance.field().disabled()).toBe(true);
      expect(input(fixture).disabled).toBe(true);
    });
  });
});
