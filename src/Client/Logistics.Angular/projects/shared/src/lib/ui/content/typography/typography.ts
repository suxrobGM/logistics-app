import { NgTemplateOutlet } from "@angular/common";
import { Component, computed, input } from "@angular/core";

export type TypographyVariant =
  | "h1"
  | "h2"
  | "h3"
  | "h4"
  | "h5"
  | "h6"
  | "body"
  | "body-sm"
  | "caption"
  | "overline"
  | "label"
  | "stat";

export type TypographyColor = "primary" | "secondary" | "muted" | "inherit";
export type TypographyWeight = "regular" | "medium" | "semibold" | "bold";
export type TypographyAlign = "start" | "center" | "end";
export type TypographyTag =
  | "span"
  | "p"
  | "label"
  | "div"
  | "h1"
  | "h2"
  | "h3"
  | "h4"
  | "h5"
  | "h6";

const variantClasses: Record<TypographyVariant, string> = {
  h1: "text-3xl md:text-4xl font-bold tracking-tight",
  h2: "text-2xl md:text-3xl font-bold tracking-tight",
  h3: "text-xl md:text-2xl font-semibold",
  h4: "text-lg font-semibold",
  h5: "text-base font-semibold",
  h6: "text-sm font-semibold",
  body: "text-base",
  "body-sm": "text-sm",
  caption: "text-xs",
  overline: "text-sm font-semibold tracking-wide uppercase",
  label: "text-sm font-medium",
  stat: "text-3xl font-bold tracking-tight",
};

const variantDefaultTag: Record<TypographyVariant, TypographyTag> = {
  h1: "h1",
  h2: "h2",
  h3: "h3",
  h4: "h4",
  h5: "h5",
  h6: "h6",
  body: "p",
  "body-sm": "p",
  caption: "span",
  overline: "span",
  label: "label",
  stat: "span",
};

const colorClasses: Record<TypographyColor, string> = {
  primary: "text-foreground",
  secondary: "text-subtle-foreground",
  muted: "text-muted-foreground",
  inherit: "",
};

const weightClasses: Record<TypographyWeight, string> = {
  regular: "font-normal",
  medium: "font-medium",
  semibold: "font-semibold",
  bold: "font-bold",
};

const alignClasses: Record<TypographyAlign, string> = {
  start: "text-start",
  center: "text-center",
  end: "text-end",
};

/**
 * Variants that represent standalone blocks (headings, paragraphs, the big
 * stat number) flow on their own line. Inline annotations (caption, overline,
 * form label) stay inline so they can sit within surrounding text. Without
 * this, a `caption` + `stat` pair in a plain container renders on one line
 * (e.g. "Total Tenants2").
 */
const blockVariants = new Set<TypographyVariant>([
  "h1",
  "h2",
  "h3",
  "h4",
  "h5",
  "h6",
  "body",
  "body-sm",
  "stat",
]);

/**
 * Theme-aware text primitive. `variant` selects the visual hierarchy
 * (h1–h6, body, body-sm, caption, overline, label, stat) and drives the
 * default semantic tag, which `as` can override.
 */
@Component({
  selector: "ui-typography",
  templateUrl: "./typography.html",
  imports: [NgTemplateOutlet],
  host: {
    "[class.block]": "isBlock()",
    "[class.inline]": "!isBlock()",
  },
})
export class Typography {
  public readonly variant = input<TypographyVariant>("body");
  public readonly color = input<TypographyColor>("inherit");
  public readonly weight = input<TypographyWeight | null>(null);
  public readonly align = input<TypographyAlign | null>(null);
  public readonly tag = input<TypographyTag | null>(null);

  /**
   * Id of the control this labels, for `variant="label"` / `tag="label"`. A <label> with no `for`
   * names nothing: clicking it focuses no control and screen readers announce it as orphaned.
   * Named `htmlFor` because `for` is a reserved word (and input aliasing is banned).
   */
  public readonly htmlFor = input<string | undefined>(undefined);

  protected readonly resolvedTag = computed<TypographyTag>(
    () => this.tag() ?? variantDefaultTag[this.variant()],
  );

  protected readonly isBlock = computed(() => blockVariants.has(this.variant()));

  protected readonly classes = computed(() => {
    const parts = [variantClasses[this.variant()], colorClasses[this.color()]];
    const weight = this.weight();
    const align = this.align();

    if (weight) {
      parts.push(weightClasses[weight]);
    }
    if (align) {
      parts.push(alignClasses[align]);
    }
    return parts.filter(Boolean).join(" ");
  });
}
