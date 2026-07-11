/**
 * Signal Forms <-> PrimeNG compatibility probe
 * ============================================
 *
 * WHAT THIS GUARDS
 * ----------------
 * Our PrimeNG-21 -> spartan/ui migration is gated on a handful of Angular 22 *stable*
 * Signal Forms facts. Each of them is an assumption baked into the migration roadmap and
 * into `ui-form-field`. If any of these silently changes in a future Angular/PrimeNG bump,
 * the wrapper strategy breaks in subtle, runtime-only ways. This executable spec pins the
 * facts down so the CI build screams the moment they drift.
 *
 * This file intentionally implements ONLY the standard Signal Forms contracts
 * (`FormValueControl`, i.e. `value = model<T>()`). There are NO shims, NO
 * `ControlValueAccessor`, NO compat layers here — the whole point is to prove the wrapper
 * strategy needs none.
 *
 * CLAIMS PINNED (see individual `it(...)` blocks):
 *   A. A `FormValueControl`-only component two-way syncs with `[formField]` (Signal Forms).
 *   B. The SAME component also works under legacy Reactive Forms `formControlName`,
 *      with NO extra compatibility code (the load-bearing Angular 22 bridge). If B fails,
 *      the whole wrapper-first plan is dead — the test says so loudly.
 *   C. `pTextarea` + `[formField]` CRASHES, because PrimeNG's Textarea subscribes to
 *      `ngControl.valueChanges` in `onInit`, and Signal Forms hands it an
 *      `InteropNgControl` that has no `valueChanges`.
 *   D. `InteropNgControl.errors` returns the classic `ValidationErrors` object shape
 *      ({ required: ... } | null), NOT a `ValidationError[]`. This is why `ui-form-field`
 *      (which does `contentChild(NgControl)` and reads `.errors`) needs no changes.
 *   E. The `pattern` collision: Signal Forms binds `pattern` as `readonly RegExp[]`, while
 *      PrimeNG's `BaseInput` declares `pattern: string`. This is a compile-time (TS2322)
 *      collision, so it is guarded with a type-level assertion against PrimeNG's own type.
 *
 * Plus three known `FormValueControl` sharp edges are probed and their ACTUAL behavior on
 * this exact version is asserted (so we notice if upstream fixes/regresses them):
 *   - angular/angular#65478 — value is a `model()`, so a computed over it re-runs per change.
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
  type InputSignal,
} from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { FormControl, FormGroup, NgControl, ReactiveFormsModule } from "@angular/forms";
import { form, FormField, max, min, required, type FormValueControl } from "@angular/forms/signals";
import type { BaseInput } from "primeng/baseinput";
import { Textarea } from "primeng/textarea";

// ---------------------------------------------------------------------------
// Test-double controls — pure FormValueControl implementations, nothing else.
// ---------------------------------------------------------------------------

/** A string control implementing ONLY `FormValueControl<string>`. No CVA, no shims. */
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

