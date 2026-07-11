import { ChangeDetectionStrategy, Component } from "@angular/core";
import { HlmToaster } from "../../primitives/sonner";

/**
 * The app's toast surface. Exactly one per shell — all four `app.html` render it, and it replaces the
 * `<p-toast key="notification">` that used to sit there.
 *
 * WHY THIS WRAPPER EXISTS (rather than putting `<hlm-toaster>` straight into the four shells):
 * `hlm-*` primitives are PRIVATE — `ui-*` is the public surface, and app shells are call sites like
 * any other. It also means the toast's position lives in ONE place instead of being retyped into four
 * templates that would then drift apart.
 *
 * `position="top-right"` is NOT decoration and must not be dropped: `hlm-toaster` defaults to
 * BOTTOM-right, whereas every `<p-toast>` it replaces was `position="top-right"`. Omitting it silently
 * relocates every toast in the product to the opposite corner.
 *
 * Colour needs no configuration: `hlm-toaster` styles itself from `--popover` / `--popover-foreground`
 * / `--border`, which are defined in both `:root` and `.dark-theme`, so dark mode follows the app
 * theme without binding sonner's own `theme` input (which reads `prefers-color-scheme` — the OS
 * setting — and would therefore disagree with our in-app theme toggle).
 */
@Component({
  selector: "ui-toaster",
  templateUrl: "./toaster.html",
  imports: [HlmToaster],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UiToaster {}
