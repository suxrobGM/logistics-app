import { NgTemplateOutlet } from "@angular/common";
import { Component, contentChild, Directive, inject, input, TemplateRef } from "@angular/core";

/** The body of one timeline event: `<ng-template uiTimelineContent let-event>…</ng-template>`. */
@Directive({ selector: "[uiTimelineContent]" })
export class UiTimelineContent {
  public readonly templateRef = inject<TemplateRef<unknown>>(TemplateRef);
}

/**
 * The dot for one event: `<ng-template uiTimelineMarker let-event>…</ng-template>`. Optional - a
 * themed default dot is drawn when it is absent.
 */
@Directive({ selector: "[uiTimelineMarker]" })
export class UiTimelineMarker {
  public readonly templateRef = inject<TemplateRef<unknown>>(TemplateRef);
}

/**
 * A vertical timeline, used by ai-dispatch/session-details and trips/trip-timeline. Hand-rolled:
 * spartan has no timeline.
 *
 * BOTH templates are supported, because both are used: `trip-timeline.html` binds `#marker` for its
 * coloured, per-event-type icons, and dropping it would silently replace them with a plain dot.
 *
 * This markup renders no spacer on the empty side (so nothing needs hiding at the call site) and the
 * content column carries `flex-1` itself - the call sites need no timeline CSS of their own.
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
