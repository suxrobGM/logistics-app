import { Directive } from "@angular/core";
import { NgControl } from "@angular/forms";

/**
 * Severs the ambient `NgControl` for a projected Helm/brain form primitive.
 *
 * Our `ui-*-field` wrappers implement `FormValueControl` and are bound with `[formField]` /
 * `formControlName` on their host, which provides an `NgControl` (Signal Forms' `InteropNgControl`)
 * in the element injector. Brain's `BrnFieldControl` — host-directed by `BrnSelect` and friends —
 * resolves that ancestor `NgControl` hierarchically and hands it to `createStateTracker`, which
 * subscribes to `control.events` / `statusChanges`. `InteropNgControl` exposes neither, so it
 * crashes at init (the same shape as the `pTextarea` + `[formField]` crash).
 *
 * Applying this directive to the inner Helm element provides `NgControl: null` on that element's
 * injector, so brain resolves null (self-element wins over the ancestor) and skips its control-state
 * tracking. The wrapper drives the primitive via plain `[value]` / `(valueChange)` and owns the
 * invalid state via `[forceInvalid]="showInvalid()"`.
 */
@Directive({
  selector: "[uiDetachedControl]",
  providers: [{ provide: NgControl, useValue: null }],
})
export class DetachedControl {}
