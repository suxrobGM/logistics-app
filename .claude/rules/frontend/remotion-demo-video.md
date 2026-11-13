---
paths:
  - "src/Client/Logistics.DemoVideo/**/*.ts"
  - "src/Client/Logistics.DemoVideo/**/*.tsx"
---

# Demo Video (Remotion + React 19)

A standalone Remotion project, the only workspace in the root `package.json` (`bun dev:video`, `bun build:video`). Not part of the Angular workspace; its own palette, no `theme.css`. Check Remotion APIs at remotion.dev.

- Kebab-case filenames, named exports only, barrels in `src/components/index.ts` and `src/scenes/index.ts`, `@/` alias for `src/`.
- Components: function declarations returning `ReactElement`, props as a named `interface XProps` destructured in the body. No `React.FC`.
- **Animation is frame-driven.** Derive everything from `useCurrentFrame()`, `interpolate()`, `spring()`. Timers, CSS transitions and effect timers look right in preview and break the render, which evaluates frames out of order.
- Composition metadata is in `src/root.tsx`; timing uses `<Sequence from>`. Assets go through `staticFile()`.
- React 19 compiler: no `useCallback`, `useMemo` or `memo`. No synchronous `setState` in an effect body.
