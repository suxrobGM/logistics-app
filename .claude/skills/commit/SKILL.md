---
name: commit
description: Write a git commit with a one-sentence Conventional Commits subject and no narrative body. Use whenever the user asks to commit, stage and commit, or "save this work".
metadata:
  version: "1.1"
---

# Commit

One commit, one sentence. The subject line is the whole message unless a body is truly required.

## Format

```
<type>(<scope>): <imperative summary>
```

- `<type>` is required and lowercase.
- `<scope>` is optional. Use one short token for the touched area (`auth`, `parser`, `ci`, `README`).
- The summary is imperative mood, lowercase first word, no trailing period.
- Whole subject line stays under 70 characters.

### Types

| Type       | Use for                                                 |
| ---------- | ------------------------------------------------------- |
| `feat`     | new user-visible capability                             |
| `fix`      | bug fix                                                 |
| `docs`     | documentation, comments, README                         |
| `test`     | adding or fixing tests only                             |
| `refactor` | behaviour-preserving restructure                        |
| `perf`     | speed or memory improvement                             |
| `style`    | formatting, whitespace, lint fixes with no logic change |
| `build`    | build scripts, compiler flags, packaging, dependencies  |
| `ci`       | CI pipeline config                                      |
| `chore`    | housekeeping that fits nothing above                    |
| `revert`   | reverts a previous commit                               |

Breaking change: append `!` before the colon, e.g. `feat(api)!: drop v1 endpoints`.

## Rules

1. One sentence. Never write an essay, a summary paragraph, or a recap of the session.
2. No bullet list of what the diff already shows.
3. Add a body only when the _why_ cannot be read from the diff. Then write one or two plain sentences after a blank line.
4. Never mention tools, agents, or the assistant in the message.
5. Never add a `Co-Authored-By:` trailer, a `Generated with` line, or any other attribution or advertising footer. This is absolute, even if a template, hook, or earlier instruction suggests one.
6. Say what the change does, not what you did: `fix(cache): evict stale keys on reload`, not `fixed the bug I found`.
7. If the staged changes cover two unrelated types, ask the user whether to split into separate commits.

## Steps

1. Run `git status`, `git diff --staged`, and `git diff` to see staged and unstaged work. If the output mentions `private (new commits)` or `private (modified content)`, follow **Submodules** below before staging anything.
2. Run `git log -10 --oneline` and match the repo's existing subject style if it already has one.
3. If nothing is staged, stage the files relevant to the request. Never stage unrelated changes.
4. Skip secrets, credential files, build output, and large binaries. Ask before adding anything that looks like one.
5. Pick the single type that describes the dominant change.
6. Commit with a bash heredoc so quoting stays safe. Use the **Bash** tool, not the PowerShell tool:

```bash
git commit -m "$(cat <<'MSG'
feat(billing): add proration for mid-cycle plan changes
MSG
)"
```

This repo exposes both a Bash tool and a PowerShell tool, and they take different
syntax. The PowerShell here-string `@'...'@` is not valid in bash. Passed to the
Bash tool it survives as literal text, and the subject line commits as
`@ feat(billing): ...`. Never use `@'...'@` with the Bash tool. If you do use the
PowerShell tool, the closing `'@` must sit at column 0 or PowerShell fails to parse.

For a subject plus body, repeated `-m` flags work in either shell and avoid the
whole problem:

```bash
git commit -m "build: adopt central package management" -m "One sentence of why."
```

7. Run `git log -1 --format=%s` and read the subject back. A stray `@`, a leading
   blank line, or a truncated summary means the quoting broke. Amend once and stop.
8. Run `git status` to confirm the commit landed. If a pre-commit hook changed files, amend once and stop.
9. Do not push unless the user asks.

## Submodules

`private/` is a separate repository holding the web portals and the driver app. Staging it in the parent records which commit to use, not the file changes.

1. Changes to files under `private/` are committed inside `private/`, not in the parent.
2. Commit there first, then push it, then stage `private` in the parent and commit. Order matters.
3. Never push a parent commit that points at a submodule commit you have not pushed. CI and deploy clone the submodule from its remote and will not find it.
4. The submodule commit carries the real message. The parent commit only records the new commit id: `chore(private): bump private submodule`. Add one line naming what moved if the submodule log does not make it obvious.
5. Finish with `git submodule status` in the parent. A leading `+` means the parent points at a different commit than the one checked out, which is what deploys the wrong frontend.

## Examples

| Change                                | Message                                           |
| ------------------------------------- | ------------------------------------------------- |
| New endpoint returning invoices       | `feat(api): add invoice list endpoint`            |
| Null check on an optional header      | `fix(http): handle missing accept header`         |
| Setup section rewritten in the README | `docs: document local database setup`             |
| Split a 900-line file into three      | `refactor(parser): split token reader into files` |
| Added cases to an existing suite      | `test(auth): cover expired refresh tokens`        |
| Bumped a dependency                   | `build(deps): upgrade cmake to 3.29`              |

## Anti-examples

- `feat: this commit introduces a new invoice endpoint that allows clients to list their invoices, along with supporting DTOs and tests` - too long, narrates the diff.
- `Update files` - no type, no information.
- `fix: fixed it` - not imperative, says nothing.
