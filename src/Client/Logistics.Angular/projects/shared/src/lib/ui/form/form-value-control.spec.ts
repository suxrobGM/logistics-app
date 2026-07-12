/**
 * The `FormValueControl` contract that every `ui-*-field` wrapper rests on.
 * =========================================================================
 *
 * These pin behaviour of `@angular/forms/signals` that the whole `ui-*-field` layer is built on and
 * that `build:all` cannot see:
 *
 *   A. A `FormValueControl`-only component (`value = model<T>()`) two-way syncs with `[formField]`.
 *      This is THE reason the wrappers need no value-accessor glue of any kind.
 *   B. `InteropNgControl.errors` returns the classic keyed `ValidationErrors` object
 *      ({ required: ... } | null), NOT a `ValidationError[]`. `ui-form-field` resolves its control
 *      via `contentChild(NgControl)` and reads `.errors`, so if this shape ever flips to an array,
 *      every inline field error in the app silently stops rendering.
 *
 * Environment note: the `shared` library test target runs under @angular/build:unit-test
 * (Vitest + jsdom). zone.js is absent (the app is zoneless), so we explicitly install
 * `provideZonelessChangeDetection()` in every TestBed module.
 */

import {
  ChangeDetectionStrategy,
  Component,
  Directive,
  inject,
  model,
  output,
  provideZonelessChangeDetection,
  signal,
  viewChild,
} from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { NgControl } from "@angular/forms";
import { form, FormField, required, type FormValueControl } from "@angular/forms/signals";

// ---------------------------------------------------------------------------
// Test-double controls — pure FormValueControl implementations, nothing else.
// ---------------------------------------------------------------------------

/** A string control implementing ONLY `FormValueControl<string>`. No shims of any kind. */
@Component({
  selector: "ui-fvc-probe",
  template: "",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class FvcProbe implements FormValueControl<string> {
  readonly value = model<string>("");
  readonly touch = output<void>();
}

/** Captures the `NgControl` that a sibling `[formField]` provides on a native input. */
@Directive({ selector: "[uiCaptureNgControl]" })
class CaptureNgControl {
  readonly ngControl = inject(NgControl, { optional: true, self: true });
}

// ---------------------------------------------------------------------------
// Host components
// ---------------------------------------------------------------------------

/** Claim A host: FormValueControl bound with Signal Forms `[formField]`. */
@Component({
  selector: "ui-host-formfield",
  imports: [FvcProbe, FormField],
  template: `<ui-fvc-probe [formField]="f.name" />`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class HostFormField {
  readonly data = signal({ name: "initial" });
  readonly f = form(this.data);
  readonly probe = viewChild.required(FvcProbe);
}

/** Claim B host: a native input carrying `[formField]`, with the InteropNgControl captured. */
@Component({
  selector: "ui-host-capture",
  imports: [FormField, CaptureNgControl],
  template: `<input uiCaptureNgControl [formField]="f.name" />`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class HostCapture {
  readonly data = signal({ name: "" });
  readonly f = form(this.data, (p) => {
    required(p.name);
  });
  readonly capture = viewChild.required(CaptureNgControl);
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/** Flush zoneless change detection + pending effects, then settle again. */
async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

function configure(): void {
  TestBed.configureTestingModule({
    providers: [provideZonelessChangeDetection()],
  });
}

// ---------------------------------------------------------------------------
// Claims
// ---------------------------------------------------------------------------

describe("FormValueControl — the contract every ui-*-field wrapper implements", () => {
  beforeEach(() => configure());

  it("A: a FormValueControl-only component two-way syncs with [formField]", async () => {
    const fixture = TestBed.createComponent(HostFormField);
    const host = fixture.componentInstance;
    await settle(fixture);
    const probe = host.probe();

    // field -> model, on initialization
    expect(probe.value()).toBe("initial");

    // field -> model, on later change
    host.f.name().value.set("from-field");
    await settle(fixture);
    expect(probe.value()).toBe("from-field");

    // model -> field, on user (model) change
    probe.value.set("from-model");
    await settle(fixture);
    expect(host.f.name().value()).toBe("from-model");
  });

  it("B: InteropNgControl.errors is the classic ValidationErrors object, not ValidationError[]", async () => {
    // `ui-form-field` resolves its control via `contentChild(NgControl)` and reads `.errors`.
    // For Signal Forms that NgControl is InteropNgControl, whose `errors` getter runs
    // `signalErrorsToValidationErrors(...)` -> `{ [kind]: error } | null`. Object shape, not array.
    const fixture = TestBed.createComponent(HostCapture);
    await settle(fixture);
    const ngControl = fixture.componentInstance.capture().ngControl as NgControl;

    const errors = ngControl.errors;
    expect(errors, "an empty required() field must report an error").not.toBeNull();
    expect(Array.isArray(errors), "errors must be a keyed object, not an array").toBe(false);
    expect(typeof errors).toBe("object");
    expect(errors).toHaveProperty("required");
  });
});
