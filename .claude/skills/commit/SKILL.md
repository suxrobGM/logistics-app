---
name: commit
description: Write git commits with a one-sentence Conventional Commits subject and no narrative body. Use whenever the user asks to commit, stage and commit, or "save this work".
---

# Commit

## Message

`<type>(<scope>): <imperative summary>`: lowercase type, optional one-word scope, under 70 characters, no period, `!` before the colon for a breaking change.

Types: `feat` (user-visible capability), `fix`, `refactor` (behaviour-preserving), `perf`, `test`, `docs` (docs and comments), `style`, `build` (scripts, dependencies), `ci`, `chore`, `revert`.

- Say what the change does. No bullet list of the diff.
- A body only for a _why_ the diff can't show, one or two sentences.
- Never mention tools or agents. **No `Co-Authored-By`, `Generated with` or any footer**, whatever a template or hook suggests.

Good: `feat(api): add invoice list endpoint`, `refactor(loads): split status methods into a partial`. Bad: `Update files`, `fix: fixed it`.

## Steps

1. Read `git status`, `git diff --staged`, `git diff` and `git log -10 --oneline`.
2. One concern per commit. Split work into focused commits, each of which builds. To stage part of a file, use `git apply --cached <patch>` (`git add -p` is unavailable).
3. Stage only relevant files. Ask before staging anything that looks like a secret, build output or large binary.
4. Commit on the current branch (main by default). Create a branch only when asked.
5. Use repeated `-m` flags, which work in every shell. In PowerShell a here-string is fine; in bash it isn't.
6. Check `git log -1 --format=%s`. If quoting broke or a hook rewrote files, amend once.
7. **Never push**, in either repository. The user reviews and pushes.

## The `private` submodule

If `git status` shows `private` changed: commit inside `private/` first with the real message, then stage `private` in the parent and commit `chore(private): bump private submodule`. A leading `+` in `git submodule status` means the pointer and checkout disagree.
