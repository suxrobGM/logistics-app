/**
 * Signal Forms — Angular 22 API surface probe
 * ===========================================
 *
 * Pins the facts about the Signal Forms API itself that `ValidatedForm`, `ui-form-field`, and the
 * form wrappers depend on. Several contradict what a reader would assume coming from Reactive Forms,
 * and every one of them is invisible to `build:all`:
 *
 *   F. `submit()` marks the whole tree touched BEFORE checking validity, guards re-entrancy,
 *      drives `submitting`, and skips `action` when invalid (running `onInvalid` instead).
 *      => `ValidatedForm`'s `markAllAsTouched()` is redundant under Signal Forms.
 *   G. `submit()` maps the errors its `action` returns back onto individual fields.
 *      => server-side validation errors need no bespoke plumbing.
 *   H. `<form [formRoot]>` sets `novalidate` itself and calls `submit()` on the submit event
 *      when the form declares `submission` options. No `(ngSubmit)`, no manual `novalidate`.
 *   I. Signal Forms applies **no** `ng-invalid` / `ng-touched` / `ng-dirty` classes by default.
 *      They are opt-in via `provideSignalFormsConfig({classes})`.
 *      => any CSS or querySelector keyed on `.ng-invalid` silently stops matching.
 *   J. Built-in error `kind`s are camelCase: `minLength` / `maxLength` — NOT the classic
 *      reactive-forms keys `minlength` / `maxlength`. And the payload prop is `minLength`,
 *      not `requiredLength`. `InteropNgControl.errors` keys by raw `kind`, so a legacy
 *      `errors['minlength'].requiredLength` lookup returns `undefined` and the UI silently
 *      falls through to a generic message.
 *   K. `FieldState.reset(value?)` exists: it clears touched/dirty and, only if a value is
 *      passed, sets the value. It does NOT restore an initial value on its own.
 *   L. `focusBoundControl()` delegates to a custom control's optional `focus()` method; with
 *      no `focus()` it calls `.focus()` on the directive's host element (a no-op for a
 *      non-focusable custom element host).
 *
 * Environment note: Vitest + jsdom, zoneless, so every TestBed module installs
 * `provideZonelessChangeDetection()`.
 */

import {
  ChangeDetectionStrategy,
  Component,
  model,
  provideZonelessChangeDetection,
  signal,
  viewChild,
  type ElementRef,
} from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { NgControl } from "@angular/forms";
import {
  form,
  FormField,
  FormRoot,
  maxLength,
  minLength,
  required,
  submit,
  type FormValueControl,
} from "@angular/forms/signals";

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

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
// Test doubles
// ---------------------------------------------------------------------------

