import {
  afterNextRender,
  booleanAttribute,
  Component,
  computed,
  DestroyRef,
  effect,
  ElementRef,
  inject,
  input,
  isDevMode,
  model,
  output,
  signal,
  untracked,
  viewChild,
} from "@angular/core";
import type { FormValueControl, ValidationError } from "@angular/forms/signals";

/** The subset of quill's API this component drives. Keeps `any` out of the file. */
interface QuillLike {
  root: HTMLElement;
  on(event: "text-change", handler: () => void): void;
  getSemanticHTML(): string;
  setContents(delta: unknown): void;
  clipboard: { convert(input: { html: string }): unknown };
  enable(enabled: boolean): void;
  hasFocus(): boolean;
}

/**
 * Rich-text editor. Replaces `<p-editor>` — one call site, the admin blog-post form.
 *
 * Implements `FormValueControl<string>` and nothing else, like every other `ui-*-field`. NOT a
 * `ControlValueAccessor`: that is the governing rule of this repo's form layer, and the Phase 7
 * exit gate greps for it. `[formField]="form.content"` binds straight to `value`.
 *
 * QUILL IS LOADED LAZILY (`await import("quill")`) and only when the component renders. It is ~43KB
 * min+gzip, it is needed on exactly one admin page, and a static import would put it in admin's
 * initial bundle for every user who never writes a blog post.
 *
 * Two consequences of lazy loading that the code below has to handle, and which are invisible until
 * they bite:
 *   1. `value` can be set (by the form, from the API) BEFORE quill exists. So the incoming value is
 *      held in a signal and flushed into quill once it resolves — see `syncIntoQuill`.
 *   2. The component can be destroyed while the `import()` is still in flight. Writing to a
 *      destroyed view then throws. `destroyed` guards the continuation.
 */
@Component({
  selector: "ui-editor",
  templateUrl: "./editor.html",
  host: { "[attr.id]": "null" },
})
export class UiEditor implements FormValueControl<string> {
  /** The control's value: an HTML string. Required by `FormValueControl`. */
  public readonly value = model<string>("");

  public readonly disabled = input(false, { transform: booleanAttribute });
  public readonly readonly = input(false, { transform: booleanAttribute });
  public readonly required = input(false, { transform: booleanAttribute });
  public readonly invalid = input(false, { transform: booleanAttribute });
  public readonly touched = input(false, { transform: booleanAttribute });
  public readonly dirty = input(false, { transform: booleanAttribute });
  public readonly errors = input<readonly ValidationError[]>([]);
  public readonly name = input<string>("");

  /** Raised on blur so the form can mark the field touched. */
  public readonly touch = output<void>();

  public readonly id = input<string>("");
  public readonly placeholder = input<string>("");

  /**
   * Height of the EDITABLE AREA — not of the whole component.
   *
   * `p-editor` took a `[style]` object and applied it, via `[ngStyle]`, to its content div only
   * (`<div [class]="cx('content')" [ngStyle]="style">`), with the toolbar stacked above and
   * contributing its own height. The blog form passed `[style]="{ height: '320px' }"`, so it asked
   * for a 320px *text area*, and the component rendered ~320px + toolbar.
   *
   * Forwarding that object to `<ui-editor>`'s host as a plain Angular style binding would have meant
   * 320px for toolbar AND text, silently shrinking the writing area. Hence an explicit input with an
   * unambiguous name instead of a `style` passthrough.
   */
  public readonly contentHeight = input<string>("320px");

  protected readonly showInvalid = computed(
    () => this.invalid() && (this.touched() || this.dirty()),
  );

  private readonly toolbarRef = viewChild.required<ElementRef<HTMLDivElement>>("toolbar");
  private readonly editorRef = viewChild.required<ElementRef<HTMLDivElement>>("editor");

  private quill: QuillLike | null = null;
  private destroyed = false;

  /**
   * The last HTML this component itself pushed into `value`. Quill normalises whatever you give it
   * (`<b>` -> `<strong>`, attribute order, whitespace), so echoing our own emission back into
   * `setContents` would reset the caret to the top of the document on every keystroke. Comparing
   * against what we emitted lets `syncIntoQuill` ignore the round trip.
   */
  private lastEmitted: string | null = null;

  /** Quill has resolved and is mounted. Flips once; drives the value flush. */
  private readonly ready = signal(false);

  constructor() {
    afterNextRender(() => void this.mount());

    effect(() => {
      const value = this.value();
      const ready = this.ready();
      if (!ready) {
        return;
      }
      untracked(() => this.syncIntoQuill(value));
    });

    effect(() => {
      const enabled = !this.disabled() && !this.readonly();
      const ready = this.ready();
      if (!ready) {
        return;
      }
      untracked(() => this.quill?.enable(enabled));
    });

    inject(DestroyRef).onDestroy(() => {
      this.destroyed = true;
      this.quill = null;
    });
  }

  private async mount(): Promise<void> {
    const { default: Quill } = await import("quill");
    if (this.destroyed) {
      return;
    }

    const quill = new Quill(this.editorRef().nativeElement, {
      theme: "snow",
      readOnly: this.disabled() || this.readonly(),
      placeholder: this.placeholder() || undefined,
      modules: { toolbar: this.toolbarRef().nativeElement },
    }) as unknown as QuillLike;

    quill.on("text-change", () => {
      // `getSemanticHTML()` is quill 2's HTML export. `root.innerHTML` would leak quill's own
      // bookkeeping (a trailing `<p><br></p>`, `ql-cursor` spans) into the persisted post body.
      const html = quill.getSemanticHTML();
      this.lastEmitted = html;
      this.value.set(html);
    });

    quill.root.addEventListener("blur", () => this.touch.emit());

    this.quill = quill;
    this.ready.set(true);
    this.assertStylesheetLoaded();

    // The form may have loaded an existing post while the import() was in flight.
    this.syncIntoQuill(this.value());
  }

  /**
   * Quill's stylesheet is a GLOBAL one (`shared/src/styles/quill.css`) — it is 24.6kB, which does not
   * fit the 4-6kB `anyComponentStyle` budget, so it cannot travel with the component. That leaves the
   * classic shared-component trap: the second app to render `<ui-editor>` forgets to import it, and
   * quill's `<select>` pickers, with nothing styling them, expand into raw option lists — a 5118px
   * toolbar. It is not subtle, but it is also not obviously ABOUT a missing stylesheet, so say so.
   */
  private assertStylesheetLoaded(): void {
    if (!isDevMode()) {
      return;
    }
    const toolbar = this.toolbarRef().nativeElement;
    const loaded = getComputedStyle(toolbar).getPropertyValue("--ui-editor-loaded").trim();
    if (loaded === "1") {
      return;
    }
    console.error(
      "[ui-editor] quill's stylesheet is not loaded, so the toolbar will render unstyled and " +
        'thousands of pixels tall. Add `@import "…/shared/src/styles/quill.css";` to this app\'s ' +
        "global styles.css (see the header of that file).",
    );
  }

  /** Push an externally-set value into quill, without fighting the user's caret. */
  private syncIntoQuill(value: string): void {
    const quill = this.quill;
    if (!quill || value === this.lastEmitted) {
      return;
    }
    this.lastEmitted = value;
    quill.setContents(quill.clipboard.convert({ html: value ?? "" }));
  }

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    this.editorRef().nativeElement.querySelector<HTMLElement>(".ql-editor")?.focus(options);
  }
}
