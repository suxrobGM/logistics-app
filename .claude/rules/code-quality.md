---
paths:
  - "**/*"
---

# Code Quality (all source code)

## Single Responsibility

- Split a file when it has more than one clear responsibility — not just because it crossed a line count.
- Line counts are a smell, not a verdict. Cohesive long files are fine; tangled short ones aren't.

## Lines of Code (soft targets / refactor thresholds)

| File type                                 | Aim     | Refactor at |
| ----------------------------------------- | ------- | ----------- |
| Angular `.ts` component                   | 150–250 | 300–400     |
| Angular `.html` template                  | 100–200 | 250–300     |
| React component (`.tsx`)                  | 150–250 | 300–400     |
| React hooks (`use*.ts`)                   | 50–150  | 200+        |
| Services / signal stores / NgRx / Zustand | 200–400 | 500+        |
| C# handlers / controllers / services      | 150–300 | 400+        |
| Kotlin classes                            | 150–300 | 400+        |

Generated files (`api.ts`, EF migrations, OpenAPI clients) — **ignore these rules**.

## How to Split

- **Large Angular templates** are usually the bigger smell. Extract child components, repeated row/detail blocks, dialogs, or status panels.
- **Large `.ts` files**: move workflow logic into services, computed view models, helper functions, or signal stores.
- **Large React components**: extract custom hooks for state/effects, split JSX into subcomponents, push data fetching into a hook or query layer.
- **Large C# handlers**: extract domain logic into the entity or a domain service; keep the handler thin.
- **Large services**: split along workflow seams (e.g. `expense-actions` vs `expense-api`), not arbitrary line counts.
