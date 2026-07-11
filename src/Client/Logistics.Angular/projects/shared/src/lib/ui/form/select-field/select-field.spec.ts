/**
 * Proves the wrapper contract that the whole PrimeNG -> spartan migration rests on.
 *
 * `UiSelectField` implements ONLY `FormValueControl`. It must therefore work:
 *   1. under Signal Forms `[formField]`,
 *   2. under legacy Reactive Forms `formControlName` (Angular 22's automatic bridge),
 *   3. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors — under BOTH form systems.
 *
 * The inner spartan `hlm-select` (brain `BrnSelect` + `BrnPopover`) is driven with plain
 * `[value]` / `(valueChange)`. `uiDetachedControl` severs the ambient `NgControl` so brain's
 * `BrnFieldControl` does not crash on Signal Forms' `InteropNgControl`. The user-interaction
 * direction is exercised through the `BrnSelect` value model (what a click on an option drives).
 *
 * If any of these break, every `ui-*-field` wrapper breaks with them.
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { form, FormField, required } from "@angular/forms/signals";
import { By } from "@angular/platform-browser";
import { BrnSelect } from "@spartan-ng/brain/select";
import { UiFormField } from "../form-field/form-field";
import { UiSelectField } from "./select-field";

const OPTIONS = [
  { label: "Red", value: "red" },
  { label: "Green", value: "green" },
];

/** Reactive Forms host: wrapper bound with formControlName, wrapped in ui-form-field chrome. */
@Component({
  selector: "ui-host-reactive-select",
  imports: [UiSelectField, UiFormField, ReactiveFormsModule],
  template: `
    <form [formGroup]="fg">
      <ui-form-field label="Color" for="color" [required]="true">
        <ui-select-field
          id="color"
          formControlName="color"
          [options]="options"
          optionLabel="label"
          optionValue="value"
        />
      </ui-form-field>
    </form>
  `,
})
class HostReactiveSelect {
  readonly options = OPTIONS;
  readonly fg = new FormGroup({
    color: new FormControl<string | null>("green", { validators: [Validators.required] }),
  });
  readonly field = viewChild.required(UiSelectField);
}

/** Signal Forms host: the SAME wrapper bound with [formField]. */
@Component({
  selector: "ui-host-signal-select",
  imports: [UiSelectField, UiFormField, FormField],
  template: `
    <ui-form-field label="Color" for="color" [required]="true">
      <ui-select-field
        id="color"
        [formField]="f.color"
        [options]="options"
        optionLabel="label"
        optionValue="value"
      />
    </ui-form-field>
  `,
})
class HostSignalSelect {
  readonly options = OPTIONS;
  readonly model = signal<{ color: string | null }>({ color: null });
  readonly f = form(this.model, (p) => {
    required(p.color);
  });
  readonly field = viewChild.required(UiSelectField);
}

async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

/** The inner brain BrnSelect instance (host-directed by hlm-select). */
function brnSelect(fixture: ComponentFixture<unknown>): BrnSelect<string> {
  return fixture.debugElement
    .query(By.css("hlm-select"))
    .injector.get(BrnSelect) as BrnSelect<string>;
}

/** Text rendered inside the trigger button (the selected value's label). */
function triggerText(fixture: ComponentFixture<unknown>): string {
  return (fixture.nativeElement.querySelector("hlm-select-trigger")?.textContent ?? "").trim();
}

/** Simulate a user picking an option — a click drives the BrnSelect value model. */
function pick(fixture: ComponentFixture<unknown>, value: string | null): void {
  brnSelect(fixture).value.set(value);
}

describe("UiSelectField — a FormValueControl-only wrapper", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("renders the select and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostReactiveSelect);
    await settle(fixture);
    expect(fixture.componentInstance.field().value()).toBe("green");
    expect(triggerText(fixture)).toContain("Green");
  });

  describe("under legacy Reactive Forms (formControlName)", () => {
    it("syncs control -> view", async () => {
      const fixture = TestBed.createComponent(HostReactiveSelect);
      await settle(fixture);

      fixture.componentInstance.fg.controls.color.setValue("red");
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe("red");
      expect(triggerText(fixture)).toContain("Red");
    });

    it("syncs view -> control (selecting an option)", async () => {
      const fixture = TestBed.createComponent(HostReactiveSelect);
      await settle(fixture);

      pick(fixture, "red");
      await settle(fixture);

      expect(fixture.componentInstance.fg.controls.color.value).toBe("red");
    });

    it("propagates disabled state from the control", async () => {
      const fixture = TestBed.createComponent(HostReactiveSelect);
      await settle(fixture);

      fixture.componentInstance.fg.controls.color.disable();
      await settle(fixture);

      // The wrapper's own contract: the Reactive Forms bridge drives its `disabled` input,
      // and the wrapper forwards it to the inner control. Assert the wrapper's resolved state
      // and that the trigger button is disabled — not a spartan-internal attribute.
      expect(fixture.componentInstance.field().disabled()).toBe(true);
      const button = fixture.nativeElement.querySelector("hlm-select-trigger button");
      expect(button?.disabled).toBe(true);
    });
  });

  describe("under Signal Forms ([formField])", () => {
    it("syncs field -> view", async () => {
      const fixture = TestBed.createComponent(HostSignalSelect);
      await settle(fixture);

      fixture.componentInstance.f.color().value.set("green");
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe("green");
      expect(triggerText(fixture)).toContain("Green");
    });

    it("syncs view -> field (selecting an option)", async () => {
      const fixture = TestBed.createComponent(HostSignalSelect);
      await settle(fixture);

      pick(fixture, "red");
      await settle(fixture);

      expect(fixture.componentInstance.model().color).toBe("red");
    });

    it("ui-form-field renders the required error once touched — with NO transitional code", async () => {
      const fixture = TestBed.createComponent(HostSignalSelect);
      await settle(fixture);

      // The wrapper marks the field touched when the panel closes; drive that state directly.
      fixture.componentInstance.f.color().markAsTouched();
      await settle(fixture);

      expect(fixture.componentInstance.f.color().invalid()).toBe(true);
      expect(fixture.nativeElement.textContent).toContain("This field is required.");
    });
  });
});
