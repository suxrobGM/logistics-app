/**
 * `ui-form-field` renders inline validation errors for BOTH form systems.
 *
 * During Phase 4 the app has reactive forms and signal forms side by side, so this component
 * auto-resolves either. The interesting cases:
 *
 *  - Signal Forms errors are a `ValidationError[]` whose `kind`s are camelCase (`minLength`) and
 *    which carry their own `message`. We render `message` verbatim when present.
 *  - Reactive Forms errors are a keyed object (`{minlength: {requiredLength}}`) with no message.
 *    We flatten and describe them.
 *
 * The camelCase-vs-lowercase split is the trap: a template that only knew `minlength` renders
 * nothing for a signal form, while still (correctly) marking the field invalid.
 *
 * @see signal-forms-v22-api-probe.spec.ts, claim J
 */

import {
  ChangeDetectionStrategy,
  Component,
  model,
  provideZonelessChangeDetection,
  signal,
} from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import {
  form,
  FormField,
  minLength,
  required,
  type FormValueControl,
} from "@angular/forms/signals";
import { UiFormField } from "./form-field";

/** A bare FormValueControl — binds under `[formField]` and `formControlName` alike. */
@Component({
  selector: "ui-probe-input",
  template: `<input [value]="value()" (input)="onInput($event)" />`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class ProbeInput implements FormValueControl<string> {
  readonly value = model<string>("");
  protected onInput(event: Event): void {
    this.value.set((event.target as HTMLInputElement).value);
  }
}

@Component({
  selector: "ui-host-signal",
  imports: [UiFormField, ProbeInput, FormField],
  template: `
    <ui-form-field label="Name">
      <ui-probe-input [formField]="f.name" />
    </ui-form-field>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class HostSignal {
  readonly data = signal({ name: "" });
  readonly f = form(this.data, (p) => {
    required(p.name, { message: "Name is required." });
    minLength(p.name, 5, { message: "Name must be at least 5 characters." });
  });
}

/** A signal form whose validators supply NO message — exercises the fallback copy. */
@Component({
  selector: "ui-host-signal-nomsg",
  imports: [UiFormField, ProbeInput, FormField],
  template: `
    <ui-form-field label="Name">
      <ui-probe-input [formField]="f.name" />
    </ui-form-field>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class HostSignalNoMessage {
  readonly data = signal({ name: "ab" });
  readonly f = form(this.data, (p) => minLength(p.name, 5));
}

@Component({
  selector: "ui-host-reactive",
  imports: [UiFormField, ProbeInput, ReactiveFormsModule],
  template: `
    <form [formGroup]="fg">
      <ui-form-field label="Name">
        <ui-probe-input formControlName="name" />
      </ui-form-field>
    </form>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class HostReactive {
  readonly fg = new FormGroup({
    name: new FormControl("", {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(5)],
    }),
  });
}

async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

function errorText(fixture: ComponentFixture<unknown>): string {
  return (fixture.nativeElement as HTMLElement).textContent?.replace(/\s+/g, " ").trim() ?? "";
}

describe("UiFormField", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("stays quiet until the field is touched or dirty", async () => {
    const fixture = TestBed.createComponent(HostSignal);
    await settle(fixture);
    expect(fixture.componentInstance.f.name().invalid()).toBe(true);
    expect(errorText(fixture)).not.toContain("Name is required.");
  });

  it("renders a Signal Forms error message once touched", async () => {
    const fixture = TestBed.createComponent(HostSignal);
    await settle(fixture);

    fixture.componentInstance.f.name().markAsTouched();
    await settle(fixture);

    expect(errorText(fixture)).toContain("Name is required.");
  });

  it("renders every Signal Forms error, not just the first", async () => {
    const fixture = TestBed.createComponent(HostSignal);
    await settle(fixture);
    const host = fixture.componentInstance;

    // "ab" is non-empty (passes required) but too short.
    host.f.name().value.set("ab");
    host.f.name().markAsTouched();
    await settle(fixture);

    expect(errorText(fixture)).toContain("Name must be at least 5 characters.");
    expect(errorText(fixture)).not.toContain("Name is required.");
  });

  it("falls back to generated copy for a camelCase minLength error with no message", async () => {
    const fixture = TestBed.createComponent(HostSignalNoMessage);
    await settle(fixture);

    fixture.componentInstance.f.name().markAsTouched();
    await settle(fixture);

    // The trap: kind is `minLength` and the payload prop is `minLength` (not `requiredLength`).
    // A template that only knew the reactive spellings would render "Invalid value." here.
    expect(errorText(fixture)).toContain("Minimum length is 5 characters.");
    expect(errorText(fixture)).not.toContain("Invalid value.");
  });

  it("renders reactive-forms errors from the projected formControlName", async () => {
    const fixture = TestBed.createComponent(HostReactive);
    await settle(fixture);
    const control = fixture.componentInstance.fg.controls.name;

    control.markAsTouched();
    await settle(fixture);
    expect(errorText(fixture)).toContain("This field is required.");

    // Reactive spelling: kind `minlength`, payload prop `requiredLength`.
    control.setValue("ab");
    await settle(fixture);
    expect(errorText(fixture)).toContain("Minimum length is 5 characters.");
  });
});
