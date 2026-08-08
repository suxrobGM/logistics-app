const RightPanelCollapsedKey = "dispatch-chat.right-panel-collapsed";

export const readStoredRightPanelCollapsed = (): boolean =>
  localStorage.getItem(RightPanelCollapsedKey) === "true";

export const persistRightPanelCollapsed = (collapsed: boolean): void =>
  localStorage.setItem(RightPanelCollapsedKey, String(collapsed));
