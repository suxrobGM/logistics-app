/**
 * Proves the wrapper contract that the whole PrimeNG -> spartan migration rests on.
 *
 * `UiTextField` implements ONLY `FormValueControl`. It must therefore work:
 *   1. under Signal Forms `[formField]`,
 *   2. under legacy Reactive Forms `formControlName` (Angular 22's automatic bridge),
 *   3. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors — under BOTH form systems.
 *
 * (3) is the reason `ui-form-field` needs no transitional code: `[formField]` provides
 * NgControl as an `InteropNgControl` whose `errors` getter returns the classic
 * `ValidationErrors` object shape.
 *
 * If any of these break, every `ui-*-field` wrapper breaks with them.
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { form, FormField, required } from "@angular/forms/signals";
import { UiFormField } from "../form-field/form-field";
import { UiTextField } from "./text-field";

/** Reactive Forms host: wrapper bound with formControlName, wrapped in ui-form-field chrome. */
@Component({
  selector: "ui-host-reactive-text",
  imports: [UiTextField, UiFormField, ReactiveFormsModule],
  template: `
    <form [formGroup]="fg">
      <ui-form-field label="Name" for="name" [required]="true">
        <ui-text-field id="name" formControlName="name" />
      </ui-form-field>
    </form>
  `,
})
class HostReactiveText {
  readonly fg = new FormGroup({
    name: new FormControl("initial", { nonNullable: true, validators: [Validators.required] }),
  });
  readonly field = viewChild.required(UiTextField);
}

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
  readonly model = signal({ name: "initial" });
  readonly f = form(this.model, (p) => {
    required(p.name);
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

  it("renders the PrimeNG input and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostReactiveText);
    await settle(fixture);
    expect(input(fixture).value).toBe("initial");
  });

  describe("under legacy Reactive Forms (formControlName)", () => {
    it("syncs control -> view", async () => {
      const fixture = TestBed.createComponent(HostReactiveText);
      await settle(fixture);

      fixture.componentInstance.fg.controls.name.setValue("from-control");
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe("from-control");
      expect(input(fixture).value).toBe("from-control");
    });

    it("syncs view -> control (typing)", async () => {
      const fixture = TestBed.createComponent(HostReactiveText);
      await settle(fixture);

      type(fixture, "typed");
      await settle(fixture);

      expect(fixture.componentInstance.fg.controls.name.value).toBe("typed");
    });

    it("propagates disabled state from the control", async () => {
      const fixture = TestBed.createComponent(HostReactiveText);
      await settle(fixture);

      fixture.componentInstance.fg.controls.name.disable();
      await settle(fixture);

      expect(input(fixture).disabled).toBe(true);
    });
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
  });
});
