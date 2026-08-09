import { persistValue, readStoredNumber } from "@logistics/shared/utils";

const DrawerWidthKey = "copilot.width";
export const DefaultDrawerWidth = 400;
const MinDrawerWidth = 320;
const MaxDrawerWidth = 640;

export const clampDrawerWidth = (width: number): number =>
  Math.min(MaxDrawerWidth, Math.max(MinDrawerWidth, Math.round(width)));

export const readStoredDrawerWidth = (): number =>
  clampDrawerWidth(readStoredNumber(DrawerWidthKey, DefaultDrawerWidth));

export const persistDrawerWidth = (width: number): void => persistValue(DrawerWidthKey, width);
