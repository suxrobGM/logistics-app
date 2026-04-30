---
description: Frontend code conventions when working with React files.
paths:
  - src/Client/Logistics.DemoVideo/*
---

## File Naming

- **Kebab-case** for all files: `app-shell.tsx`, `use-auth.ts`, `auth-card.tsx`
- No PascalCase filenames

## Exports

- **Named exports** for all components, hooks, providers: `export function Sidebar()`
- **Default exports** only for Next.js pages and layouts (`page.tsx`, `layout.tsx`)

## Component Props

- Always declare props as a named `interface {ComponentName}Props` — never inline object types
- Destructure props inside the function body, not in parameters

```typescript
interface SidebarProps {
  open: boolean;
  onToggle: () => void;
}

function Sidebar(props: SidebarProps): ReactElement {
  const { open, onToggle } = props;
}
```

## Component Definition

- Use **function declarations** for components, not arrow functions with explicit return type `ReactElement`:

```typescript
function Sidebar(): ReactElement {
  return <div>...</div>;
}
```

- Avoid using `React.FC` or `React.FunctionComponent` types for components.
- When you declare a function inside the component body, arrow functions instead of function declarations:

```typescript
function Sidebar(): ReactElement {
  const handleClick = () => {
    // ...
  };
}
```

## Path Aliases

tsconfig uses `"@/*": ["./src/*"]`. Imports use `@/` without `src/`.

## React 19

- Use `use()` hook for async data in client components instead of `useEffect` + `useState` pattern
- **Never** use `useCallback`, `useMemo`, or `memo` — the React 19 compiler handles memoization
- **Never** call `setState` synchronously inside a `useEffect` body — derive state from existing values, or call `setState` only inside async callbacks (`.then()`, event handlers)
