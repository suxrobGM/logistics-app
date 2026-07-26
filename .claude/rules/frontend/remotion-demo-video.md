---
description: Conventions for the Remotion marketing/demo video project.
paths:
  - "src/Client/Logistics.DemoVideo/**/*.ts"
  - "src/Client/Logistics.DemoVideo/**/*.tsx"
---

# Demo Video (Remotion + React 19)

`src/Client/Logistics.DemoVideo/` is a standalone **Remotion** project that renders the marketing
video - `remotion studio` to preview, `remotion render` to output. It is not part of the Angular
workspace and is not in the root `package.json`. There is no Next.js and no router here.

For Remotion APIs themselves (sequencing, transitions, audio, captions, fonts, charts), use the
`remotion-best-practices` skill rather than guessing.

## Naming & exports

- **Kebab-case filenames**: `animated-text.tsx`, `scene-loads.tsx`, `use-thing.ts`. No PascalCase files.
- **Named exports only** - `export function SceneLoads()`. Nothing here has a default export.
- Barrels: `src/components/index.ts` and `src/scenes/index.ts`. Add new files to the barrel.
- Imports use the `@/` alias for `./src/*` (`import { AnimatedText } from "@/components"`).

## Components

- Declare props as a named `interface {ComponentName}Props` - never an inline object type - and
  destructure inside the function body, not in the parameter list.
- Use function declarations with an explicit `ReactElement` return type. No `React.FC`.
- Functions declared _inside_ a component body are arrow functions.

```tsx
interface AnimatedTextProps {
  text: string;
  delay: number;
}

export function AnimatedText(props: AnimatedTextProps): ReactElement {
  const { text, delay } = props;
}
```

## Remotion specifics

- **Animation is frame-driven, never time-driven.** Derive everything from `useCurrentFrame()` /
  `interpolate()` / `spring()`. A `setTimeout`, a CSS transition, or a `useEffect` timer produces a
  correct-looking preview and a broken render, because rendering evaluates frames out of order and
  in parallel.
- Composition metadata (`durationInFrames`, `fps`, `width`, `height`) lives in `src/root.tsx`.
  Scene timing is expressed with `<Sequence from={...}>`, not by mutating the composition.
- Assets resolve through Remotion's `staticFile()` from `public/`.
- Tailwind v4 is wired via `@remotion/tailwind-v4`; shared style tokens are in `src/index.css` and
  `src/lib/constants.ts`. This project has its own palette and does **not** import the Angular
  `theme.css`.

## React 19

- **Never** `useCallback`, `useMemo`, or `memo` - the React 19 compiler handles memoization.
- **Never** call `setState` synchronously inside a `useEffect` body. Derive from existing values, or
  set state only inside async callbacks and event handlers.
