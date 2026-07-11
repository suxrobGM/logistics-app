/**
 * Maps the legacy PrimeIcons-style names `ui-icon` receives (`pi-` prefix stripped) onto
 * lucide icon names (kebab-case). Lets the 80+ existing `<ui-icon name="...">` call sites keep
 * their names while rendering lucide SVGs. Names not in the map are assumed to already be lucide
 * kebab names and pass through unchanged (that is how the nav's dynamic icons resolve). Phase 6
 * can migrate call sites to lucide names directly.
 *
 * Icons render through `@ng-icons/lucide`, whose exports are named `lucide` + PascalCase of the
 * kebab name (e.g. `chevron-down` → `lucideChevronDown`). {@link toNgIconName} applies that
 * transform; the resolved icon must be registered via `provideIcons(...)` in each portal.
 */
export const PI_TO_LUCIDE: Record<string, string> = {
  "check-circle": "circle-check",
  box: "box",
  truck: "truck",
  "map-marker": "map-pin",
  "map-pin": "map-pin",
  "exclamation-triangle": "triangle-alert",
  user: "user",
  users: "users",
  "exclamation-circle": "circle-alert",
  "alert-circle": "circle-alert",
  check: "check",
  "info-circle": "info",
  info: "info",
  file: "file",
  "id-card": "id-card",
  comment: "message-square",
  comments: "messages-square",
  tag: "tag",
  clock: "clock",
  times: "x",
  "times-circle": "circle-x",
  inbox: "inbox",
  flag: "flag",
  dollar: "dollar-sign",
  building: "building-2",
  map: "map",
  link: "link",
  image: "image",
  images: "images",
  folder: "folder",
  "folder-open": "folder-open",
  "external-link": "external-link",
  "credit-card": "credit-card",
  car: "car",
  cog: "settings",
  minus: "minus",
  "minus-circle": "circle-minus",
  lock: "lock",
  "file-edit": "file-pen",
  "file-pdf": "file-text",
  "file-invoice": "file-text",
  "file-check": "file-check",
  circle: "circle",
  "chart-line": "chart-line",
  "chart-bar": "chart-column",
  bolt: "zap",
  wallet: "wallet",
  "user-plus": "user-plus",
  upload: "upload",
  trophy: "trophy",
  "star-fill": "star",
  sparkles: "sparkles",
  send: "send",
  "search-plus": "zoom-in",
  receipt: "receipt",
  question: "circle-help",
  "plus-circle": "circle-plus",
  phone: "phone",
  package: "package",
  history: "history",
  globe: "globe",
  directions: "navigation",
  clipboard: "clipboard",
  "chevron-right": "chevron-right",
  "angle-right": "chevron-right",
  calendar: "calendar",
  briefcase: "briefcase",
  "bell-slash": "bell-off",
  "arrows-h": "move-horizontal",
  "arrow-right": "arrow-right",
  sun: "sun",
  moon: "moon",
  eye: "eye",
  "eye-slash": "eye-off",
};

/** Fallback rendered when a name is empty or unresolved. */
export const UI_ICON_FALLBACK = "circle";

/**
 * Converts a lucide kebab-case name to its `@ng-icons/lucide` export name.
 * `chevron-down` → `lucideChevronDown`, `map-pin` → `lucideMapPin`, `building-2` → `lucideBuilding2`.
 */
export function toNgIconName(kebab: string): string {
  const pascal = kebab
    .split("-")
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join("");
  return `lucide${pascal}`;
}

/**
 * Resolves a raw icon name (legacy PrimeIcons-style or lucide kebab) to its registered
 * `@ng-icons/lucide` export name. The single mapping site shared by `ui-icon` and the nav's
 * dynamic-icon renderers.
 */
export function resolveNgIcon(name: string): string {
  const trimmed = name.replace(/^pi-?/, "");
  const kebab = (PI_TO_LUCIDE[trimmed] ?? trimmed) || UI_ICON_FALLBACK;
  return toNgIconName(kebab);
}
