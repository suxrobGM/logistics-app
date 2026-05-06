---
name: github-issues
description: "Create GitHub issues from user requirements, a PRD document, or conversation context, and add them to a GitHub Project with priority, size, and type fields."
argument-hint: "[source-file-path] [--project <number>]"
---

# GitHub Issues

Create well-structured GitHub issues from any input — a PRD document, a requirements description in chat, feature ideas, or a task breakdown — then add them to a GitHub Project board with Priority, Size, and Type metadata.

## Input

The user may provide:

- A path to a document (PDF, Markdown, text) via `$ARGUMENTS`
- Requirements described directly in the conversation
- A mix of both

If `$ARGUMENTS` contains a file path, read that file. Otherwise, work from what the user described in chat. If no project number is given, detect it or ask.

## Step-by-step process

### 1. Understand the requirements

- If a file path is provided in `$ARGUMENTS`, read and analyze it
- If requirements are described in conversation, use that context
- Extract: features, user flows, screens, technical requirements, integrations
- Identify distinct work items that can each be assigned to one developer
- Ask clarifying questions if the scope is ambiguous

### 2. Load or create project configuration

Look for a cached config file at this path (colocated with this skill):

```
.claude/skills/github-issues/.github-project-config.json
```

**If the file exists**, read it and use all IDs directly — skip all GitHub API discovery calls.

**If the file does NOT exist**, run the full discovery flow below, then **save the results** to that path so future runs are instant. Also ensure `.github-project-config.json` is in the repo's `.gitignore` (the pattern `**.github-project-config.json` or the exact path).

#### Discovery flow (first run only)

**a) Detect repo info:**

```bash
gh repo view --json name,url,owner
```

Determine if the owner is a `user` or `organization` (check the `owner.id` prefix — `O_` = org, `U_` = user, or use `gh api users/<login> --jq .type`).

**b) List labels:**

```bash
gh label list
```

**c) Find the project:**

```bash
gh project list --owner <owner>
```

If a project number was provided in args, use it. If multiple projects exist, ask the user. If only one, use it automatically.

**d) Query project fields (Priority, Size, Status) and their option IDs:**

```bash
gh api graphql -f query='{
  <ownerType>(login: "<owner>") {
    projectV2(number: <N>) {
      id
      fields(first: 30) {
        nodes {
          ... on ProjectV2SingleSelectField {
            name
            id
            options { id name }
          }
        }
      }
    }
  }
}'
```

**e) Query repo-level issue types:**

```bash
gh api graphql -f query='{
  <ownerType>(login: "<owner>") {
    repository(name: "<repo>") {
      issueTypes(first: 20) {
        nodes { id name description isEnabled }
      }
    }
  }
}'
```

**f) Save config:** Write all collected data to `.github-project-config.json` using the format in the "Config file format" section. Only include fields that actually exist on the project — omit `priority`, `size`, or `issueTypes` if the project doesn't have them.

### 3. Plan the issues

Before creating anything, present the user with a summary table:

| #   | Title | Labels | Priority | Size | Type |
| --- | ----- | ------ | -------- | ---- | ---- |

Group issues by area (Backend, Web, Mobile, Database, Infra, Docs, etc.).

**Ask the user to confirm** before proceeding with creation. Let them adjust titles, priorities, sizes, or remove issues.

### 4. Create labels (if needed)

Check the `labels` array in the config. Only create labels that don't already exist:

```bash
gh label create "<name>" -c "<color>" -d "<description>"
```

After creating new labels, update the config file's `labels` array.

Do NOT create priority labels (P0/P1/P2) — use project fields instead.

### 5. Create issues

For each issue, use `gh issue create` with a heredoc body:

```bash
gh issue create --title "<title>" --label "<label1>,<label2>" --body "$(cat <<'EOF'
## Description
<clear description of what needs to be built>

## Requirements
<bullet list of specific requirements>

## Implementation notes
<relevant technical details, existing code to reuse, file paths>

## Files
<list of files to create or modify>
EOF
)"
```

Guidelines for issue content:

- Title: concise, action-oriented (e.g., "Student auth: OTP send & verify flow")
- Description: what to build, not how (leave implementation to the developer)
- Include relevant API endpoints, screen descriptions, or data models
- Reference existing code/files when applicable
- Keep each issue self-contained and assignable to one developer

### 6. Add issues to the project

Use `project.number` and `repo.owner` from the config:

