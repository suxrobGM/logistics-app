/**
 * Proves the wrapper contract that the whole PrimeNG -> spartan migration rests on.
 *
 * `UiPasswordField` implements ONLY `FormValueControl`. It must therefore work:
 *   1. under Signal Forms `[formField]`,
 *   2. under legacy Reactive Forms `formControlName` (Angular 22's automatic bridge),
 *   3. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors — under BOTH form systems.
 *
 * `p-password` extends `BaseInput`, which declares a `pattern` input that collides with
 * Signal Forms' `pattern` state input — the exact reason `[formField]` must land on THIS
 * wrapper and never on the `p-password` element. See the class doc comment.
 *
 * If any of these break, every `ui-*-field` wrapper breaks with them.
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { form, FormField, required } from "@angular/forms/signals";
import { provideIcons } from "@ng-icons/core";
import { BASE_NG_ICONS } from "../../icons/icon-registry.generated";
import { UiFormField } from "../form-field/form-field";
import { UiPasswordField } from "./password-field";

/** Reactive Forms host: wrapper bound with formControlName, wrapped in ui-form-field chrome. */
@Component({
  selector: "ui-host-reactive-password",
  imports: [UiPasswordField, UiFormField, ReactiveFormsModule],
  template: `
    <form [formGroup]="fg">
      <ui-form-field label="Secret" for="secret" [required]="true">
        <ui-password-field id="secret" formControlName="secret" [feedback]="false" />
      </ui-form-field>
    </form>
  `,
})
class HostReactivePassword {
  readonly fg = new FormGroup({
    secret: new FormControl("initial", { nonNullable: true, validators: [Validators.required] }),
  });
  readonly field = viewChild.required(UiPasswordField);
}

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
  readonly model = signal({ secret: "initial" });
  readonly f = form(this.model, (p) => {
    required(p.secret);
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
      providers: [provideZonelessChangeDetection(), provideIcons(BASE_NG_ICONS)],
    });
  });

  it("renders the PrimeNG password input and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostReactivePassword);
    await settle(fixture);
    expect(input(fixture).value).toBe("initial");
  });

  describe("under legacy Reactive Forms (formControlName)", () => {
    it("syncs control -> view", async () => {
      const fixture = TestBed.createComponent(HostReactivePassword);
      await settle(fixture);

      fixture.componentInstance.fg.controls.secret.setValue("from-control");
      await settle(fixture);

      expect(fixture.componentInstance.field().value()).toBe("from-control");
      expect(input(fixture).value).toBe("from-control");
    });

    it("syncs view -> control (typing)", async () => {
      const fixture = TestBed.createComponent(HostReactivePassword);
      await settle(fixture);

      type(fixture, "typed");
      await settle(fixture);

      expect(fixture.componentInstance.fg.controls.secret.value).toBe("typed");
    });

    it("propagates disabled state from the control", async () => {
      const fixture = TestBed.createComponent(HostReactivePassword);
      await settle(fixture);

      fixture.componentInstance.fg.controls.secret.disable();
      await settle(fixture);

      expect(input(fixture).disabled).toBe(true);
    });
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
  });
});
