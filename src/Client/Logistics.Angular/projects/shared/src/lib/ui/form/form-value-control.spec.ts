/**
 * The `FormValueControl` contract that every `ui-*-field` wrapper rests on.
 * =========================================================================
 *
 * This is the surviving half of the old `signal-forms-compat-probe.spec.ts` (deleted in S13).
 *
 * That file pinned two DIFFERENT kinds of claim, and only one of them died with PrimeNG:
 *
 *   - PrimeNG interop claims — `pTextarea` + `[formField]` crashing on a missing `valueChanges`,
 *     `BaseInput.pattern` colliding with Signal Forms' `readonly RegExp[]`, and the legacy
 *     Reactive-Forms bridge that let a bare `FormValueControl` work under `formControlName`.
 *     Those describe a library this repo no longer depends on, and a form system it no longer
 *     uses. They are gone.
 *
 *   - Angular Signal Forms facts — the ones below. These have NOTHING to do with PrimeNG. They
 *     pin behaviour of `@angular/forms/signals` itself that the whole `ui-*-field` layer is built
 *     on, and they are exactly as load-bearing today as they were before. Deleting them along
 *     with the PrimeNG half would have silently dropped real coverage.
 *
 * WHAT IS PINNED
 * --------------
 *   A. A `FormValueControl`-only component (`value = model<T>()`) two-way syncs with `[formField]`.
 *      This is THE reason the wrappers need no value-accessor glue of any kind.
 *   B. `InteropNgControl.errors` returns the classic keyed `ValidationErrors` object
 *      ({ required: ... } | null), NOT a `ValidationError[]`. `ui-form-field` resolves its control
 *      via `contentChild(NgControl)` and reads `.errors`, so if this shape ever flips to an array,
 *      every inline field error in the app silently stops rendering.
 *
 * Plus three known `FormValueControl` sharp edges, whose ACTUAL behaviour on this exact Angular
 * version is asserted so we notice if upstream fixes or regresses them:
 *   - angular/angular#65478 — `value` is a `model()`, so a computed over it re-runs per change.
 *   - angular/angular#65576 — driving `value` externally while also `[formField]`-bound.
 *   - angular/angular#63625 — `min`/`max` state inputs + updating value.
 *
 * Environment note: the `shared` library test target runs under @angular/build:unit-test
 * (Vitest + jsdom). zone.js is absent (the app is zoneless), so we explicitly install
 * `provideZonelessChangeDetection()` in every TestBed module.
 */

import {
  ChangeDetectionStrategy,
  Component,
  computed,
  Directive,
  inject,
  input,
  model,
  output,
  provideZonelessChangeDetection,
  signal,
  viewChild,
} from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { NgControl } from "@angular/forms";
import { form, FormField, max, min, required, type FormValueControl } from "@angular/forms/signals";

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

