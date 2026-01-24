# Logistics Angular Workspace

See the `angular-workspace` skill for detailed commands and patterns.

## Quick Reference

```bash
bun install                # Install dependencies
bun run start:tms          # TMS Portal: https://localhost:7003
bun run start:customer     # Customer Portal: https://localhost:7004
bun run start:website      # Website: https://localhost:7005
bun run build:all          # Build all projects
bun run gen:api            # Regenerate API client
bun run lint               # Lint code
```

## Projects

| Project           | Port | Prefix |
| ----------------- | ---- | ------ |
| `tms-portal`      | 7003 | `app-` |
| `customer-portal` | 7004 | `cp-`  |
| `website`         | 7005 | `web-` |
| `shared`          | N/A  | N/A    |

## Key Patterns

- Standalone components with separate HTML templates
- Signals + `@ngrx/signals` for state management
- `input()`/`output()` functions (not decorators)
- Native control flow (`@if`, `@for`)
