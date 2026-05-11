import { NgTemplateOutlet } from "@angular/common";
import { booleanAttribute, Component, computed, input } from "@angular/core";
import { resolveResponsive, type Responsive, type ResponsiveClassTable } from "../responsive";

export type StackDirection = "row" | "col";
export type StackGap = "0" | "1" | "2" | "3" | "4" | "6" | "8";
export type StackAlign = "start" | "center" | "end" | "stretch";
export type StackJustify = "start" | "center" | "end" | "between" | "around";
export type StackTag = "div" | "section" | "header" | "footer";

// All class strings are literals so Tailwind's JIT scanner can detect them.
const directionClasses: ResponsiveClassTable<StackDirection> = {
  row: {
    xs: "flex-row",
    sm: "sm:flex-row",
    md: "md:flex-row",
    lg: "lg:flex-row",
    xl: "xl:flex-row",
    "2xl": "2xl:flex-row",
  },
  col: {
    xs: "flex-col",
    sm: "sm:flex-col",
    md: "md:flex-col",
    lg: "lg:flex-col",
    xl: "xl:flex-col",
    "2xl": "2xl:flex-col",
  },
};

const gapClasses: ResponsiveClassTable<StackGap> = {
  "0": {
    xs: "gap-0",
    sm: "sm:gap-0",
    md: "md:gap-0",
    lg: "lg:gap-0",
    xl: "xl:gap-0",
    "2xl": "2xl:gap-0",
  },
  "1": {
    xs: "gap-1",
    sm: "sm:gap-1",
    md: "md:gap-1",
    lg: "lg:gap-1",
    xl: "xl:gap-1",
    "2xl": "2xl:gap-1",
  },
  "2": {
    xs: "gap-2",
    sm: "sm:gap-2",
    md: "md:gap-2",
    lg: "lg:gap-2",
    xl: "xl:gap-2",
    "2xl": "2xl:gap-2",
  },
  "3": {
    xs: "gap-3",
    sm: "sm:gap-3",
    md: "md:gap-3",
    lg: "lg:gap-3",
    xl: "xl:gap-3",
    "2xl": "2xl:gap-3",
  },
  "4": {
    xs: "gap-4",
    sm: "sm:gap-4",
    md: "md:gap-4",
    lg: "lg:gap-4",
    xl: "xl:gap-4",
    "2xl": "2xl:gap-4",
  },
  "6": {
    xs: "gap-6",
    sm: "sm:gap-6",
    md: "md:gap-6",
    lg: "lg:gap-6",
    xl: "xl:gap-6",
    "2xl": "2xl:gap-6",
  },
  "8": {
    xs: "gap-8",
    sm: "sm:gap-8",
    md: "md:gap-8",
    lg: "lg:gap-8",
    xl: "xl:gap-8",
    "2xl": "2xl:gap-8",
  },
};

const alignClasses: ResponsiveClassTable<StackAlign> = {
  start: {
    xs: "items-start",
    sm: "sm:items-start",
    md: "md:items-start",
    lg: "lg:items-start",
    xl: "xl:items-start",
    "2xl": "2xl:items-start",
  },
  center: {
    xs: "items-center",
    sm: "sm:items-center",
    md: "md:items-center",
    lg: "lg:items-center",
    xl: "xl:items-center",
    "2xl": "2xl:items-center",
  },
  end: {
    xs: "items-end",
    sm: "sm:items-end",
    md: "md:items-end",
    lg: "lg:items-end",
    xl: "xl:items-end",
    "2xl": "2xl:items-end",
  },
  stretch: {
    xs: "items-stretch",
    sm: "sm:items-stretch",
    md: "md:items-stretch",
    lg: "lg:items-stretch",
    xl: "xl:items-stretch",
    "2xl": "2xl:items-stretch",
  },
};

const justifyClasses: ResponsiveClassTable<StackJustify> = {
  start: {
    xs: "justify-start",
    sm: "sm:justify-start",
    md: "md:justify-start",
    lg: "lg:justify-start",
    xl: "xl:justify-start",
    "2xl": "2xl:justify-start",
  },
  center: {
    xs: "justify-center",
    sm: "sm:justify-center",
    md: "md:justify-center",
    lg: "lg:justify-center",
    xl: "xl:justify-center",
    "2xl": "2xl:justify-center",
  },
  end: {
    xs: "justify-end",
    sm: "sm:justify-end",
    md: "md:justify-end",
    lg: "lg:justify-end",
    xl: "xl:justify-end",
    "2xl": "2xl:justify-end",
  },
  between: {
    xs: "justify-between",
    sm: "sm:justify-between",
    md: "md:justify-between",
    lg: "lg:justify-between",
    xl: "xl:justify-between",
    "2xl": "2xl:justify-between",
  },
  around: {
    xs: "justify-around",
    sm: "sm:justify-around",
    md: "md:justify-around",
    lg: "lg:justify-around",
    xl: "xl:justify-around",
    "2xl": "2xl:justify-around",
  },
};

/**
 * Flex container primitive. Replaces ad-hoc `flex {row|col} gap-N items-* justify-*`
 * class strings with a typed input API. Each value input also accepts a per-breakpoint
 * object: `[direction]="{ xs: 'col', md: 'row' }"`.
 */
@Component({
  selector: "ui-stack",
  templateUrl: "./stack.html",
  imports: [NgTemplateOutlet],
  host: { class: "block" },
})
export class Stack {
  public readonly direction = input<Responsive<StackDirection>>("col");
  public readonly gap = input<Responsive<StackGap>>("0");
  public readonly align = input<Responsive<StackAlign> | null>(null);
  public readonly justify = input<Responsive<StackJustify> | null>(null);
  public readonly wrap = input<boolean, unknown>(false, { transform: booleanAttribute });
  public readonly tag = input<StackTag>("div");

  protected readonly classes = computed(() => {
    const parts: string[] = ["flex"];
    parts.push(...resolveResponsive(this.direction(), directionClasses));
    parts.push(...resolveResponsive(this.gap(), gapClasses));
    parts.push(...resolveResponsive(this.align(), alignClasses));
    parts.push(...resolveResponsive(this.justify(), justifyClasses));
    if (this.wrap()) parts.push("flex-wrap");
    return parts.join(" ");
  });
}
