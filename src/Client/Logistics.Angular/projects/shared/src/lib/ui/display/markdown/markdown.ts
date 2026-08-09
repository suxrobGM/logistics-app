import { Component, input } from "@angular/core";
import { MarkdownPipe } from "../../../pipes/markdown.pipe";

/**
 * Renders markdown text. Single owner of the `prose` class string, which used to be copied into
 * every consumer and drifted between them. {@link MarkdownPipe} sanitizes, so `[innerHTML]` never
 * sees raw model output.
 *
 * `scale="chat"` rescales prose headings and margins to a chat pane - document-scale h1/h2
 * dwarf it. The default stays neutral so a new consumer does not inherit chat sizing by accident.
 */
@Component({
  selector: "ui-markdown",
  templateUrl: "./markdown.html",
  host: { class: "block min-w-0" },
  imports: [MarkdownPipe],
})
export class Markdown {
  public readonly content = input<string | null | undefined>(null);
  public readonly scale = input<"chat" | "document">("document");
  /** Collapses to three lines, for reasoning blocks that expand on click. */
  public readonly clamped = input(false);
}
