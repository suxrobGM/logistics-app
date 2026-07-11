/**
 * The icon runtime's contract. Every failure mode here is INVISIBLE to the build, to lint and to every
 * other spec — it shows up as a 0x0 <svg> on a page nobody opened. So it is pinned:
 *
 *   1. alias resolution  — `cog` -> lucideSettings, `times` -> lucideX, `facebook` -> brandFacebook.
 *   2. size="inherit"    — emits NO `text-*` class, so `[&_ng-icon:not([class*='text-'])]:size-4`
 *                          (how Helm's button sizes its icons) still matches.
 *   3. spin              — `animate-spin`, replacing primeicons' spin class (a CSS keyframe, not a glyph).
 *   4. unknown name      — console.error + a VISIBLE error glyph, and NOT a throw: a throw during
 *                          render tears down the view in a zoneless app, and every intermediate state
 *                          of the PrimeNG -> spartan migration must stay drivable in a browser.
 */
import { Component, provideZonelessChangeDetection, signal } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { provideIcons } from "@ng-icons/core";
import { lucideCircleAlert, lucideSettings } from "@ng-icons/lucide";
import { brandFacebook } from "../../icons/brand-icons";
import { BASE_NG_ICONS, ICON_ALIASES, type UiIconName } from "../../icons/icon-registry.generated";
import { resolveNgIcon, toNgIconName, UI_ICON_ERROR_GLYPH } from "../../icons/ui-icons";
import { Icon, type IconSize } from "./icon";

/** A host with a DYNAMIC [name] — the only way an unknown name can reach the runtime. */
@Component({
  selector: "ui-host-icon",
  imports: [Icon],
  template: `<ui-icon [name]="name()" [size]="size()" [spin]="spin()" />`,
})
class HostIcon {
  readonly name = signal<UiIconName>("check");
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
  // `as UiIconName` is the whole point of the unknown-name tests: a literal would not compile.
  fixture.componentInstance.name.set(name as UiIconName);
  fixture.componentInstance.size.set(size);
  fixture.componentInstance.spin.set(spin);
  await settle(fixture);
  return fixture;
}

describe("ui-icon", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideIcons({ ...BASE_NG_ICONS, lucideSettings, brandFacebook }),
      ],
    });
  });

  describe("name resolution", () => {
    it("maps a legacy PrimeIcons name to its lucide export", () => {
      expect(resolveNgIcon("cog")).toBe("lucideSettings");
      expect(resolveNgIcon("times")).toBe("lucideX");
      expect(resolveNgIcon("exclamation-triangle")).toBe("lucideTriangleAlert");
    });

    it("passes an already-lucide kebab name through the map to itself", () => {
      expect(resolveNgIcon("chevron-down")).toBe("lucideChevronDown");
      expect(resolveNgIcon("settings")).toBe("lucideSettings");
    });

    it("resolves the brand family to the hand-vendored exports, not to lucide", () => {
      expect(resolveNgIcon("facebook")).toBe("brandFacebook");
      expect(resolveNgIcon("linkedin")).toBe("brandLinkedin");
      expect(resolveNgIcon("twitter")).toBe("brandX");
      // `x` is a real lucide glyph and must NOT be confused with the brand one.
      expect(toNgIconName("brand-x")).toBe("brandX");
      expect(toNgIconName("x")).toBe("lucideX");
      expect(toNgIconName("building-2")).toBe("lucideBuilding2");
    });

    it("renders the resolved svg", async () => {
      const fixture = await render("cog");
      expect(ngIcon(fixture).querySelector("svg")).toBeTruthy();
    });

    it("renders a brand glyph's svg", async () => {
      const fixture = await render("facebook");
      expect(ngIcon(fixture).innerHTML).toContain("<svg");
    });

    it("every name in the union resolves without hitting the error path", () => {
      const error = vi.spyOn(console, "error").mockImplementation(() => undefined);

      for (const name of Object.keys(ICON_ALIASES)) {
        expect(resolveNgIcon(name)).toMatch(/^(lucide|brand)[A-Z]/);
      }

      expect(error).not.toHaveBeenCalled();
      error.mockRestore();
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
      const fixture = await render("spinner");
      expect(ngIcon(fixture).className).not.toContain("animate-spin");
    });

    it("applies animate-spin when set", async () => {
      const fixture = await render("spinner", { spin: true });
      expect(ngIcon(fixture).className).toContain("animate-spin");
    });

    it("is hosted on an inline-flex element — transform is a no-op on an inline box", async () => {
      const fixture = await render("spinner", { spin: true });
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
      expect(ngIcon(fixture).innerHTML).toBe(lucideCircleAlert);
      expect(UI_ICON_ERROR_GLYPH).toBe("lucideCircleAlert");

      error.mockRestore();
    });

    it("recovers when the binding changes back to a known name", async () => {
      const error = vi.spyOn(console, "error").mockImplementation(() => undefined);

      const fixture = await render("not-a-real-icon");
      fixture.componentInstance.name.set("check");
      await settle(fixture);

      expect(ngIcon(fixture).innerHTML).not.toBe(lucideCircleAlert);
      expect(ngIcon(fixture).querySelector("svg")).toBeTruthy();

      error.mockRestore();
    });
  });
});
