// ---------------------------------------------------------------------------------------------
// GENERATED FILE — DO NOT EDIT.
// Source: tools/codemods/icon-map.json (via tools/gen-icon-registry.mjs)
// Regenerate: node tools/gen-icon-registry.mjs   (runs automatically on `bun run build:shared`)
// ---------------------------------------------------------------------------------------------

import {
  lucideArrowDown,
  lucideArrowLeft,
  lucideArrowUp,
  lucideCalendar,
  lucideCheck,
  lucideChevronDown,
  lucideChevronLeft,
  lucideChevronRight,
  lucideChevronUp,
  lucideCircle,
  lucideCircleAlert,
  lucideCircleCheck,
  lucideCirclePlus,
  lucideCircleX,
  lucideEllipsisVertical,
  lucideExternalLink,
  lucideEye,
  lucideEyeOff,
  lucideFileText,
  lucideInbox,
  lucideInfo,
  lucideLoaderCircle,
  lucideLock,
  lucideLogOut,
  lucideMinus,
  lucideMoon,
  lucidePlus,
  lucideRefreshCw,
  lucideSearch,
  lucideSun,
  lucideTrash,
  lucideTriangleAlert,
  lucideX,
} from "@ng-icons/lucide";

/**
 * Every icon name a call site may write. A name outside this union is a COMPILE error at
 * `<ui-icon name="...">` (strictTemplates + `input.required<IconName>()`), which is the whole
 * point: an unmapped icon used to render a silently blank <svg>.
 */
export type UiIconName =
  | "check"
  | "check-circle"
  | "times"
  | "truck"
  | "arrow-left"
  | "plus"
  | "trash"
  | "exclamation-triangle"
  | "eye"
  | "box"
  | "send"
  | "user"
  | "users"
  | "pencil"
  | "chart-bar"
  | "ellipsis-v"
  | "map-marker"
  | "clock"
  | "arrow-right"
  | "refresh"
  | "inbox"
  | "chart-line"
  | "times-circle"
  | "download"
  | "link"
  | "search"
  | "dollar"
  | "external-link"
  | "file"
  | "info-circle"
  | "map"
  | "exclamation-circle"
  | "spinner"
  | "bolt"
  | "car"
  | "credit-card"
  | "pen-to-square"
  | "sign-out"
  | "building"
  | "cog"
  | "copy"
  | "wifi"
  | "circle"
  | "save"
  | "comment"
  | "envelope"
  | "history"
  | "list"
  | "lock"
  | "calendar"
  | "chart-pie"
  | "file-edit"
  | "id-card"
  | "image"
  | "clipboard"
  | "directions"
  | "folder"
  | "home"
  | "plus-circle"
  | "shield"
  | "tag"
  | "comments"
  | "flag"
  | "heart"
  | "minus"
  | "play"
  | "sign-in"
  | "sparkles"
  | "table"
  | "upload"
  | "user-edit"
  | "wallet"
  | "wrench"
  | "bars"
  | "briefcase"
  | "chevron-down"
  | "chevron-right"
  | "eye-slash"
  | "file-check"
  | "file-pdf"
  | "folder-open"
  | "globe"
  | "linkedin"
  | "receipt"
  | "snowflake"
  | "star"
  | "sync"
  | "alert-circle"
  | "arrow-down"
  | "arrow-up"
  | "bell-slash"
  | "book"
  | "compass"
  | "file-invoice"
  | "info"
  | "minus-circle"
  | "money-bill"
  | "moon"
  | "phone"
  | "question-circle"
  | "shopping-cart"
  | "sliders-h"
  | "sun"
  | "user-plus"
  | "warehouse"
  | "angle-right"
  | "arrows-h"
  | "ban"
  | "bell"
  | "calculator"
  | "calendar-times"
  | "check-square"
  | "chevron-left"
  | "chevron-up"
  | "cloud"
  | "facebook"
  | "file-export"
  | "file-import"
  | "file-o"
  | "filter"
  | "filter-slash"
  | "images"
  | "key"
  | "lightbulb"
  | "map-pin"
  | "mobile"
  | "package"
  | "pause-circle"
  | "paw"
  | "percentage"
  | "question"
  | "route"
  | "search-plus"
  | "server"
  | "sort-down"
  | "sort-up"
  | "star-fill"
  | "stop-circle"
  | "th-large"
  | "trophy"
  | "twitter"
  | "undo"
  | "user-minus"
  | "wifi-off"
  | "banknote"
  | "bot"
  | "circle-check"
  | "file-pen-line"
  | "house"
  | "layout-grid"
  | "mail"
  | "messages-square"
  | "settings"
  | "shield-check"
  | "trending-up";

