---
name: commit
description: "Stage changes and create a git commit with a well-crafted message following conventional commit style."
---

# Commit

Create a well-structured git commit from the current staged and unstaged changes.

## Step-by-step process

### 1. Gather context

Run these commands in parallel to understand the current state:

```bash
git status
git diff HEAD --stat
git log --oneline -5
```

### 2. Analyze the changes

Run `git diff HEAD` (or `git diff --cached` if there are only staged changes) to understand what was actually changed. Read enough context to write an accurate commit message.

### 3. Stage files

- Stage relevant files by name — prefer `git add <file1> <file2>` over `git add -A`
- Never stage files that look like secrets (`.env`, credentials, tokens)
- If the user has already staged specific files, respect their selection

### 4. Write the commit message

Follow this format:

```
<type>: <short summary in imperative mood>

<optional body — what changed and why, not how>
```

**Types:** `feat`, `fix`, `refactor`, `chore`, `docs`, `test`, `style`, `perf`, `ci`, `build`

**Rules:**

- Summary line: imperative mood, lowercase, no period, under 72 chars
- Body: wrap at 72 chars, separate from summary with a blank line
- Focus on **why** over **what** — the diff shows what changed
- If multiple areas are touched, summarize the overall intent, not every file
- Do NOT append `Co-Authored-By` or any trailer unless the user explicitly asks
- Use a HEREDOC to pass the message:

```bash
git commit -m "$(cat <<'EOF'
type: summary here

Optional body here.
EOF
)"
```

### 5. Verify

After the commit, run `git status` to confirm it succeeded.

## Examples

Single-line (small change):

```
fix: align payer delete request body with backend expected shape
```

Multi-line (larger change):

```
refactor: extract shared onQueryStarted toast helpers

Replace ~20 duplicated onQueryStarted handlers across RTK Query API
slices with shared toastOnQueryStarted and errorOnlyOnQueryStarted
helpers. Also fixes incorrect error path in all API slices.
```

## Important notes

- Do NOT add `Co-Authored-By` trailers
- Do NOT amend previous commits unless the user explicitly asks
- Do NOT push to remote — only commit locally
- If there are no changes to commit, say so and stop
- If pre-commit hooks fail, report the error — do not retry with `--no-verify`
