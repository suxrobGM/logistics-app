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
 * Rich-text editor, used by the admin blog-post form.
 *
 * Implements `FormValueControl<string>` and nothing else, never a legacy `ControlValueAccessor`:
 * `[formField]="form.content"` binds straight to `value`.
 *
 * Quill is loaded lazily (`await import("quill")`) when the component renders — it is ~43KB min+gzip
 * for one admin page, so a static import would land in admin's initial bundle for everyone. Two
 * consequences the code has to handle: `value` can be set before quill exists (held in a signal and
 * flushed by `syncIntoQuill` once it resolves), and the component can be destroyed mid-`import()`
 * (writing to the destroyed view throws, so `destroyed` guards the continuation).
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

  /** Height of the editable area only — the toolbar stacks above it and adds its own height. */
  public readonly contentHeight = input<string>("320px");

  protected readonly showInvalid = computed(
    () => this.invalid() && (this.touched() || this.dirty()),
  );

  private readonly toolbarRef = viewChild.required<ElementRef<HTMLDivElement>>("toolbar");
  private readonly editorRef = viewChild.required<ElementRef<HTMLDivElement>>("editor");

  private quill: QuillLike | null = null;
  private destroyed = false;

  /**
   * The last HTML this component pushed into `value`. Quill normalises what you give it (`<b>` ->
   * `<strong>`, attribute order, whitespace), so feeding our own emission back into `setContents`
   * would reset the caret to the top of the document on every keystroke; `syncIntoQuill` compares
   * against this to ignore the round trip.
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
   * Quill's stylesheet is global (`shared/src/styles/quill.css`): at 24.6kB it blows the
   * `anyComponentStyle` budget, so it cannot travel with the component and each app must import it.
   * When an app forgets, quill's `<select>` pickers expand into raw option lists and the toolbar
   * renders thousands of pixels tall — a symptom that does not point at a missing stylesheet.
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
