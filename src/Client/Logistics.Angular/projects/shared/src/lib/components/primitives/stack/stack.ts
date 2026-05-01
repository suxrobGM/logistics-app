import { NgTemplateOutlet } from "@angular/common";
import { booleanAttribute, Component, computed, input } from "@angular/core";

export type StackDirection = "row" | "col";
export type StackGap = "0" | "1" | "2" | "3" | "4" | "6" | "8";
export type StackAlign = "start" | "center" | "end" | "stretch";
export type StackJustify = "start" | "center" | "end" | "between" | "around";
export type StackTag = "div" | "section" | "header" | "footer";

const directionClasses: Record<StackDirection, string> = {
  row: "flex flex-row",
  col: "flex flex-col",
};

const gapClasses: Record<StackGap, string> = {
  "0": "gap-0",
  "1": "gap-1",
  "2": "gap-2",
  "3": "gap-3",
  "4": "gap-4",
  "6": "gap-6",
  "8": "gap-8",
};

const alignClasses: Record<StackAlign, string> = {
  start: "items-start",
  center: "items-center",
  end: "items-end",
  stretch: "items-stretch",
};

const justifyClasses: Record<StackJustify, string> = {
  start: "justify-start",
  center: "justify-center",
  end: "justify-end",
  between: "justify-between",
  around: "justify-around",
};

/**
 * Flex container primitive. Replaces ad-hoc `flex {row|col} gap-N items-* justify-*`
 * class strings with a typed input API.
 */
@Component({
  selector: "ui-stack",
  templateUrl: "./stack.html",
  imports: [NgTemplateOutlet],
  host: { class: "block" },
})
export class Stack {
  public readonly direction = input<StackDirection>("col");
  public readonly gap = input<StackGap>("4");
  public readonly align = input<StackAlign | null>(null);
  public readonly justify = input<StackJustify | null>(null);
  public readonly wrap = input<boolean, unknown>(false, { transform: booleanAttribute });
  public readonly tag = input<StackTag>("div");

  protected readonly classes = computed(() => {
    const parts = [directionClasses[this.direction()], gapClasses[this.gap()]];
    const align = this.align();
    if (align) parts.push(alignClasses[align]);
    const justify = this.justify();
    if (justify) parts.push(justifyClasses[justify]);
    if (this.wrap()) parts.push("flex-wrap");
    return parts.join(" ");
  });
}
