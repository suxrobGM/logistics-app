/**
 * The icon runtime's contract. Every failure mode here is INVISIBLE to the build, to lint and to every
 * other spec — it shows up as a 0x0 <svg> on a page nobody opened. So it is pinned:
 *
 *   1. svg binding      — a known name binds its raw SVG straight to `<ng-icon [svg]>`; a renamed
 *                          legacy name (`cog`) is now a compile error, not a runtime alias.
 *   2. size="inherit"   — emits NO `text-*` class, so `[&_ng-icon:not([class*='text-'])]:size-4`
 *                          (how Helm's button sizes its icons) still matches.
 *   3. spin             — `animate-spin`, replacing primeicons' spin class (a CSS keyframe, not a glyph).
 *   4. unknown name     — console.error + a VISIBLE error glyph, and NOT a throw: a throw during
 *                          render tears down the view in a zoneless app, and every intermediate state
 *                          of an icon-name rename must stay drivable in a browser.
 */
import { Component, provideZonelessChangeDetection, signal } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { ERROR_ICON_SVG, UI_ICONS, type IconName } from "../../icons/icons";
import { Icon, type IconSize } from "./icon";

/** A host with a DYNAMIC [name] — the only way an unknown name can reach the runtime. */
@Component({
  selector: "ui-host-icon",
  imports: [Icon],
  template: `<ui-icon [name]="name()" [size]="size()" [spin]="spin()" />`,
})
class HostIcon {
  readonly name = signal<IconName>("check");
  readonly size = signal<IconSize>("md");
  readonly spin = signal(false);
}

async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

function ngIcon(fixture: ComponentFixture<unknown>): HTMLElement {
  return fixture.nativeElement.querySelector("ng-icon") as HTMLElement;
}

async function render(
  name: string,
  { size = "md" as IconSize, spin = false } = {},
): Promise<ComponentFixture<HostIcon>> {
  const fixture = TestBed.createComponent(HostIcon);
  // `as IconName` is the whole point of the unknown-name tests: a literal would not compile.
  fixture.componentInstance.name.set(name as IconName);
  fixture.componentInstance.size.set(size);
  fixture.componentInstance.spin.set(spin);
  await settle(fixture);
  return fixture;
}

describe("ui-icon", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  describe("svg resolution", () => {
    it("binds the resolved svg for a known name", async () => {
      const fixture = await render("settings");
      expect(ngIcon(fixture).querySelector("svg")).toBeTruthy();
      expect(ngIcon(fixture).innerHTML).toBe(UI_ICONS.settings);
    });

    it("renders a brand glyph's svg", async () => {
      const fixture = await render("brand-facebook");
      expect(ngIcon(fixture).innerHTML).toContain("<svg");
      expect(ngIcon(fixture).innerHTML).toBe(UI_ICONS["brand-facebook"]);
    });

    it("every entry in UI_ICONS is a real svg string", () => {
      for (const [name, svg] of Object.entries(UI_ICONS)) {
        expect(svg, `"${name}" has no svg`).toContain("<svg");
      }
    });
  });

  describe("size", () => {
    it("emits a text-* class by default (md) — 246 call sites depend on it", async () => {
      const fixture = await render("check");
      expect(ngIcon(fixture).className).toContain("text-base");
    });

    it('size="inherit" emits NO class at all, so a Helm button can size the icon itself', async () => {
      const fixture = await render("check", { size: "inherit" });
      expect(ngIcon(fixture).className).not.toContain("text-");
      // The exact predicate Helm's button uses: ng-icon:not([class*='text-']).
      expect(ngIcon(fixture).matches("ng-icon:not([class*='text-'])")).toBe(true);
    });
  });

  describe("spin", () => {
    it("is off by default", async () => {
      const fixture = await render("loader-circle");
      expect(ngIcon(fixture).className).not.toContain("animate-spin");
    });

    it("applies animate-spin when set", async () => {
      const fixture = await render("loader-circle", { spin: true });
      expect(ngIcon(fixture).className).toContain("animate-spin");
    });

    it("is hosted on an inline-flex element — transform is a no-op on an inline box", async () => {
      const fixture = await render("loader-circle", { spin: true });
      const host = fixture.nativeElement.querySelector("ui-icon") as HTMLElement;
      expect(host.className).toContain("inline-flex");
    });
  });

  describe("an unknown name (only reachable through a dynamic [name] binding)", () => {
    it("logs, renders the error glyph, and does NOT throw", async () => {
      const error = vi.spyOn(console, "error").mockImplementation(() => undefined);

      const fixture = await render("not-a-real-icon");

      expect(error).toHaveBeenCalledOnce();
      expect(String(error.mock.calls[0][0])).toContain("not-a-real-icon");
      expect(ngIcon(fixture).querySelector("svg")).toBeTruthy();
      expect(ngIcon(fixture).innerHTML).toBe(ERROR_ICON_SVG);
      expect(ERROR_ICON_SVG).toBe(UI_ICONS["circle-alert"]);

      error.mockRestore();
    });

    it("recovers when the binding changes back to a known name", async () => {
      const error = vi.spyOn(console, "error").mockImplementation(() => undefined);

      const fixture = await render("not-a-real-icon");
      fixture.componentInstance.name.set("check");
      await settle(fixture);

      expect(ngIcon(fixture).innerHTML).toBe(UI_ICONS.check);
      expect(ngIcon(fixture).querySelector("svg")).toBeTruthy();

      error.mockRestore();
    });
  });
});
