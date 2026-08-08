const DrawerWidthKey = "copilot.width";
export const DefaultDrawerWidth = 400;
const MinDrawerWidth = 320;
const MaxDrawerWidth = 640;

export const clampDrawerWidth = (width: number): number =>
  Math.min(MaxDrawerWidth, Math.max(MinDrawerWidth, Math.round(width)));

export const readStoredDrawerWidth = (): number => {
  const stored = Number(localStorage.getItem(DrawerWidthKey));
  return Number.isFinite(stored) && stored > 0 ? clampDrawerWidth(stored) : DefaultDrawerWidth;
};

export const persistDrawerWidth = (width: number): void =>
  localStorage.setItem(DrawerWidthKey, String(width));
