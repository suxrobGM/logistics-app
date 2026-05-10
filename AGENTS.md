# AGENTS.md

This repository uses Claude Code guidance as the single source of truth.

Codex and other coding agents should not duplicate project rules here. Instead, load and follow the existing Claude files:

1. Read [CLAUDE.md](CLAUDE.md) first for repository-wide architecture, commands, ports, and workflow notes.
2. Before searching broadly by feature, read [.claude/feature-map.md](.claude/feature-map.md).
3. When working inside a directory that has its own `CLAUDE.md`, read that file as additional local guidance. For example, Angular work under `src/Client/Logistics.Angular/` must also follow [src/Client/Logistics.Angular/CLAUDE.md](src/Client/Logistics.Angular/CLAUDE.md).
4. Apply the relevant rule files from `.claude/rules/` based on the files being changed:
   - Backend C# / API / tests: `.claude/rules/backend/`
   - Angular frontend: `.claude/rules/frontend/angular-conventions.md` and `.claude/rules/frontend/angular-security.md`
   - React frontend: `.claude/rules/frontend/react-conventions.md`
   - Kotlin driver app: `.claude/rules/mobile/kotlin-driver-app.md`
5. Reuse skills from `.claude/skills/`. Each skill folder's `SKILL.md` is authoritative; do not copy skill instructions into `.agents/skills/` or this file. If an agent runtime needs skills under `.agents/skills/` for discovery, add thin forwarding `SKILL.md` shims that point back to `.claude/skills/<skill-name>/SKILL.md` using `@` imports. Filesystem links or directory junctions are also acceptable, but shims are preferred on Windows.
6. When adding or changing a top-level feature, update [.claude/feature-map.md](.claude/feature-map.md) so Claude Code remains authoritative.

If this file and the Claude files ever disagree, treat the Claude files as authoritative and update this file only to preserve that delegation.
