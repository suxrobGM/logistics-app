import { UiTab } from "./tab";
import { UiTabList } from "./tab-list";
import { UiTabPanel } from "./tab-panel";
import { UiTabPanels } from "./tab-panels";
import { UiTabs } from "./tabs";

export * from "./tab";
export * from "./tab-list";
export * from "./tab-panel";
export * from "./tab-panels";
export * from "./tab-value";
export * from "./tabs";

/** All five tab elements. Import this rather than picking them off one by one. */
export const UiTabsImports = [UiTabs, UiTabList, UiTab, UiTabPanels, UiTabPanel] as const;
