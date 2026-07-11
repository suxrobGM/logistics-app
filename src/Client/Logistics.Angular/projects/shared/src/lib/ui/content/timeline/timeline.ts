import { NgTemplateOutlet } from "@angular/common";
import { Component, contentChild, Directive, inject, input, TemplateRef } from "@angular/core";

/** The body of one timeline event: `<ng-template uiTimelineContent let-event>…</ng-template>`. */
@Directive({ selector: "[uiTimelineContent]" })
export class UiTimelineContent {
  public readonly templateRef = inject<TemplateRef<unknown>>(TemplateRef);
}

/**
 * The dot for one event: `<ng-template uiTimelineMarker let-event>…</ng-template>`. Optional — a
 * themed default dot is drawn when it is absent.
 */
@Directive({ selector: "[uiTimelineMarker]" })
export class UiTimelineMarker {
  public readonly templateRef = inject<TemplateRef<unknown>>(TemplateRef);
}

/**
 * A vertical timeline. Replaces `<p-timeline>` at 2 call sites (ai-dispatch/session-detail and
 * trips/trip-timeline). Hand-rolled: spartan has no timeline.
 *
 * BOTH templates are supported, because both are used. The brief stated that only `#content` was in
 * play and that there was no `#marker` — that is wrong: `trip-timeline.html` binds `#marker` too, and
 * dropping it would have silently replaced its coloured, per-event-type icons with a plain dot. The
 * source won.
 *
 * The `::ng-deep .p-timeline-*` rules in `session-detail.css` are ported by CONSTRUCTION rather than
 * copied: `.p-timeline-event-opposite { display: none }` existed to hide a spacer element PrimeNG
 * rendered on the empty side, and this markup has no such element; `.p-timeline-event-content
 * { flex: 1 }` is the `flex-1` on the content column below. Both rules are therefore deleted at the
 * call site — and they had to be, since an element-selector rule stops matching the moment the tag is
 * renamed, silently (the S5 lesson).
 */
@Component({
  selector: "ui-timeline",
  templateUrl: "./timeline.html",
  imports: [NgTemplateOutlet],
})
export class UiTimeline<T = unknown> {
  public readonly value = input<readonly T[]>([]);

  protected readonly content = contentChild.required(UiTimelineContent);
  protected readonly marker = contentChild(UiTimelineMarker);
}
