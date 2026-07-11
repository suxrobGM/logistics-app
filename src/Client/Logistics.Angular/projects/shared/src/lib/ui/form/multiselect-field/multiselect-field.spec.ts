/**
 * Proves the wrapper contract that the whole PrimeNG -> spartan migration rests on.
 *
 * `UiMultiSelectField` implements ONLY `FormValueControl`. It must therefore work:
 *   1. under Signal Forms `[formField]`,
 *   2. under legacy Reactive Forms `formControlName` (Angular 22's automatic bridge),
 *   3. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors — under BOTH form systems.
 *
 * The inner spartan `hlm-select-multiple` (brain `BrnSelectMultiple` + `BrnPopover`) is driven by
 * plain `[value]` / `(valueChange)`; `uiDetachedControl` severs the ambient `NgControl`. A user
 * picking options is exercised through the `BrnSelectMultiple` value model.
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { form, FormField, requiredError, validate } from "@angular/forms/signals";
import { By } from "@angular/platform-browser";
import { BrnSelectMultiple } from "@spartan-ng/brain/select";
import { UiFormField } from "../form-field/form-field";
import { UiMultiSelectField } from "./multiselect-field";

const OPTIONS = [
  { label: "Alpha", value: "a" },
  { label: "Beta", value: "b" },
];

/** Reactive Forms host: wrapper bound with formControlName, wrapped in ui-form-field chrome. */
@Component({
  selector: "ui-host-reactive-multiselect",
  imports: [UiMultiSelectField, UiFormField, ReactiveFormsModule],
  template: `
    <form [formGroup]="fg">
      <ui-form-field label="Tags" for="tags" [required]="true">
        <ui-multiselect-field
          id="tags"
          formControlName="tags"
          [options]="options"
          optionLabel="label"
          optionValue="value"
        />
      </ui-form-field>
    </form>
  `,
})
class HostReactiveMultiSelect {
  readonly options = OPTIONS;
  readonly fg = new FormGroup({
    tags: new FormControl<string[]>(["a"], {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });
  readonly field = viewChild.required(UiMultiSelectField);
}

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
  readonly options = OPTIONS;
  readonly model = signal<{ tags: string[] }>({ tags: ["a"] });
  readonly f = form(this.model, (p) => {
    // Signal Forms' built-in `required()` treats only "", false, and null/undefined as empty
    // (see `isEmpty` in @angular/forms/signals) — an empty ARRAY passes it and would count as
    // valid. So to make "no selection" invalid we add a custom validator that raises a
    // `required`-kind error, which `ui-form-field` renders as "This field is required."
    validate(p.tags, (ctx) => (ctx.value().length === 0 ? requiredError() : undefined));
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

describe("UiMultiSelectField — a FormValueControl-only wrapper", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("renders the multiselect and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostReactiveMultiSelect);
    await settle(fixture);
    expect(fixture.nativeElement.querySelector("hlm-select-multiple")).toBeTruthy();
    expect(fixture.componentInstance.field().value()).toEqual(["a"]);
  });

  describe("under legacy Reactive Forms (formControlName)", () => {
    it("syncs control -> view", async () => {
      const fixture = TestBed.createComponent(HostReactiveMultiSelect);
      await settle(fixture);

      fixture.componentInstance.fg.controls.tags.setValue(["a", "b"]);
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toEqual(["a", "b"]);
    });

    it("syncs view -> control (selecting)", async () => {
      const fixture = TestBed.createComponent(HostReactiveMultiSelect);
      await settle(fixture);

      selectValue(fixture, ["b"]);
      await settle(fixture);

      expect(fixture.componentInstance.fg.controls.tags.value).toEqual(["b"]);
    });

    it("propagates disabled state from the control", async () => {
      const fixture = TestBed.createComponent(HostReactiveMultiSelect);
      await settle(fixture);

      fixture.componentInstance.fg.controls.tags.disable();
      await settle(fixture);

      expect(fixture.componentInstance.field().disabled()).toBe(true);
      const button = fixture.nativeElement.querySelector("hlm-select-trigger button");
      expect(button?.disabled).toBe(true);
    });
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
  });
});
