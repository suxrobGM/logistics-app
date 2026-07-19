# Frontend Folder Structure

## Pages

- `pages/{feature}/` = `{feature}.routes.ts` (lazy-loaded) + page folders + `store/` + `components/` (never `_components`)
- Page folder names: `{x}-list`, `{x}-add`, `{x}-edit`, `{x}-details` - detail pages are plural (`-details`, not `-detail`)
- Folder name, file basename, and exported class should agree (e.g. `tenant-quotas/tenant-quotas.ts` -> `TenantQuotas`). Don't leave a folder wrapping a self-named or mismatched file - flatten it or rename the folder to something meaningful once a sibling exists.
- Nesting caps at `pages/{feature}/{page}/` - do not add another level under a page folder. Sub-feature trees flatten instead of nesting deeper (e.g. `reports/{report-name}/`, not `reports/{report-name}/{report-name}-detail/{report-name}-detail-inner/`)
- A domain form lives in `{feature}/components/` until it has a **second** consumer outside that feature, then moves to `shared/components/domain-forms/`
- Feature-local `utils/` are allowed for helpers that don't need to be shared

## Shared components

- `shared/components/` groups by domain, not by "misc": `search`, `tags`, `maps`, `charts`, `widgets`, `domain-forms`, `integrations`, `subscription`, `documents`, `display`, `reports`
- No `other/` grab-bag. If a component doesn't fit an existing group, either it belongs in a single feature's `components/` (not shared at all), or it's the first member of a new, honestly-named group
- Each group folder has an `index.ts` barrel (`export * from "./name/name";`); the top-level `shared/components/index.ts` re-exports every group

## Portal app layout

- Portal apps (`tms-portal`, `admin-portal`, `customer-portal`): `core/` (auth, services) + `shared/` (components, layout, pipes, utils) + `pages/`
- The website app deliberately differs: top-level `layout/` + `sections/` pattern, no `core/`