/**
 * What an icon-name input accepts. Identical to {@link UiIconName}: S3 swept every call site onto
 * the bare canonical name, so the transitional `pi-`-prefixed spelling (LegacyPiIconName) is gone.
 * The alias is kept because it reads better at the call sites that already reference it.
 */
export type IconName = UiIconName;

/** Call-site name -> lucide (or hand-vendored `brand-*`) kebab name. The runtime map. */
export const ICON_ALIASES: Record<UiIconName, string> = {
  check: "check",
  "check-circle": "circle-check",
  times: "x",
  truck: "truck",
  "arrow-left": "arrow-left",
  plus: "plus",
  trash: "trash",
  "exclamation-triangle": "triangle-alert",
  eye: "eye",
  box: "box",
  send: "send",
  user: "user",
  users: "users",
  pencil: "pencil",
  "chart-bar": "chart-column",
  "ellipsis-v": "ellipsis-vertical",
  "map-marker": "map-pin",
  clock: "clock",
  "arrow-right": "arrow-right",
  refresh: "refresh-cw",
  inbox: "inbox",
  "chart-line": "chart-line",
  "times-circle": "circle-x",
  download: "download",
  link: "link",
  search: "search",
  dollar: "dollar-sign",
  "external-link": "external-link",
  file: "file",
  "info-circle": "info",
  map: "map",
  "exclamation-circle": "circle-alert",
  spinner: "loader-circle",
  bolt: "zap",
  car: "car",
  "credit-card": "credit-card",
  "pen-to-square": "square-pen",
  "sign-out": "log-out",
  building: "building-2",
  cog: "settings",
  copy: "copy",
  wifi: "wifi",
  circle: "circle",
  save: "save",
  comment: "message-square",
  envelope: "mail",
  history: "history",
  list: "list",
  lock: "lock",
  calendar: "calendar",
  "chart-pie": "chart-pie",
  "file-edit": "file-pen",
  "id-card": "id-card",
  image: "image",
  clipboard: "clipboard",
  directions: "navigation",
  folder: "folder",
  home: "home",
  "plus-circle": "circle-plus",
  shield: "shield",
  tag: "tag",
  comments: "messages-square",
  flag: "flag",
  heart: "heart",
  minus: "minus",
  play: "play",
  "sign-in": "log-in",
  sparkles: "sparkles",
  table: "table",
  upload: "upload",
  "user-edit": "user-pen",
  wallet: "wallet",
  wrench: "wrench",
  bars: "menu",
  briefcase: "briefcase",
  "chevron-down": "chevron-down",
  "chevron-right": "chevron-right",
  "eye-slash": "eye-off",
  "file-check": "file-check",
  "file-pdf": "file-text",
  "folder-open": "folder-open",
  globe: "globe",
  linkedin: "brand-linkedin",
  receipt: "receipt",
  snowflake: "snowflake",
  star: "star",
  sync: "refresh-cw",
  "alert-circle": "circle-alert",
  "arrow-down": "arrow-down",
  "arrow-up": "arrow-up",
  "bell-slash": "bell-off",
  book: "book",
  compass: "compass",
  "file-invoice": "file-text",
  info: "info",
  "minus-circle": "circle-minus",
  "money-bill": "banknote",
  moon: "moon",
  phone: "phone",
  "question-circle": "circle-help",
  "shopping-cart": "shopping-cart",
  "sliders-h": "sliders-horizontal",
  sun: "sun",
  "user-plus": "user-plus",
  warehouse: "warehouse",
  "angle-right": "chevron-right",
  "arrows-h": "move-horizontal",
  ban: "ban",
  bell: "bell",
  calculator: "calculator",
  "calendar-times": "calendar-x",
  "check-square": "check-square",
  "chevron-left": "chevron-left",
  "chevron-up": "chevron-up",
  cloud: "cloud",
  facebook: "brand-facebook",
  "file-export": "file-output",
  "file-import": "file-input",
  "file-o": "file",
  filter: "filter",
  "filter-slash": "filter-x",
  images: "images",
  key: "key",
  lightbulb: "lightbulb",
  "map-pin": "map-pin",
  mobile: "smartphone",
  package: "package",
  "pause-circle": "pause-circle",
  paw: "paw-print",
  percentage: "percent",
  question: "circle-help",
  route: "route",
  "search-plus": "zoom-in",
  server: "server",
  "sort-down": "arrow-down",
  "sort-up": "arrow-up",
  "star-fill": "star",
  "stop-circle": "stop-circle",
  "th-large": "layout-grid",
  trophy: "trophy",
  twitter: "brand-x",
  undo: "undo",
  "user-minus": "user-minus",
  "wifi-off": "wifi-off",
  banknote: "banknote",
  bot: "bot",
  "circle-check": "circle-check",
  "file-pen-line": "file-pen-line",
  house: "house",
  "layout-grid": "layout-grid",
  mail: "mail",
  "messages-square": "messages-square",
  settings: "settings",
  "shield-check": "shield-check",
  "trending-up": "trending-up",
};

