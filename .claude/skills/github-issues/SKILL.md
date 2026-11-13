---
name: github-issues
description: "Create GitHub issues from user requirements, a PRD document, or conversation context, and add them to a GitHub Project with priority, size, and type fields."
argument-hint: "[source-file-path] [--project <number>]"
---

# GitHub Issues

Turn a PRD, chat requirements or a task breakdown into GitHub issues, then add them to a Project with Priority, Size and Type. If `$ARGUMENTS` has a file path, read it; otherwise use the conversation. If there is no project number, detect it or ask. If the repo has no project, offer issues only.

## 1. Understand the requirements

Extract features, flows, screens, technical needs and integrations. Split into work items one developer can own. Ask if scope is ambiguous.

## 2. Load or create the config

Cache: `.claude/skills/github-issues/.github-project-config.json`. If it exists, use its IDs and skip discovery. If a field ID call fails, the cache is stale: delete it and rediscover.

Otherwise run discovery, save the result there, and make sure the file is gitignored (it holds environment-specific IDs, not secrets).

```bash
gh repo view --json name,url,owner          # owner type: gh api users/<login> --jq .type
gh label list
gh project list --owner <owner>             # use --project arg; one project: use it; several: ask
```

Project fields and option IDs (`<ownerType>` is `organization` or `user`):

```bash
gh api graphql -f query='{
  <ownerType>(login: "<owner>") {
    projectV2(number: <N>) {
      id
      fields(first: 30) { nodes { ... on ProjectV2SingleSelectField { name id options { id name } } } }
    }
  }
}'
```

Repo issue types:

```bash
gh api graphql -f query='{
  <ownerType>(login: "<owner>") {
    repository(name: "<repo>") { issueTypes(first: 20) { nodes { id name description isEnabled } } }
  }
}'
```

Config format. Only `repo` is required; omit `priority`, `size`, `issueTypes` if the project lacks them:

```json
{
  "repo": { "name": "repo-name", "owner": "org-or-user", "ownerType": "organization | user" },
  "project": { "number": 1, "id": "PVT_..." },
  "fields": {
    "status": { "id": "PVTSSF_...", "options": { "Backlog": "id", "Todo": "id", "In Progress": "id", "Done": "id" } },
    "priority": { "id": "PVTSSF_...", "options": { "P0": "id", "P1": "id", "P2": "id" } },
    "size": { "id": "PVTSSF_...", "options": { "XS": "id", "S": "id", "M": "id", "L": "id", "XL": "id" } }
  },
  "issueTypes": { "Task": "IT_...", "Bug": "IT_...", "Feature": "IT_..." },
  "labels": ["backend", "frontend", "bug", "enhancement"]
}
```

## 3. Plan, then confirm

Show a table grouped by area (Backend, Web, Mobile, Database, Infra, Docs):

| #   | Title | Labels | Priority | Size | Type |
| --- | ----- | ------ | -------- | ---- | ---- |

**Do not create anything until the user confirms.** Let them edit or drop rows.

## 4. Labels

Create only labels missing from the config's `labels`, then update the config. Never create priority labels; use the project field.

```bash
gh label create "<name>" -c "<color>" -d "<description>"
```

## 5. Create issues

```bash
gh issue create --title "<title>" --label "<l1>,<l2>" --body "$(cat <<'EOF'
## Description
<what to build>

## Requirements
<specific bullets>

## Implementation notes
<technical details, code to reuse, file paths>

## Files
<files to create or modify>
EOF
)"
```

Title: concise and action-oriented ("Student auth: OTP send & verify flow"). Describe what, not how. Include endpoints, screens or data models, and reference existing code. Each issue is self-contained.

## 6. Add to the project

```bash
gh project item-add <project.number> --owner <repo.owner> --url <issue-url>
```

Keep the returned item IDs.

## 7. Set fields

Use config IDs; skip fields the config lacks. Priority and Size:

```bash
gh project item-edit --project-id <project.id> --id <item-id> \
  --field-id <fields.priority.id> --single-select-option-id <fields.priority.options[value]>
```

Issue type needs the issue node ID and a mutation. Put the mutation in a temp file to avoid shell escaping:

```bash
gh api graphql -f query='query { repository(owner: "<repo.owner>", name: "<repo.name>") { issue(number: <N>) { id } } }'

cat > "${TMPDIR:-/tmp}/gql_query.txt" << 'GQL'
mutation($issueId: ID!, $typeId: ID!) {
  updateIssueIssueType(input: {issueId: $issueId, issueTypeId: $typeId}) { issue { number } }
}
GQL
gh api graphql -F query=@"${TMPDIR:-/tmp}/gql_query.txt" -F issueId=<issue-node-id> -F typeId=<issueTypes[value]>
```

If `item-edit` fails on scope, tell the user to run `gh auth refresh -s project` and retry.

## 8. Verify and report

`gh issue list --limit 100 --state open`. Report totals by area, Priority and Size counts, and the project link.

## Guides

| Size   | Scope                                                 |
| ------ | ----------------------------------------------------- |
| **XS** | Simple CRUD, single config, small copy change         |
| **S**  | One endpoint or screen, straightforward logic         |
| **M**  | Several endpoints or screens, moderate complexity     |
| **L**  | Complex feature across multiple files or systems      |
| **XL** | Large cross-cutting feature, significant architecture |

| Priority | Meaning                                  |
| -------- | ---------------------------------------- |
| **P0**   | Must-have for launch, blocks other work  |
| **P1**   | Important, build after P0                |
| **P2**   | Nice to have, can ship without it        |

| Type        | Use for                                                            |
| ----------- | ------------------------------------------------------------------ |
| **Feature** | New user-facing functionality, API endpoints, UI screens           |
| **Task**    | Setup, configuration, CI/CD, documentation, testing infrastructure |
| **Bug**     | Only fixing existing broken behavior                               |