```bash
gh project item-add <project.number> --owner <repo.owner> --url <issue-url>
```

Collect the returned item IDs for field assignment in the next step.

### 7. Set project fields (Priority, Size, Type)

Use IDs directly from the config — no GraphQL lookups needed. Skip any field that doesn't exist in the config.

For Priority and Size, use `gh project item-edit`:

```bash
gh project item-edit --project-id <project.id> --id <item-id> \
  --field-id <fields.priority.id> --single-select-option-id <fields.priority.options[value]>
```

For issue types (Feature, Task, Bug), write the GraphQL mutation to a temp file to avoid shell escaping issues:

```bash
cat > /tmp/gql_query.txt << 'GQL'
mutation($issueId: ID!, $typeId: ID!) {
  updateIssueIssueType(input: {issueId: $issueId, issueTypeId: $typeId}) {
    issue { number }
  }
}
GQL
gh api graphql -F query=@/tmp/gql_query.txt -F issueId=<issue-node-id> -F typeId=<issueTypes[value]>
```

To get the issue node ID for type assignment:

```bash
gh api graphql -f query='query { repository(owner: "<repo.owner>", name: "<repo.name>") { issue(number: <N>) { id } } }'
```

If the `project` scope is missing for item-edit, inform the user to run `gh auth refresh -s project` and retry.

### 8. Verify and report

After all issues are created:

```bash
gh issue list --limit 100 --state open
```

Present a final summary to the user with:

- Total issues created, grouped by area
- Priority breakdown (P0/P1/P2 counts)
- Size breakdown (XS/S/M/L/XL counts)
- Link to the project board

## Config file format

The `.github-project-config.json` file should have this structure. Only include fields that exist on the project — all sections except `repo` are optional:

```json
{
  "repo": {
    "name": "repo-name",
    "owner": "org-or-user",
    "ownerType": "organization | user"
  },
  "project": {
    "number": 1,
    "id": "PVT_..."
  },
  "fields": {
    "status": {
      "id": "PVTSSF_...",
      "options": { "Backlog": "id", "Todo": "id", "In Progress": "id", "Done": "id" }
    },
    "priority": {
      "id": "PVTSSF_...",
      "options": { "P0": "id", "P1": "id", "P2": "id" }
    },
    "size": {
      "id": "PVTSSF_...",
      "options": { "XS": "id", "S": "id", "M": "id", "L": "id", "XL": "id" }
    }
  },
  "issueTypes": {
    "Task": "IT_...",
    "Bug": "IT_...",
    "Feature": "IT_..."
  },
  "labels": ["backend", "frontend", "bug", "enhancement"]
}
```

## Issue sizing guide

| Size   | Scope                                                 |
| ------ | ----------------------------------------------------- |
| **XS** | Simple CRUD, single config, small copy change         |
| **S**  | One endpoint or one screen, straightforward logic     |
| **M**  | Multiple endpoints or screens, moderate complexity    |
| **L**  | Complex feature spanning multiple files/systems       |
| **XL** | Large cross-cutting feature, significant architecture |

## Priority guide

| Priority | Meaning                                  |
| -------- | ---------------------------------------- |
| **P0**   | Must-have for launch, blocks other work  |
| **P1**   | Important, build after P0 items are done |
| **P2**   | Nice to have, can ship without it        |

## Type assignment guide

| Type        | When to use                                                        |
| ----------- | ------------------------------------------------------------------ |
| **Feature** | New user-facing functionality, API endpoints, UI screens           |
| **Task**    | Setup, configuration, CI/CD, documentation, testing infrastructure |
| **Bug**     | Only if fixing existing broken behavior                            |

## Important notes

- Always check for `.github-project-config.json` first — skip all discovery if it exists
- On first run, create the config file automatically after discovery so future runs are instant
- Ensure `.github-project-config.json` is gitignored (it contains project-specific IDs, not secrets, but is environment-specific)
- Always ask the user to confirm the issue plan before creating
- Never create priority labels — use project fields instead
- Check for existing labels before creating new ones
- Use heredocs for issue bodies to preserve formatting
- For GraphQL mutations, write the query to a temp file and use `-F query=@file` to avoid shell escaping issues
- Batch operations where possible for efficiency
- If the repo has no project, ask if the user wants issues only (skip project-related steps)
- If `gh` auth scopes are insufficient, tell the user which scope to add
- If the config seems stale (e.g., field IDs fail), delete it and re-run discovery