/** Icons every portal registers: what the shared library itself renders, plus the error glyph. */
export const BASE_ICON_NAMES: readonly UiIconName[] = [
  "alert-circle",
  "arrow-left",
  "calendar",
  "check",
  "check-circle",
  "chevron-down",
  "chevron-left",
  "chevron-right",
  "chevron-up",
  "circle",
  "ellipsis-v",
  "exclamation-triangle",
  "external-link",
  "eye",
  "eye-slash",
  "file-pdf",
  "inbox",
  "info-circle",
  "lock",
  "minus",
  "moon",
  "plus",
  "plus-circle",
  "refresh",
  "search",
  "sign-out",
  "sort-down",
  "sort-up",
  "spinner",
  "sun",
  "times",
  "times-circle",
  "trash",
];

/** Every icon name `tms-portal` writes (base included). Used by check-icons.mjs and the ui-lab. */
export const TMS_ICON_NAMES: readonly UiIconName[] = [
  "alert-circle",
  "angle-right",
  "arrow-down",
  "arrow-left",
  "arrow-right",
  "arrow-up",
  "arrows-h",
  "ban",
  "banknote",
  "bars",
  "bell",
  "bell-slash",
  "bolt",
  "book",
  "box",
  "briefcase",
  "building",
  "calendar",
  "calendar-times",
  "car",
  "chart-bar",
  "chart-line",
  "chart-pie",
  "check",
  "check-circle",
  "check-square",
  "chevron-down",
  "chevron-left",
  "chevron-right",
  "circle",
  "circle-check",
  "clipboard",
  "clock",
  "cloud",
  "cog",
  "comment",
  "comments",
  "compass",
  "copy",
  "credit-card",
  "directions",
  "dollar",
  "download",
  "ellipsis-v",
  "envelope",
  "exclamation-circle",
  "exclamation-triangle",
  "external-link",
  "eye",
  "file",
  "file-check",
  "file-edit",
  "file-export",
  "file-import",
  "file-invoice",
  "file-o",
  "file-pdf",
  "file-pen-line",
  "filter",
  "filter-slash",
  "flag",
  "folder",
  "folder-open",
  "globe",
  "heart",
  "history",
  "home",
  "house",
  "id-card",
  "image",
  "images",
  "inbox",
  "info",
  "info-circle",
  "key",
  "link",
  "list",
  "lock",
  "map",
  "map-marker",
  "map-pin",
  "messages-square",
  "minus",
  "minus-circle",
  "money-bill",
  "moon",
  "package",
  "pause-circle",
  "paw",
  "pen-to-square",
  "pencil",
  "percentage",
  "phone",
  "play",
  "plus",
  "plus-circle",
  "question",
  "question-circle",
  "refresh",
  "route",
  "save",
  "search",
  "search-plus",
  "send",
  "server",
  "settings",
  "shield",
  "shopping-cart",
  "sign-in",
  "sign-out",
  "sliders-h",
  "snowflake",
  "sort-down",
  "sort-up",
  "sparkles",
  "spinner",
  "star",
  "stop-circle",
  "sun",
  "sync",
  "table",
  "tag",
  "th-large",
  "times",
  "times-circle",
  "trash",
  "trending-up",
  "trophy",
  "truck",
  "undo",
  "upload",
  "user",
  "user-edit",
  "user-plus",
  "users",
  "wallet",
  "warehouse",
  "wifi",
  "wifi-off",
  "wrench",
];