/** Claim B host: the SAME FormValueControl bound with legacy Reactive Forms. */
@Component({
  selector: "ui-host-reactive",
  imports: [FvcProbe, ReactiveFormsModule],
  template: `
    <form [formGroup]="fg">
      <ui-fvc-probe formControlName="name" />
    </form>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class HostReactive {
  readonly fg = new FormGroup({
    name: new FormControl("initial", { nonNullable: true }),
  });
  readonly probe = viewChild.required(FvcProbe);
}

/** Claim C host: PrimeNG Textarea directive on a native textarea + `[formField]`. */
@Component({
  selector: "ui-host-textarea",
  imports: [Textarea, FormField],
  template: `<textarea pTextarea [formField]="f.note"></textarea>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class HostTextarea {
  readonly data = signal({ note: "" });
  readonly f = form(this.data);
}

/** Claims C/D host: a native input carrying `[formField]`, with the InteropNgControl captured. */
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

describe("Signal Forms <-> PrimeNG compat probe", () => {
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

  it("B: the SAME component works under legacy Reactive Forms formControlName (Angular 22 bridge)", async () => {
    // If this throws "No value accessor for form control", the compiler-assisted custom-control
    // bridge (NgControl.ngControlCreate -> host.customControl) is gone. That would be CRITICAL:
    // every planned PrimeNG wrapper relies on a bare FormValueControl working under reactive forms
    // with zero CVA glue. The failure message below makes that unmistakable.
    let fixture: ComponentFixture<HostReactive>;
    try {
      fixture = TestBed.createComponent(HostReactive);
      await settle(fixture);
    } catch (e) {
      throw new Error(
        "CRITICAL FINDING: a bare FormValueControl no longer bridges to Reactive Forms " +
          "`formControlName` without a ControlValueAccessor. The entire wrapper-first PrimeNG " +
          "migration strategy depends on this Angular 22 behavior. Underlying error: " +
          (e as Error)?.message,
      );
    }

    const host = fixture.componentInstance;
    const probe = host.probe();

    expect(
      probe.value(),
      "control -> model init sync failed: custom-control bridge is broken",
    ).toBe("initial");

    host.fg.controls.name.setValue("from-control");
    await settle(fixture);
    expect(probe.value(), "control -> model sync failed: custom-control bridge is broken").toBe(
      "from-control",
    );

    probe.value.set("from-model");
    await settle(fixture);
    expect(
      host.fg.controls.name.value,
      "model -> control sync failed: custom-control bridge is broken",
    ).toBe("from-model");
  });

  it("C: pTextarea + [formField] crashes (PrimeNG subscribes to a non-existent valueChanges)", () => {
    // PrimeNG Textarea.onInit(): `this.ngControl.valueChanges.subscribe(...)`.
    // FormField provides NgControl as InteropNgControl, which has no `valueChanges` member,
    // so `undefined.subscribe(...)` throws during first change detection.
    // NB: the crash fires in ngOnInit, i.e. on the FIRST change detection only — so the throwing
    // call must be the first detectChanges() on a freshly created fixture.
    const fixture = TestBed.createComponent(HostTextarea);
    expect(() => fixture.detectChanges()).toThrow(/subscribe|valueChanges/);
  });

  it("C (root cause): the InteropNgControl handed to PrimeNG has no valueChanges", async () => {
    const fixture = TestBed.createComponent(HostCapture);
    await settle(fixture);
    const ngControl = fixture.componentInstance.capture().ngControl;

    expect(ngControl).toBeTruthy();
    expect("valueChanges" in (ngControl as object)).toBe(false);
    expect((ngControl as NgControl).valueChanges).toBeUndefined();
  });

  it("D: InteropNgControl.errors is the classic ValidationErrors object, not ValidationError[]", async () => {
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

  it("E: PrimeNG BaseInput.pattern is typed as string, colliding with Signal Forms readonly RegExp[]", () => {
    // Signal Forms binds `pattern` as `InputSignal<readonly RegExp[]>` (FormUiControl), but PrimeNG's
    // BaseInput declares `pattern: InputSignal<string | null | undefined>`. Under strictTemplates this
    // is a TS2322 template-type error, so it cannot be observed at runtime. Six PrimeNG components
    // extend BaseInput and therefore inherit this collision:
    //   Select, InputNumber, DatePicker, AutoComplete, InputMask, Password.
    // (verified: each `primeng-<name>.d.ts` declares `extends BaseInput<...>`.)
    //
    // Guard #1 (compile-time): the type-level assertion below fails to COMPILE — breaking this
    // test's build — the moment upstream changes BaseInput['pattern'] to anything but string.
    type UnwrapInputSignal<T> = T extends InputSignal<infer V> ? V : never;
    type BaseInputPatternType = UnwrapInputSignal<BaseInput["pattern"]>;
    type Equal<A, B> =
      (<T>() => T extends A ? 1 : 2) extends <T>() => T extends B ? 1 : 2 ? true : false;
    type Expect<T extends true> = T;
    // Intentionally unreferenced: the guard IS the declaration. If upstream changes
    // BaseInput['pattern'], `Expect<...>` fails its `T extends true` constraint and this file stops
    // compiling. Referencing it would add nothing, so the unused-vars rule is wrong here.
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    type _AssertPatternIsString = Expect<Equal<BaseInputPatternType, string | null | undefined>>;
    const patternIsString: BaseInputPatternType = "" as string | null | undefined;
    expect(typeof patternIsString).toBe("string");

    // NOTE: a runtime read of node_modules/primeng/types/primeng-baseinput.d.ts was considered as a
    // second guard, but the `shared` library spec tsconfig does not include @types/node (types is
    // just ["vitest/globals"]), so `node:fs` will not type-check here. Rather than diverge that
    // tsconfig, we rely on the compile-time assertion above — it is enforced on every test build.
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
