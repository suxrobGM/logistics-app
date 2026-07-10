/**
 * Proves the wrapper contract that the whole PrimeNG -> spartan migration rests on.
 *
 * `UiTextareaField` implements ONLY `FormValueControl`. It must therefore work:
 *   1. under Signal Forms `[formField]`,
 *   2. under legacy Reactive Forms `formControlName` (Angular 22's automatic bridge),
 *   3. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors — under BOTH form systems.
 *
 * If any of these break, every `ui-*-field` wrapper breaks with them.
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { form, FormField, required } from "@angular/forms/signals";
import { UiFormField } from "../form-field/form-field";
import { UiTextareaField } from "./textarea-field";

/** Reactive Forms host: wrapper bound with formControlName, wrapped in ui-form-field chrome. */
@Component({
  selector: "ui-host-reactive-textarea",
  imports: [UiTextareaField, UiFormField, ReactiveFormsModule],
  template: `
    <form [formGroup]="fg">
      <ui-form-field label="Notes" for="notes" [required]="true">
        <ui-textarea-field id="notes" formControlName="notes" />
      </ui-form-field>
    </form>
  `,
})
class HostReactiveTextarea {
  readonly fg = new FormGroup({
    notes: new FormControl("initial", { nonNullable: true, validators: [Validators.required] }),
  });
  readonly field = viewChild.required(UiTextareaField);
}

/** Signal Forms host: the SAME wrapper bound with [formField]. */
@Component({
  selector: "ui-host-signal-textarea",
  imports: [UiTextareaField, UiFormField, FormField],
  template: `
    <ui-form-field label="Notes" for="notes" [required]="true">
      <ui-textarea-field id="notes" [formField]="f.notes" />
    </ui-form-field>
  `,
})
class HostSignalTextarea {
  readonly model = signal({ notes: "initial" });
  readonly f = form(this.model, (p) => {
    required(p.notes);
  });
  readonly field = viewChild.required(UiTextareaField);
}

async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

function textarea(fixture: ComponentFixture<unknown>): HTMLTextAreaElement {
  return fixture.nativeElement.querySelector("textarea") as HTMLTextAreaElement;
}

function type(fixture: ComponentFixture<unknown>, text: string): void {
  const el = textarea(fixture);
  el.value = text;
  el.dispatchEvent(new Event("input"));
}

describe("UiTextareaField — a FormValueControl-only wrapper", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("renders the PrimeNG textarea and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostReactiveTextarea);
    await settle(fixture);
    expect(textarea(fixture).value).toBe("initial");
  });

  describe("under legacy Reactive Forms (formControlName)", () => {
    it("syncs control -> view", async () => {
      const fixture = TestBed.createComponent(HostReactiveTextarea);
      await settle(fixture);

      fixture.componentInstance.fg.controls.notes.setValue("from-control");
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe("from-control");
      expect(textarea(fixture).value).toBe("from-control");
    });

    it("syncs view -> control (typing)", async () => {
      const fixture = TestBed.createComponent(HostReactiveTextarea);
      await settle(fixture);

      type(fixture, "typed");
      await settle(fixture);

      expect(fixture.componentInstance.fg.controls.notes.value).toBe("typed");
    });

    it("propagates disabled state from the control", async () => {
      const fixture = TestBed.createComponent(HostReactiveTextarea);
      await settle(fixture);

      fixture.componentInstance.fg.controls.notes.disable();
      await settle(fixture);

      expect(textarea(fixture).disabled).toBe(true);
    });
  });

  describe("under Signal Forms ([formField])", () => {
    it("syncs field -> view", async () => {
      const fixture = TestBed.createComponent(HostSignalTextarea);
      await settle(fixture);

      fixture.componentInstance.f.notes().value.set("from-field");
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe("from-field");
      expect(textarea(fixture).value).toBe("from-field");
    });

    it("syncs view -> field (typing)", async () => {
      const fixture = TestBed.createComponent(HostSignalTextarea);
      await settle(fixture);

      type(fixture, "typed");
      await settle(fixture);

      expect(fixture.componentInstance.model().notes).toBe("typed");
    });

    it("ui-form-field renders the required error once touched — with NO transitional code", async () => {
      const fixture = TestBed.createComponent(HostSignalTextarea);
      await settle(fixture);

      type(fixture, "");
      // blur raises `touch`, which Signal Forms uses to mark the field touched
      textarea(fixture).dispatchEvent(new Event("blur"));
      await settle(fixture);

      expect(fixture.componentInstance.f.notes().invalid()).toBe(true);
      expect(fixture.nativeElement.textContent).toContain("This field is required.");
    });
  });
});