/** Every icon name `admin-portal` writes (base included). Used by check-icons.mjs and the ui-lab. */
export const ADMIN_ICON_NAMES: readonly UiIconName[] = [
  "arrow-left",
  "arrow-right",
  "bars",
  "bot",
  "box",
  "building",
  "chart-bar",
  "check",
  "cog",
  "credit-card",
  "ellipsis-v",
  "envelope",
  "eye",
  "eye-slash",
  "file-pen-line",
  "house",
  "inbox",
  "info",
  "layout-grid",
  "lock",
  "mail",
  "pen-to-square",
  "plus",
  "refresh",
  "save",
  "send",
  "shield",
  "shield-check",
  "sign-in",
  "sign-out",
  "star-fill",
  "times",
  "times-circle",
  "trash",
  "user",
  "user-minus",
  "users",
];

/** Every icon name `customer-portal` writes (base included). Used by check-icons.mjs and the ui-lab. */
export const CUSTOMER_ICON_NAMES: readonly UiIconName[] = [
  "arrow-left",
  "arrow-right",
  "box",
  "building",
  "check",
  "check-circle",
  "chevron-right",
  "circle",
  "clock",
  "cog",
  "download",
  "exclamation-triangle",
  "eye",
  "file",
  "flag",
  "folder-open",
  "home",
  "inbox",
  "info-circle",
  "lock",
  "map",
  "map-marker",
  "question",
  "receipt",
  "send",
  "sign-in",
  "sign-out",
  "times",
  "truck",
  "user",
  "user-edit",
];

/** Every icon name `website` writes (base included). Used by check-icons.mjs and the ui-lab. */
export const WEBSITE_ICON_NAMES: readonly UiIconName[] = [
  "arrow-down",
  "arrow-left",
  "arrow-right",
  "bars",
  "bolt",
  "book",
  "box",
  "briefcase",
  "building",
  "calculator",
  "calendar",
  "car",
  "chart-bar",
  "check",
  "check-circle",
  "chevron-down",
  "clock",
  "comments",
  "compass",
  "credit-card",
  "directions",
  "envelope",
  "exclamation-circle",
  "exclamation-triangle",
  "external-link",
  "eye",
  "facebook",
  "globe",
  "heart",
  "history",
  "home",
  "image",
  "inbox",
  "lightbulb",
  "linkedin",
  "map",
  "map-marker",
  "mobile",
  "phone",
  "play",
  "receipt",
  "search",
  "send",
  "shield",
  "sparkles",
  "spinner",
  "times",
  "truck",
  "twitter",
  "user",
  "user-edit",
  "users",
  "wallet",
];

/**
 * The icons every portal must register. Merge with the app's own set:
 *   provideIcons({ ...BASE_NG_ICONS, ...TMS_NG_ICONS })
 */
export const BASE_NG_ICONS = {
  lucideArrowDown,
  lucideArrowLeft,
  lucideArrowUp,
  lucideCalendar,
  lucideCheck,
  lucideChevronDown,
  lucideChevronLeft,
  lucideChevronRight,
  lucideChevronUp,
  lucideCircle,
  lucideCircleAlert,
  lucideCircleCheck,
  lucideCirclePlus,
  lucideCircleX,
  lucideEllipsisVertical,
  lucideExternalLink,
  lucideEye,
  lucideEyeOff,
  lucideFileText,
  lucideInbox,
  lucideInfo,
  lucideLoaderCircle,
  lucideLock,
  lucideLogOut,
  lucideMinus,
  lucideMoon,
  lucidePlus,
  lucideRefreshCw,
  lucideSearch,
  lucideSun,
  lucideTrash,
  lucideTriangleAlert,
  lucideX,
};
