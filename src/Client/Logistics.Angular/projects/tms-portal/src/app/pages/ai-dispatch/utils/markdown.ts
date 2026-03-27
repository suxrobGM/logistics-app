import { Pipe, type PipeTransform, inject } from "@angular/core";
import { DomSanitizer, type SafeHtml } from "@angular/platform-browser";
import { marked } from "marked";

@Pipe({ name: "markdown" })
export class MarkdownPipe implements PipeTransform {
  private readonly sanitizer = inject(DomSanitizer);

  transform(value?: string | null): SafeHtml {
    if (!value) return "";
    const html = marked.parse(value, { async: false }) as string;
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }
}

/** Strips markdown syntax to produce plain text for previews. */
export function stripMarkdown(md: string | null | undefined): string {
  if (!md) return "";
  return md
    .replace(/^#{1,6}\s+/gm, "") // headers
    .replace(/\*\*(.+?)\*\*/g, "$1") // bold
    .replace(/\*(.+?)\*/g, "$1") // italic
    .replace(/^-{3,}$/gm, "") // horizontal rules
    .replace(/^\|.*\|$/gm, "") // table rows
    .replace(/^- /gm, "") // bullet lists
    .replace(/\n{2,}/g, " ") // collapse multiple newlines
    .replace(/\n/g, " ") // single newlines to spaces
    .trim();
}
