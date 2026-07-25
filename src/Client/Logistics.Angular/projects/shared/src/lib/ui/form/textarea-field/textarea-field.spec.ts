/**
 * Proves the wrapper contract that the whole `ui-*-field` layer rests on.
 *
 * `UiTextareaField` implements ONLY `FormValueControl` - no value-accessor glue of any kind. It must work:
 *   1. under Signal Forms `[formField]`,
 *   2. inside `<ui-form-field>`, whose `contentChild(NgControl)` must still resolve and
 *      render validation errors.
 *
 * If any of these break, every `ui-*-field` wrapper breaks with them.
 */
import { Component, provideZonelessChangeDetection, signal, viewChild } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { disabled, form, FormField, required } from "@angular/forms/signals";
import { UiFormField } from "../form-field/form-field";
import { UiTextareaField } from "./textarea-field";

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
  /** Flips the schema's disabled rule - proves the wrapper REACTS, not just reads once. */
  readonly lock = signal(false);
  readonly model = signal({ notes: "initial" });
  readonly f = form(this.model, (p) => {
    required(p.notes, { message: "This field is required." });
    // Reactive disabled rule - the shape every real form uses:
    //   disabled(p.truckId, { when: () => this.mode() === "edit" })
    disabled(p.notes, { when: () => this.lock() });
  });
  readonly field = viewChild.required(UiTextareaField);
}

/** The counter is presentation only, so this host skips the Signal Forms wiring. */
@Component({
  selector: "ui-host-counter-textarea",
  imports: [UiTextareaField],
  template: `
    <ui-textarea-field
      [(value)]="text"
      [maxlength]="max()"
      [showCounter]="show()"
      [counterWarnAt]="warnAt()"
    />
  `,
})
class HostCounterTextarea {
  readonly text = signal("");
  readonly max = signal<number | null>(500);
  readonly show = signal(true);
  readonly warnAt = signal(50);
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

describe("UiTextareaField - a FormValueControl-only wrapper", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("renders the textarea and reflects the initial value", async () => {
    const fixture = TestBed.createComponent(HostSignalTextarea);
    await settle(fixture);
    expect(textarea(fixture).value).toBe("initial");
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

    it("ui-form-field renders the required error once touched - with NO transitional code", async () => {
      const fixture = TestBed.createComponent(HostSignalTextarea);
      await settle(fixture);

      type(fixture, "");
      // blur raises `touch`, which Signal Forms uses to mark the field touched
      textarea(fixture).dispatchEvent(new Event("blur"));
      await settle(fixture);

      expect(fixture.componentInstance.f.notes().invalid()).toBe(true);
      expect(fixture.nativeElement.textContent).toContain("This field is required.");
    });
    /**
     * REGRESSION GUARD. `disabled` is a first-class part of the FormValueControl contract and is
     * load-bearing in production (`disabled(p.salary, { when: ... })` in employee-edit,
     * `disabled(p.truckId, ...)` in the expense forms, tax-rates, timesheets, ...).
     *
     * It used to be covered ONLY by the legacy Reactive-Forms host ("propagates disabled state from
     * the control"). That host was deleted, and the assertion went with it - leaving the
     * whole `disabled` dimension of all 10 wrappers untested. This is its Signal Forms twin.
     */
    it("propagates the schema's disabled rule to the control - and reacts when it flips", async () => {
      const fixture = TestBed.createComponent(HostSignalTextarea);
      await settle(fixture);

      expect(fixture.componentInstance.f.notes().disabled()).toBe(false);
      expect(fixture.componentInstance.field().disabled()).toBe(false);
      expect(textarea(fixture).disabled).toBe(false);

      fixture.componentInstance.lock.set(true);
      await settle(fixture);

      expect(fixture.componentInstance.f.notes().disabled()).toBe(true);
      expect(fixture.componentInstance.field().disabled()).toBe(true);
      expect(textarea(fixture).disabled).toBe(true);
    });
  });

  /**
   * Replaced three hand-rolled copies, one of which had drifted onto a hardcoded `text-red-500`
   * with no dark counterpart. The arithmetic and the token live here now.
   */
  describe("remaining-characters counter", () => {
    function counter(fixture: ComponentFixture<unknown>): HTMLElement | null {
      return fixture.nativeElement.querySelector("ui-textarea-field > div");
    }

    it("is opt-in and counts down from maxlength", async () => {
      const fixture = TestBed.createComponent(HostCounterTextarea);
      await settle(fixture);
      expect(counter(fixture)?.textContent?.trim()).toBe("500 / 500");

      type(fixture, "hello");
      await settle(fixture);
      expect(counter(fixture)?.textContent?.trim()).toBe("495 / 500");

      fixture.componentInstance.show.set(false);
      await settle(fixture);
      expect(counter(fixture)).toBeNull();
    });

    it("stays hidden without a maxlength to count against", async () => {
      const fixture = TestBed.createComponent(HostCounterTextarea);
      fixture.componentInstance.max.set(null);
      await settle(fixture);

      expect(counter(fixture)).toBeNull();
    });

    it("switches to the destructive token at the warn threshold - never a raw palette colour", async () => {
      const fixture = TestBed.createComponent(HostCounterTextarea);
      await settle(fixture);
      expect(counter(fixture)?.className).toContain("text-muted-foreground");

      // 460 typed leaves 40 remaining, under the warnAt of 50.
      type(fixture, "x".repeat(460));
      await settle(fixture);

      expect(counter(fixture)?.className).toContain("text-destructive");
      expect(counter(fixture)?.className).not.toContain("text-red");
    });

    it("honours a caller's warn threshold", async () => {
      const fixture = TestBed.createComponent(HostCounterTextarea);
      fixture.componentInstance.warnAt.set(200);
      await settle(fixture);

      type(fixture, "x".repeat(250));
      await settle(fixture);
      expect(counter(fixture)?.className).toContain("text-muted-foreground");

      type(fixture, "x".repeat(350));
      await settle(fixture);
      expect(counter(fixture)?.className).toContain("text-destructive");
    });
  });
});