/** A FormValueControl that records whether Signal Forms called its optional `focus()` hook. */
@Component({
  selector: "ui-focusable-probe",
  template: "",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class FocusableProbe implements FormValueControl<string> {
  readonly value = model<string>("");
  focusCalls = 0;
  focus(): void {
    this.focusCalls++;
  }
}

@Component({
  selector: "ui-host-focus",
  imports: [FocusableProbe, FormField],
  template: `<ui-focusable-probe [formField]="f.name" />`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class HostFocus {
  readonly data = signal({ name: "" });
  readonly f = form(this.data, (p) => required(p.name));
  readonly probe = viewChild.required(FocusableProbe);
}

/** Native input + [formField], used to observe status classes and the InteropNgControl errors. */
@Component({
  selector: "ui-host-classes",
  imports: [FormField],
  template: `<input #el [formField]="f.name" />`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class HostClasses {
  readonly data = signal({ name: "" });
  readonly f = form(this.data, (p) => {
    required(p.name);
    minLength(p.name, 5);
  });
  readonly el = viewChild.required<ElementRef<HTMLInputElement>>("el");
}

/** `<form [formRoot]>` with declared submission options. */
@Component({
  selector: "ui-host-root",
  imports: [FormRoot, FormField],
  template: `
    <form [formRoot]="f">
      <input [formField]="f.name" />
      <button type="submit">Go</button>
    </form>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class HostRoot {
  readonly data = signal({ name: "" });
  actionRuns = 0;
  invalidRuns = 0;
  readonly f = form(this.data, (p) => required(p.name), {
    submission: {
      action: async () => {
        this.actionRuns++;
        return undefined;
      },
      onInvalid: () => {
        this.invalidRuns++;
      },
    },
  });
}

// ---------------------------------------------------------------------------
// Claims
// ---------------------------------------------------------------------------

describe("Signal Forms v22 API probe", () => {
  beforeEach(() => configure());

  it("F: submit() marks the tree touched even when invalid, and skips the action", async () => {
    const data = signal({ name: "", note: "" });
    // `form()` calls `inject()`, so it needs an injection context when built outside a component.
    const f = TestBed.runInInjectionContext(() => form(data, (p) => required(p.name)));

    expect(f.name().touched()).toBe(false);
    expect(f.note().touched()).toBe(false);

    let actionRuns = 0;
    let invalidRuns = 0;
    const ok = await submit(f, {
      action: async () => {
        actionRuns++;
        return undefined;
      },
      onInvalid: () => {
        invalidRuns++;
      },
    });

    expect(ok, "an invalid submit resolves false").toBe(false);
    expect(actionRuns, "action must NOT run for an invalid form").toBe(0);
    expect(invalidRuns, "onInvalid must run instead").toBe(1);

    // The load-bearing bit: submit() touches EVERY field, including valid ones, so inline
    // `ui-form-field` errors reveal themselves. ValidatedForm.markAllAsTouched() is redundant.
    expect(f.name().touched(), "submit() marks invalid fields touched").toBe(true);
    expect(f.note().touched(), "submit() marks *all* fields touched, not just invalid ones").toBe(
      true,
    );
  });

  it("F: submit() runs the action when valid and resolves true", async () => {
    const data = signal({ name: "ok" });
    const f = TestBed.runInInjectionContext(() => form(data, (p) => required(p.name)));

    let actionRuns = 0;
    const ok = await submit(f, async () => {
      actionRuns++;
      return undefined;
    });

    expect(ok).toBe(true);
    expect(actionRuns).toBe(1);
  });

  it("G: submit() maps errors returned by the action back onto the named field", async () => {
    const data = signal({ username: "god" });
    const f = TestBed.runInInjectionContext(() => form(data));

    const ok = await submit(f, async (root) => {
      // Returning a ValidationError targeted at a field attaches it to that field.
      return [{ kind: "server", message: "Username already taken", fieldTree: root.username }];
    });

    expect(ok, "a submit whose action returns errors resolves false").toBe(false);
    const errors = f.username().errors();
    expect(errors.length).toBe(1);
    expect(errors[0].kind).toBe("server");
    expect(errors[0].message).toBe("Username already taken");
    expect(f.username().invalid()).toBe(true);
  });

  it("H: <form [formRoot]> sets novalidate and submits via the form's submission options", async () => {
    const fixture = TestBed.createComponent(HostRoot);
    await settle(fixture);
    const host = fixture.componentInstance;

    const formEl = fixture.nativeElement.querySelector("form") as HTMLFormElement;
    expect(formEl.hasAttribute("novalidate"), "FormRoot sets novalidate itself").toBe(true);

    // Invalid: dispatching submit runs onInvalid, not action.
    formEl.dispatchEvent(new Event("submit", { cancelable: true, bubbles: true }));
    await settle(fixture);
    expect(host.actionRuns).toBe(0);
    expect(host.invalidRuns).toBe(1);
    expect(host.f.name().touched(), "the submit event marked the tree touched").toBe(true);

    // Valid: the action runs.
    host.f.name().value.set("filled");
    await settle(fixture);
    formEl.dispatchEvent(new Event("submit", { cancelable: true, bubbles: true }));
    await settle(fixture);
    expect(host.actionRuns).toBe(1);
  });

  it("I: no ng-invalid / ng-touched / ng-dirty classes are applied by default", async () => {
    const fixture = TestBed.createComponent(HostClasses);
    await settle(fixture);
    const host = fixture.componentInstance;

    host.f.name().markAsTouched();
    host.f.name().markAsDirty();
    await settle(fixture);

    expect(host.f.name().invalid()).toBe(true);
    expect(host.f.name().touched()).toBe(true);

    const el = host.el().nativeElement;
    const classes = Array.from(el.classList);
    // Signal Forms ships NO status classes unless provideSignalFormsConfig({classes}) opts in.
    // Anything querying `.ng-invalid` (like the old ValidatedForm) matches nothing.
    expect(
      classes,
      `expected no ng-* classes, got: ${classes.join(" ") || "(none)"}`,
    ).not.toContain("ng-invalid");
    expect(classes).not.toContain("ng-touched");
    expect(classes).not.toContain("ng-dirty");
    expect(el.matches(".ng-invalid")).toBe(false);
  });

  it("J: built-in error kinds are camelCase (minLength), not the classic minlength", async () => {
    const data = signal({ name: "ab" });
    const f = TestBed.runInInjectionContext(() =>
      form(data, (p) => {
        minLength(p.name, 5);
        maxLength(p.name, 10);
      }),
    );

    const errors = f.name().errors();
    expect(errors.length).toBe(1);
    const err = errors[0] as { kind: string; minLength?: number; requiredLength?: number };

    expect(err.kind, "the kind is camelCase minLength").toBe("minLength");
    expect(err.kind).not.toBe("minlength");

    // Payload prop is `minLength`, NOT reactive forms' `requiredLength`.
    expect(err.minLength).toBe(5);
    expect(err.requiredLength, "there is no requiredLength prop").toBeUndefined();
  });

  it("J: InteropNgControl.errors keys by the raw camelCase kind, breaking legacy lookups", async () => {
    const fixture = TestBed.createComponent(HostClasses);
    await settle(fixture);
    const host = fixture.componentInstance;

    host.f.name().value.set("ab"); // non-empty but shorter than minLength(5)
    await settle(fixture);

    const ngControl = fixture.debugElement
      .query((de) => de.nativeElement.tagName === "INPUT")
      .injector.get(NgControl);
    const errors = ngControl.errors as Record<string, unknown> | null;

    expect(errors).not.toBeNull();
    // THE TRAP: `ui-form-field` (and any reactive-forms-era template) checks `errors['minlength']`.
    expect(errors!["minLength"], "Signal Forms key").toBeTruthy();
    expect(errors!["minlength"], "classic reactive-forms key is ABSENT").toBeUndefined();
  });

  it("K: FieldState.reset() clears touched/dirty but does not restore a value unless given one", async () => {
    const data = signal({ name: "initial" });
    const f = TestBed.runInInjectionContext(() => form(data, (p) => required(p.name)));

    f.name().value.set("edited");
    f.name().markAsTouched();
    f.name().markAsDirty();
    expect(f.name().touched()).toBe(true);
    expect(f.name().dirty()).toBe(true);

    f().reset();
    expect(f.name().touched(), "reset() clears touched").toBe(false);
    expect(f.name().dirty(), "reset() clears dirty").toBe(false);
    expect(f.name().value(), "reset() does NOT restore the initial value").toBe("edited");

    // Passing a value is how you get reactive-forms' reset(initial) semantics.
    f().reset({ name: "initial" });
    expect(f.name().value()).toBe("initial");
    expect(data(), "the model signal is the source of truth and is written through").toEqual({
      name: "initial",
    });
  });

  it("L: focusBoundControl() calls a custom control's optional focus() hook", async () => {
    const fixture = TestBed.createComponent(HostFocus);
    await settle(fixture);
    const host = fixture.componentInstance;

    expect(host.probe().focusCalls).toBe(0);
    host.f.name().focusBoundControl();
    expect(
      host.probe().focusCalls,
      "a FormValueControl that implements focus() receives focusBoundControl()",
    ).toBe(1);
  });
});
