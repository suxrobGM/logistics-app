/**
 * P1 PROBE — throwaway. Proves the ONE assumption the whole S11 design rests on.
 *
 * `<ui-data-table>` will provide `UiTableState` on its own host node and drop the
 * `{provide: Table, useFactory: host => host.innerTable()}` hack. For that to work:
 *
 *   A `<ng-template #header>` written in the CONSUMER is DECLARED as a child node of the
 *   `<ui-data-table>` element. `createEmbeddedView` parents the embedded view's injector at the
 *   DECLARATION node — NOT at the insertion point inside the table's own view. So `<th
 *   uiSortHeader>` inside it walks: ng-template -> <ui-data-table> (provides UiTableState) ->
 *   consumer.
 *
 * And for trips-list's NESTED table, the inner table's element is a strictly closer ancestor for
 * its OWN templates, so shadowing is structural.
 *
 * This mirrors the real mechanism exactly: templates declared in the consumer, pulled in with
 * contentChild, rendered with ngTemplateOutlet from inside the table's view — including a nested
 * table declared inside the OUTER table's #expandedrow template (an embedded view inside an
 * embedded view), which is precisely the trips-list shape.
 *
 * If this fails, the fallback is an explicit `<th uiSortHeader="dt">` template-ref handle.
 */
import { NgTemplateOutlet } from "@angular/common";
import {
  Component,
  contentChild,
  Directive,
  inject,
  Injectable,
  provideZonelessChangeDetection,
  type TemplateRef,
} from "@angular/core";
import { TestBed } from "@angular/core/testing";

@Injectable()
class ProbeState {
  public label = "";
}

/** Stands in for `<th uiSortHeader>`: written in a projected template, injects the table's state. */
@Directive({ selector: "[uiProbeHeader]" })
class ProbeHeader {
  public readonly state = inject(ProbeState);
}

/**
 * Stands in for `<ui-data-table>`: provides the state on its own host node, and renders the
 * consumer-declared templates through ngTemplateOutlet from inside its OWN view.
 */
@Component({
  selector: "ui-probe-table",
  imports: [NgTemplateOutlet],
  providers: [ProbeState],
  template: `
    <ng-container [ngTemplateOutlet]="$any(headerTpl())" />
    <ng-container [ngTemplateOutlet]="$any(expandedTpl())" />
  `,
})
class ProbeTable {
  public readonly state = inject(ProbeState);
  protected readonly headerTpl = contentChild<TemplateRef<unknown>>("header");
  protected readonly expandedTpl = contentChild<TemplateRef<unknown>>("expandedrow");
}

/** The trips-list shape: an outer table whose #expandedrow contains a whole second table. */
@Component({
  selector: "ui-probe-host",
  imports: [ProbeTable, ProbeHeader],
  template: `
    <ui-probe-table #outer>
      <ng-template #header>
        <span uiProbeHeader data-which="outer"></span>
      </ng-template>

      <ng-template #expandedrow>
        <ui-probe-table #inner>
          <ng-template #header>
            <span uiProbeHeader data-which="inner"></span>
          </ng-template>
        </ui-probe-table>
      </ng-template>
    </ui-probe-table>
  `,
})
class ProbeHost {}

describe("P1 probe — UiTableState reaches projected content, and nesting shadows", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("each projected header injects the UiTableState of its own lexically-enclosing table", async () => {
    const fixture = TestBed.createComponent(ProbeHost);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const tables = fixture.debugElement
      .queryAllNodes((n) => !!n.providerTokens?.includes(ProbeTable))
      .map((n) => n.injector.get(ProbeTable));
    expect(tables.length).toBe(2); // outer + the nested one inside #expandedrow

    const headers = fixture.debugElement
      .queryAllNodes((n) => !!n.providerTokens?.includes(ProbeHeader))
      .map((n) => ({
        which: (n.nativeNode as HTMLElement).getAttribute("data-which"),
        state: n.injector.get(ProbeHeader).state,
      }));
    expect(headers.length).toBe(2);

    const outer = headers.find((h) => h.which === "outer");
    const inner = headers.find((h) => h.which === "inner");

    // 1. DI reaches projected content at all: neither threw NG0201 above.
    // 2. The outer header got the OUTER table's state…
    expect(outer?.state).toBe(tables[0].state);
    // 3. …and the inner header got the INNER table's state — shadowed, not the outer's.
    expect(inner?.state).toBe(tables[1].state);
    expect(outer?.state).not.toBe(inner?.state);
  });
});
