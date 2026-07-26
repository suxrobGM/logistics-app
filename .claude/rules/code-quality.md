---
paths:
  - "**/*"
---

# Code Quality (all source code)

## Single Responsibility

- Split a file when it has more than one clear responsibility - not just because it crossed a line count.
- Line counts are a smell, not a verdict. Cohesive long files are fine; tangled short ones aren't.

## Lines of Code (soft targets / refactor thresholds)

This table is the **single source** for size thresholds - `refactor-and-split` and every other doc
links here rather than restating numbers.

| File type                                | Aim     | Refactor at |
| ---------------------------------------- | ------- | ----------- |
| C# domain entity                         | 100–200 | 250         |
| C# EF configuration                      | 50–100  | 200         |
| C# command/query handler                 | 150–250 | 400         |
| C# controller                            | 150–250 | 400         |
| C# application service                   | 200–300 | 500         |
| C# infrastructure service                | 250–400 | 600         |
| C# static utility class                  | 150–250 | 400         |
| Angular `.ts` component                  | 150–250 | 350         |
| Angular `.html` template                 | 100–200 | 300         |
| Angular services / `@ngrx/signals` store | 200–400 | 500         |
| React component (`.tsx`)                 | 150–250 | 350         |
| React hooks (`use*.ts`)                  | 50–150  | 200         |
| Kotlin classes                           | 150–300 | 400         |

Generated files (`api.ts`, EF migrations, OpenAPI clients) - **ignore these rules**.

## Comments

**Default to none.** A comment earns its place by saying _why_, never _what_.

Write one only for: code that looks wrong but is right (state the constraint); a load-bearing
ordering or lifetime; an external contract (vendor quirk, spec units); a regex or formula's intent.

Never write section labels (`// Filter state`, `// Data inputs`), restatements of the identifier
(`// Get user by id`), change narration (`// Added for X`, `// was previously a switch`), or
commented-out code. A comment explaining a name means the name is wrong - rename instead.

C#: `///` on public members with a contract a caller must honour; skip them on `internal sealed`
handlers and DTOs. TypeScript: TSDoc only on exported shared-library API.

## How to Split

- **Large Angular templates** are usually the bigger smell. Extract child components, repeated row/detail blocks, dialogs, or status panels.
- **Large `.ts` files**: move workflow logic into services, computed view models, helper functions, or signal stores.
- **Large React components**: extract custom hooks for state/effects, split JSX into subcomponents, push data fetching into a hook or query layer.
- **Large C# handlers**: extract domain logic into the entity or a domain service; keep the handler thin.
- **Large services**: split along workflow seams (e.g. `expense-actions` vs `expense-api`), not arbitrary line counts.
