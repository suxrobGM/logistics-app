---
name: pr
description: "Create a pull request with a well-structured title and description based on branch commits."
argument-hint: "[--base <branch>] [--draft]"
---

# Pull Request

Create a pull request from the current branch with a clear title and structured description based on all commits since diverging from the base branch.

## Step-by-step process

### 1. Gather context

Run these commands in parallel:

```bash
git status
git branch --show-current
git log --oneline -1 origin/HEAD 2>/dev/null || git log --oneline -1 origin/main 2>/dev/null || git log --oneline -1 origin/master
```

Determine the base branch:

- If `--base <branch>` is in `$ARGUMENTS`, use that
- Otherwise detect: `main` or `master` (whichever exists on remote)

Then run:

```bash
git log --oneline <base>..HEAD
git diff <base>...HEAD --stat
```

### 2. Analyze all commits

Read the full diff to understand the scope of changes:

```bash
git diff <base>...HEAD
```

Understand **all commits** on the branch, not just the latest one. Group related changes by area.

### 3. Check remote state

```bash
git rev-parse --abbrev-ref --symbolic-full-name @{u} 2>/dev/null
```

If the branch doesn't track a remote, or is behind, push first:

```bash
git push -u origin <current-branch>
```

### 4. Write the PR title

- Under 70 chars
- Concise, describes the overall change
- No prefix like `feat:` — save that for commits
- Examples:
  - "Fix payer and form deletion removing all records"
  - "Add shared toast helpers for RTK Query endpoints"
  - "Unify admin login through regular login page"

### 5. Write the PR body

Use this format:

```markdown
## Summary

<1-3 bullet points covering the key changes>

## Changes

<grouped list of what was changed, organized by area>

## Test plan

- [ ] <checklist of how to verify the changes>
```

**Rules:**

- Summary: high-level, 1-3 bullets, focus on **why** and **what**
- Changes: group by area (Backend, Frontend, Config, etc.) if touching multiple areas
- Test plan: actionable checklist — what to click, what API to call, what to verify
- Do NOT add `Co-Authored-By` or AI attribution
- Keep it concise — reviewers skim PRs

### 6. Create the PR

Parse `$ARGUMENTS` for flags:

- `--draft`: create as draft PR
- `--base <branch>`: set base branch

```bash
gh pr create --title "<title>" --base <base-branch> [--draft] --body "$(cat <<'EOF'
## Summary
...

## Changes
...

## Test plan
...
EOF
)"
```

### 7. Report

Output the PR URL so the user can open it.

## Examples

Small fix:

```
Title: Fix payer deletion removing all records instead of selected ones

## Summary
- Fix dangerous Prisma `deleteMany` behavior where `undefined` filter deletes all records
- Align frontend request body property names with backend expectations

## Changes
- **Backend:** `payer.ts` — read `payerIds` instead of `ids`, add `UserId` scope
- **Backend:** `form.ts` — same fix for form deletion

## Test plan
- [ ] Select 2 payers and delete — only those 2 should be removed
- [ ] Select 1 form and delete — only that form should be removed
- [ ] Verify other users' records are unaffected
```

Feature branch with multiple changes:

```
Title: Migrate RTK Query slices and fix multiple API issues

## Summary
- Fix several frontend/backend mismatches causing data loss and silent errors
- Extract shared toast helpers to reduce duplication across 8 API slices
- Unify admin login through the regular login page

## Changes
### Backend
- Fix payer and form delete endpoints to scope by `UserId`
- Add admin detection in login service
- Handle admin role in `GET /user/me`

### Frontend
- Extract `toastOnQueryStarted` and `errorOnlyOnQueryStarted` into `query-helpers.ts`
- Fix error path from `data.data.message` to `data.error.message` across all API slices
- Remove dedicated admin login page
- Fix checkout order summary payload shape

## Test plan
- [ ] Login as admin through `/auth/login` — should redirect to admin dashboard
- [ ] Delete selected payers/forms — only selected records removed
- [ ] Submit a form and trigger a backend error — error toast should display
- [ ] Complete checkout flow — order summary displays, payment succeeds
```

## Important notes

- Analyze ALL commits on the branch, not just the latest
- Do NOT add `Co-Authored-By` or AI attribution to the PR body
- Do NOT merge the PR — only create it
- If there are uncommitted changes, warn the user and stop
- If the branch is already up-to-date with an existing PR, show the existing PR URL instead
