/**
 * `ui-overlay-badge` exists for ONE call site — the notification bell — and for one behaviour that
 * the call site cannot tell you about:
 *
 *     [value]="unreadCount() > 0 ? (… ) : null"        notification-bell.html
 *
 * The bell says "no unread messages" by handing over `null`, and trusts the badge to vanish — there
 * is no `@if`, no class and no comment at the call site recording the dependency. Get it wrong and
 * the bell shows a red dot forever, on every page, for every user — with a green build and a green
 * test suite.
 *
 * That is what this file is for. The rest of the component is 10 lines of positioning.
 */
import { Component, provideZonelessChangeDetection, signal } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import { OverlayBadge } from "./overlay-badge";

@Component({
  selector: "ui-host-overlay-badge",
  imports: [OverlayBadge],
  template: `
    <ui-overlay-badge [value]="value()" severity="danger" badgeSize="small">
      <span data-test="content">bell</span>
    </ui-overlay-badge>
  `,
})
class HostOverlayBadge {
  readonly value = signal<string | number | null>(null);
}

describe("ui-overlay-badge", () => {
  let fixture: ComponentFixture<HostOverlayBadge>;
  let host: HostOverlayBadge;

  const badge = (): HTMLElement | null => fixture.nativeElement.querySelector("ui-count-badge");

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HostOverlayBadge],
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();
    fixture = TestBed.createComponent(HostOverlayBadge);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("always projects its content", () => {
    expect(fixture.nativeElement.querySelector('[data-test="content"]')).toBeTruthy();
  });

  describe("a value of nothing HIDES the badge — the notification bell depends on it", () => {
    it("renders no badge for null", () => {
      host.value.set(null);
      fixture.detectChanges();
      expect(badge()).toBeNull();
    });

    it("renders no badge for an empty string", () => {
      host.value.set("");
      fixture.detectChanges();
      expect(badge()).toBeNull();
    });

    it("renders no badge for a zero count — 0 unread is not a notification", () => {
      host.value.set(0);
      fixture.detectChanges();
      expect(badge()).toBeNull();
    });

    it("still projects the content while hidden", () => {
      host.value.set(null);
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('[data-test="content"]')).toBeTruthy();
    });
  });

  describe("a real value SHOWS the badge", () => {
    it('renders the count — including the bell\'s "9+"', () => {
      host.value.set("9+");
      fixture.detectChanges();
      expect(badge()?.textContent?.trim()).toBe("9+");
    });

    it("renders a numeric count", () => {
      host.value.set(3);
      fixture.detectChanges();
      expect(badge()?.textContent?.trim()).toBe("3");
    });
  });
});