/** A numeric control that also opts into the optional `min`/`max` state inputs. */
@Component({
  selector: "ui-minmax-probe",
  template: "",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class MinMaxProbe implements FormValueControl<number | null> {
  readonly value = model<number | null>(null);
  readonly min = input<number | undefined>(undefined);
  readonly max = input<number | undefined>(undefined);
}

/** A control with a computed derived from `value()` — used to probe angular#65478. */
@Component({
  selector: "ui-computed-probe",
  template: "",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class ComputedProbe implements FormValueControl<string> {
  readonly value = model<string>("");
  computeCount = 0;
  readonly derived = computed(() => {
    this.computeCount++;
    return (this.value() ?? "").toUpperCase();
  });
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

/** Sharp-edge angular#63625 host: min/max validators feeding a custom control's state inputs. */
@Component({
  selector: "ui-host-minmax",
  imports: [MinMaxProbe, FormField],
  template: `<ui-minmax-probe [formField]="f.age" />`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class HostMinMax {
  readonly data = signal<{ age: number | null }>({ age: 0 });
  readonly f = form(this.data, (p) => {
    min(p.age, 5);
    max(p.age, 10);
  });
  readonly probe = viewChild.required(MinMaxProbe);
}

/** Sharp-edge angular#65576 host: value driven by an external signal AND `[formField]`. */
@Component({
  selector: "ui-host-double-bind",
  imports: [FvcProbe, FormField],
  template: `<ui-fvc-probe [(value)]="external" [formField]="f.name" />`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class HostDoubleBind {
  readonly external = signal("external");
  readonly data = signal({ name: "field" });
  readonly f = form(this.data);
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

// ---------------------------------------------------------------------------
// Known FormValueControl sharp edges — assert ACTUAL behavior on this version.
// These do NOT fail on upstream bugs; they document current reality so drift is visible.
// ---------------------------------------------------------------------------

describe("FormValueControl sharp edges (record-and-notice)", () => {
  beforeEach(() => configure());

  it("angular#65478: a computed over value() recomputes per distinct change (value is a model)", () => {
    const fixture = TestBed.createComponent(ComputedProbe);
    const probe = fixture.componentInstance;

    // Lazy: first read computes once.
    expect(probe.derived()).toBe("");
    expect(probe.computeCount).toBe(1);

    // A distinct change + read recomputes ("every keystroke").
    probe.value.set("ab");
    expect(probe.derived()).toBe("AB");
    expect(probe.computeCount).toBe(2);

    // Re-reading without a change does NOT recompute (signal memoization holds).
    expect(probe.derived()).toBe("AB");
    expect(probe.computeCount).toBe(2);

    // Setting the SAME value does NOT recompute (model uses Object.is equality) — so the
    // "re-runs on every keystroke" cost is bounded to *distinct* values, not raw sets.
    probe.value.set("ab");
    expect(probe.derived()).toBe("AB");
    expect(probe.computeCount).toBe(2);

    // Two more distinct changes -> two more recomputes.
    probe.value.set("abc");
    probe.derived();
    probe.value.set("abcd");
    probe.derived();
    expect(probe.computeCount).toBe(4);
  });

  it("angular#65576: value driven externally AND by [formField] — record actual behavior", () => {
    let error: Error | null = null;
    let synced: string | null = null;
    try {
      const fixture = TestBed.createComponent(HostDoubleBind);
      fixture.detectChanges();
      synced = fixture.componentInstance.data().name;
    } catch (e) {
      error = e as Error;
    }

    // OBSERVED on Angular 22.0.6: this does NOT reproduce NG0318. Rendering succeeds and the
    // `[formField]` directive's ownership of `value` WINS over the external `[(value)]` signal —
    // the form model keeps the field's value ("field"), not the external "external". The historical
    // angular#65576 crash does not occur with this shape here; if a future version starts throwing
    // (or lets the external binding win instead), one of these assertions flips and we investigate.
    expect(error, "expected no NG0318 / no throw on this version").toBeNull();
    expect(synced, "the [formField] value should win over the external two-way binding").toBe(
      "field",
    );
  });

  it("angular#63625: min/max state inputs sync from validators and value updates re-validate", async () => {
    const fixture = TestBed.createComponent(HostMinMax);
    await settle(fixture);
    const host = fixture.componentInstance;
    const probe = host.probe();

    // The Field directive pushes the validator-derived min/max down into the control's state inputs.
    expect(probe.min()).toBe(5);
    expect(probe.max()).toBe(10);

    // value 0 < min(5) -> invalid.
    expect(host.f.age().valid()).toBe(false);

    // Update to an in-range value -> valid, and value propagates to the control model.
    host.f.age().value.set(7);
    await settle(fixture);
    expect(host.f.age().valid()).toBe(true);
    expect(probe.value()).toBe(7);

    // Above max(10) -> invalid again.
    host.f.age().value.set(99);
    await settle(fixture);
    expect(host.f.age().valid()).toBe(false);
  });
});
