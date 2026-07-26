---
name: add-angular-page
description: Add a new page or route to a portal app (tms-portal, admin-portal, customer-portal) - folder, lazy route, permission, sidebar entry, and role access. Use when adding a screen to an existing feature or a new feature area in the frontend, without necessarily adding a backend entity. Covers the four independent nav filters that each silently hide a page.
---

# Add an Angular page

Portals: `tms-portal` (dispatchers), `customer-portal` (shippers), `admin-portal` (super admin).
Layout rules are in `.claude/rules/frontend/folder-structure.md`; component conventions in
`angular-conventions.md`. This skill is the wiring.

**The thing that goes wrong:** a page's visibility passes through four independent filters. Miss one
and the page is reachable by URL but absent from the menu, or in the menu but 403s. Nothing fails at
build time and no test catches it.

| Filter                   | Where                                                       | Symptom when missed                  |
| ------------------------ | ----------------------------------------------------------- | ------------------------------------ |
| Route registered         | `app.routes.ts`                                             | 404                                  |
| Permission on route data | `{feature}.routes.ts`                                       | reachable by anyone / redirected     |
| Nav item declared        | `shared/layout/sidebar/sidebar-items.ts`                    | no menu entry                        |
| Role allowlist           | `core/services/sidebar-nav.service.ts` → `ROLE_ITEM_ACCESS` | no menu entry **for that role only** |

## 1. Page folder

Adding to an existing feature → drop a page folder into `pages/{feature}/`.
New feature area → `pages/{feature}/` with `{feature}.routes.ts`, page folders, optional `store/`
and `components/`.

Naming: `{x}-list`, `{x}-add`, `{x}-edit`, `{x}-details` (**plural** on details). Folder name, file
basename, and class agree. Nesting stops at `pages/{feature}/{page}/`.

## 2. Route

In `pages/{feature}/{feature}.routes.ts`:

```ts
{
  path: "detailed",
  loadComponent: () => import("./thing-details/thing-details").then((m) => m.ThingDetails),
  canActivate: [authGuard],
  data: { breadcrumb: "", permission: Permission.Thing.View },
}
```

`authGuard` reads `data.permission` (`projects/shared/src/lib/auth/auth-guard.factory.ts`). A route
with no `permission` is open to any authenticated user - that is a decision, make it deliberately.
If the permission doesn't exist yet, use the `add-permission` skill.

Then register the feature's routes lazily in `app.routes.ts` (skip if the feature already is).

## 3. Sidebar entry

`shared/layout/sidebar/sidebar-items.ts` - add a `NavItem` with a stable `id`, `icon` (a typed
`IconName`), `route`, and where relevant a `permission` and/or `feature`.

Then add that same `id` to every role that should see it in `ROLE_ITEM_ACCESS`
(`core/services/sidebar-nav.service.ts`). This allowlist runs **before** the feature-flag and
permission filters, so a missing id hides the item no matter what else is set. `"*"` means all items.

Use `menuHidden` for a page that is routable but intentionally not in the menu (detail pages reached
from a list).

## 4. Data access

API calls go through the generated client in `projects/shared/src/lib/api/generated/`. If the
backend endpoint is new, regenerate first:

```bash
bun run gen:api:live      # needs the API running on 7000
```

Never hand-edit anything under `generated/`.

If the page shows data pushed over SignalR, add a `ttl: 0` rule to
`projects/shared/src/lib/api/cache.config.ts` **before** the catch-all - otherwise the interceptor
serves a 2-minute-stale response over your live updates.

## 5. Building the page

- Shared `ui-*` components before hand-rolled Tailwind; browse `/ui-lab` first.
- Forms: `<ui-form-field>` + the `*-field` controls + `ValidatedForm`. See the Angular `CLAUDE.md`.
- Semantic theme tokens only - never `bg-white` / `text-gray-600`.
- Feature-gated page? Guard the route with the feature flag and use `FeatureService` in the template
  (`add-tenant-feature-flag`).

## Checklist

- [ ] Page folder follows the `{x}-list` / `-details` naming; class, file, folder agree
- [ ] Route added to `{feature}.routes.ts` with `canActivate: [authGuard]` and `data.permission`
- [ ] Feature routes lazy-registered in `app.routes.ts`
- [ ] `NavItem` added to `sidebar-items.ts` with a stable `id` (or `menuHidden` deliberately)
- [ ] That `id` added to every relevant role in `ROLE_ITEM_ACCESS`
- [ ] `bun run gen:api` run if the backend endpoint is new
- [ ] `ttl: 0` cache rule added for SignalR-backed data
- [ ] Shared `ui-*` components used; no hardcoded colours
- [ ] `bun run lint` and `bun run check` pass
- [ ] Verified in the running app **as each role that should and should not see it**

Test as a role that _shouldn't_ see it, and as one that should but isn't Owner - Owner is usually
`"*"` in `ROLE_ITEM_ACCESS`, so it passes even when the entry is missing.

## Related

- `.claude/rules/frontend/folder-structure.md`, `angular-conventions.md`
- `add-permission`, `add-tenant-feature-flag`, `signal-forms-reference`
- `scaffold-feature` - if you also need the backend slice
