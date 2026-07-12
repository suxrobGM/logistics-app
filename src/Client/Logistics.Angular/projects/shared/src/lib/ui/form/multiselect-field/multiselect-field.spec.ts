/**
 * Proves the wrapper contract that the whole `ui-*-field` layer rests on.
 *
 * `UiMultiSelectField` implements ONLY `FormValueControl` — no value-accessor glue of any kind. It must work:
 *   1. under Signal Forms `[formField]`,
 *   2. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors.
 *
 * The inner spartan `hlm-select-multiple` (brain `BrnSelectMultiple` + `BrnPopover`) is driven by
 * plain `[value]` / `(valueChange)`; `uiDetachedControl` severs the ambient `NgControl`. A user
 * picking options is exercised through the `BrnSelectMultiple` value model.
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { disabled, form, FormField, requiredError, validate } from "@angular/forms/signals";
import { By } from "@angular/platform-browser";
import { BrnSelectMultiple } from "@spartan-ng/brain/select";
import { UiFormField } from "../form-field/form-field";
import { UiMultiSelectField } from "./multiselect-field";

const OPTIONS = [
  { label: "Alpha", value: "a" },
  { label: "Beta", value: "b" },
];

/** Signal Forms host: the SAME wrapper bound with [formField]. */
@Component({
  selector: "ui-host-signal-multiselect",
  imports: [UiMultiSelectField, UiFormField, FormField],
  template: `
    <ui-form-field label="Tags" for="tags" [required]="true">
      <ui-multiselect-field
        id="tags"
        [formField]="f.tags"
        [options]="options"
        optionLabel="label"
        optionValue="value"
      />
    </ui-form-field>
  `,
})
class HostSignalMultiSelect {
  /** Flips the schema's disabled rule — proves the wrapper REACTS, not just reads once. */
  readonly lock = signal(false);
  readonly options = OPTIONS;
  readonly model = signal<{ tags: string[] }>({ tags: ["a"] });
  readonly f = form(this.model, (p) => {
    // Signal Forms' built-in `required()` treats only "", false, and null/undefined as empty
    // (see `isEmpty` in @angular/forms/signals) — an empty ARRAY passes it and would count as
    // valid. So to make "no selection" invalid we add a custom validator that raises a
    // `required`-kind error, which `ui-form-field` renders as "This field is required."
    validate(p.tags, (ctx) =>
      ctx.value().length === 0 ? requiredError({ message: "This field is required." }) : undefined,
    );
    // Reactive disabled rule — the shape every real form uses:
    //   disabled(p.truckId, { when: () => this.mode() === "edit" })
    disabled(p.tags, { when: () => this.lock() });
  });
  readonly field = viewChild.required(UiMultiSelectField);
}

async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

/** The inner brain BrnSelectMultiple instance (host-directed by hlm-select-multiple). */
function brnSelect(fixture: ComponentFixture<unknown>): BrnSelectMultiple<string> {
  return fixture.debugElement
    .query(By.css("hlm-select-multiple"))
    .injector.get(BrnSelectMultiple) as BrnSelectMultiple<string>;
}

/** Simulate a user picking options — a click drives the BrnSelectMultiple value model. */
function selectValue(fixture: ComponentFixture<unknown>, value: string[]): void {
  brnSelect(fixture).value.set(value);
}

/** The trigger the user actually clicks — spartan renders it as a real <button>. */
function triggerButton(fixture: ComponentFixture<unknown>): HTMLButtonElement {
  return fixture.nativeElement.querySelector("hlm-select-trigger button") as HTMLButtonElement;
}

describe("UiMultiSelectField — a FormValueControl-only wrapper", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("renders the multiselect and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostSignalMultiSelect);
    await settle(fixture);
    expect(fixture.nativeElement.querySelector("hlm-select-multiple")).toBeTruthy();
    expect(fixture.componentInstance.field().value()).toEqual(["a"]);
  });
  describe("under Signal Forms ([formField])", () => {
    it("syncs field -> view", async () => {
      const fixture = TestBed.createComponent(HostSignalMultiSelect);
      await settle(fixture);

      fixture.componentInstance.f.tags().value.set(["a", "b"]);
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toEqual(["a", "b"]);
    });

    it("syncs view -> field (selecting)", async () => {
      const fixture = TestBed.createComponent(HostSignalMultiSelect);
      await settle(fixture);

      selectValue(fixture, ["b"]);
      await settle(fixture);

      expect(fixture.componentInstance.model().tags).toEqual(["b"]);
    });

    it("ui-form-field renders the required error once touched — with NO transitional code", async () => {
      const fixture = TestBed.createComponent(HostSignalMultiSelect);
      await settle(fixture);

      fixture.componentInstance.f.tags().value.set([]);
      fixture.componentInstance.f.tags().markAsTouched();
      await settle(fixture);

      expect(fixture.componentInstance.f.tags().invalid()).toBe(true);
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
      const fixture = TestBed.createComponent(HostSignalMultiSelect);
      await settle(fixture);

      expect(fixture.componentInstance.f.tags().disabled()).toBe(false);
      expect(fixture.componentInstance.field().disabled()).toBe(false);
      expect(triggerButton(fixture).disabled).toBe(false);

      fixture.componentInstance.lock.set(true);
      await settle(fixture);

      expect(fixture.componentInstance.f.tags().disabled()).toBe(true);
      expect(fixture.componentInstance.field().disabled()).toBe(true);
      expect(triggerButton(fixture).disabled).toBe(true);
    });
  });
});
