/**
 * `ui-form-field` renders inline validation errors for a projected `[formField]`.
 *
 * It resolves the field through the public `FORM_FIELD` token and reads `FieldState.errors()`,
 * which is a `ValidationError[]` — each error carrying its own `message` when the validator
 * supplied one.
 *
 * The trap this guards: Signal Forms' built-in error `kind`s are camelCase (`minLength`) with a
 * `minLength` payload prop, NOT reactive forms' `minlength` + `requiredLength`. A template written
 * against the reactive spellings renders nothing while still (correctly) marking the field invalid.
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

/** A signal form whose validator supplies NO message — exercises the generic fallback copy. */
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

  it("falls back to a generic message when a validator supplies none", async () => {
    const fixture = TestBed.createComponent(HostSignalNoMessage);
    await settle(fixture);

    fixture.componentInstance.f.name().markAsTouched();
    await settle(fixture);

    // There is no per-`kind` message table — a message-less error renders one generic sentence,
    // regardless of its `kind`.
    expect(errorText(fixture)).toContain("This field is invalid.");
  });
});
