/*
 * @logistics/shared/ui — the shared design system.
 *
 * Presentational + form building blocks composed on top of the spartan/ui
 * primitives in `./primitives` (Helm, code-in-repo). The primitives are an
 * implementation detail behind these components and are intentionally NOT
 * re-exported here — consumers use the `ui-*` components, not raw Helm.
 */
export * from "./form";
export * from "./action";
export * from "./table";
export * from "./layout";
export * from "./containers";
export * from "./disclosure";
export * from "./badges";
export * from "./display";
export * from "./overlay";
export * from "./status";
export * from "./icons";

// The one primitive-layer export consumers need: the app-bootstrap provider that configures
// CDK overlays for the spartan/brain components (select, date-picker, …). The Helm primitives
// themselves stay private behind the `ui-*` components above.
export { provideSpartanHlm } from "./primitives/utils";
