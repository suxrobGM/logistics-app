# Observability and Backups

What to switch on before the first pilot carrier: error tracking, database backups you have
restored at least once, a health endpoint worth alerting on, and the production checklist at the
end of this page.

## Error tracking (Sentry)

Sentry is off until a DSN is set. With no DSN nothing is initialised, nothing is sent, and the
apps behave exactly as before.

### Backend (API, Identity Server)

Both hosts start through `LogisticsHost` (`src/Presentation/Logistics.HostDefaults/`), which
calls `AddLogisticsSentry()` when `Sentry__Dsn` is set. The MCP server and Telegram bot run inside
the API process, so they are covered too. The DbMigrator does not use `LogisticsHost` and is not
wired: it runs by hand and fails loudly in the terminal.

- **What becomes an event.** Every Serilog entry at `Error` or above, through the Sentry Serilog
  sink. Lower levels (from `Information`) ride along as breadcrumbs on the next event. Unhandled
  exceptions that escape the pipeline are captured by the ASP.NET Core integration.
- **Hangfire jobs.** No extra filter is needed. A recurring job's per-tenant failure is logged at
  `Error` by `TenantJobRunner` ("Error during {Operation} for tenant {TenantName}"), and a job
  that throws is logged by Hangfire at `Error` once its retries run out. Both reach Sentry, with
  the tenant name in the event's extra data.
- **Tenant tag.** `TenantSentryEventProcessor` tags events raised during a request with
  `tenant_id`, read from `ICurrentTenantAccessor`. If no tenant can be resolved (anonymous
  endpoint, bad header) the event goes out untagged rather than being dropped. Every event also
  carries an `app` tag (`Logistics.API` / `Logistics.IdentityServer`).
- **Scrubbing** (`SentryEventScrubber`): no request bodies, cookies or query string (SignalR passes
  the access token there); `Authorization`, `Cookie`, `X-Api-Key` and `Stripe-Signature` headers
  removed; the user reduced to its opaque id (no email, username or IP); email addresses in log
  messages, log properties and breadcrumbs replaced by `[redacted]`. The Serilog request-log line
  is dropped because it duplicates the exception event and carries the response body.
- **No performance tracing.** Only errors are sent; there is no traces sample rate.

### Portals (TMS, admin, customer)

`provideErrorTracking(environment.sentry)` in each portal's `app.config.ts` installs Sentry as the
Angular `ErrorHandler` (there was no custom one), so uncaught errors and the window errors
`provideBrowserGlobalErrorListeners` forwards are reported. The SDK loads as a separate chunk, so
the initial bundle is unchanged and nothing loads at all without a DSN. The TMS portal tags
events with `tenant_id` once the user signs in. User info, cookies, headers, bodies and query
parameters are not collected (the SDK collects them by default).

The DSN is injected at container start like the Mapbox token: `app-env.prod.ts` holds
`${SENTRY_DSN}`, `${SENTRY_ENVIRONMENT}` and `${SENTRY_RELEASE}` placeholders, which
`docker-entrypoint-spa.sh` replaces. Compose maps one variable per portal, so each portal can
report to its own Sentry project. The driver app is not wired; it reports through Crashlytics.

### Setup

1. Create Sentry projects: one ASP.NET Core project for the backend, one Angular project per
   portal (or one shared Angular project, using the same DSN three times).
2. Add the DSNs to `.env` (the `DOCKER_ENV` secret): the backend DSN plus one per portal, as
   listed in the Sentry block of [`deploy/.env.example`](../../deploy/.env.example).
3. Deploy. The deploy workflow appends `SENTRY_RELEASE=<commit sha>` to `.env`, so backend and
   portal events from one deploy share a release.
4. Verify: raise a test error (for example a request that makes the API log an error) and check
   it arrives with `tenant_id` and `app` tags and without an email anywhere in the event.

Add an alert rule in Sentry (new issue, or a spike) that notifies the on-call channel.

## Health checks

`GET /health` on the API returns `Healthy` (200) or `Unhealthy` (503). It checks:

| Check       | Unhealthy when                                                                        |
| ----------- | ------------------------------------------------------------------------------------- |
| `master-db` | The master database is unreachable                                                    |
| `hangfire`  | Hangfire storage is unreachable, or no Hangfire server has sent a heartbeat for 5 min |

A dead Hangfire server is the failure that otherwise goes unnoticed: the API keeps serving
requests while ELD, load board, accounting and payroll syncs silently stop. The deploy workflow
already polls `/health` after each deploy; point an uptime monitor at
`https://api.yourdomain.com/health` too. The Identity Server's `/health` checks its master DB.

## Database backups

PostgreSQL runs outside the app stack, so backups run in their own compose project:
`deploy/docker-compose.backup.yml` runs `deploy/db-backup/pg-backup.sh` nightly (03:30 UTC by
default) in the `postgres:18` client image, with host networking since Postgres listens on the
host's `localhost`. The script:

- discovers databases on the server on every run (`datname LIKE '%_logisticsx'`), which covers
  `master_logisticsx` and every tenant database (`TenantDatabaseDefaults:NameTemplate` is
  `{tenant}_logisticsx`). A tenant created today is in tonight's backup.
- writes `pg_dump -Fc` per database plus `globals.sql` (roles, without passwords) into
  `daily/YYYY-MM-DD/` under `BACKUP_HOST_DIR`, checking each dump with `pg_restore --list` before
  keeping it, and keeps the newest 7 runs locally.
- optionally copies the run to `postgres/YYYY-MM-DD/` in S3-compatible storage. Production
  documents already live on Cloudflare R2 (`BlobStorage__Type=Cloudflare`), so use R2 here too,
  but a **separate bucket** with its own API token. A leaked documents token then cannot delete
  the backups.
- exits non-zero if any database, the upload, or the globals dump failed, and pings
  `BACKUP_HEARTBEAT_URL` only after a fully successful run. Point that at a dead-man's-switch
  monitor (healthchecks.io, Better Stack) so a run that never happens raises an alert.

Set the `BACKUP_*` variables in `deploy/.env` (the backups block of
[`deploy/.env.example`](../../deploy/.env.example) lists them), then:

```bash
docker compose -f deploy/docker-compose.backup.yml up -d
docker compose -f deploy/docker-compose.backup.yml logs -f      # see each night's run
docker compose -f deploy/docker-compose.backup.yml exec db-backup sh -c '. /etc/backup.env; sh /scripts/pg-backup.sh'   # run now
```

The app deploy's `--remove-orphans` leaves this project alone. The deploy workflow copies only
`docker-compose.yml`; copy `docker-compose.backup.yml` and the `db-backup/` folder to the VPS once.

### Off-site retention

The script uploads but never deletes remote copies. Add one lifecycle rule on the backup bucket
(R2: bucket settings, Object lifecycle rules) that deletes objects under `postgres/` after 30
days. Older history comes from that rule alone: there is no weekly tier, locally or remotely,
because 30 off-site dailies already cover what four weekly copies did.

### Restore

`deploy/db-backup/pg-restore.sh <file.dump> <target-database>` creates the target if needed and
restores in a single transaction (`--no-owner --no-acl`, so the target server's roles do not have
to match). Run it in the backup container, which has the client tools and the connection:

```bash
docker compose -f deploy/docker-compose.backup.yml exec db-backup \
  sh /scripts/pg-restore.sh /backups/daily/<day>/acme_logisticsx.dump acme_logisticsx_restore
```

**One tenant** (for example a tenant deleted data by mistake):

1. Find the dump: `/backups/daily/<day>/` in the container (`BACKUP_HOST_DIR` on the host), or
   fetch an older one from the bucket into the container:
   `docker compose -f deploy/docker-compose.backup.yml exec db-backup sh -c 'aws s3 cp s3://logisticsx-backups/postgres/<day>/acme_logisticsx.dump /backups/ --endpoint-url "$BACKUP_S3_ENDPOINT"'`.
2. Restore it next to the live database as `acme_logisticsx_restore` and inspect it.
3. Either copy the rows you need across, or swap the databases: stop the API
   (`docker compose stop api`), then
   `ALTER DATABASE acme_logisticsx RENAME TO acme_logisticsx_old;`
   `ALTER DATABASE acme_logisticsx_restore RENAME TO acme_logisticsx;`, then start the API.
   Tenant connection strings name the database, so the renamed copy is picked up as is.

**Whole server** (new Postgres host): restore `globals.sql` with `psql -f globals.sql` and set the
role passwords again, restore `master_logisticsx` first, then every tenant dump under its original
name. If the host name changed, update `ConnectionStrings__MasterDatabase` and the tenant rows'
connection strings in the master `tenants` table.

### Restore drill

Do this before the first pilot and then monthly. A backup that has never been restored is not a
backup.

1. Pick yesterday's off-site run (not the local copy) and download `master_logisticsx.dump` and
   one tenant's dump.
2. Restore both under `_drill` names on the production server or a scratch Postgres of the same
   major version.
3. Spot-check: `SELECT count(*) FROM tenants;` on the master copy, and in the tenant copy the
   load count and the most recent `created_at` (should be from the day of the dump).
4. Drop the `_drill` databases and note the date and the time the restore took.

## Pre-pilot production checklist

- [ ] **Email.** Resend sending domain verified (SPF, DKIM, DMARC), `Resend__SenderEmail` on that
      domain, `Resend__WebhookSecret` set. Send an invitation to an outside inbox and check it is
      not in spam.
- [ ] **Sentry.** `Sentry__Dsn` and the three portal DSNs set, a test error received from the API
      and from the TMS portal with the `tenant_id` tag, and an alert rule routed to a person.
- [ ] **Backups.** Nightly backup running with off-site copy, heartbeat monitor configured,
      lifecycle rule on the bucket, and the [restore drill](#restore-drill) done once with the
      result written down.
- [ ] **Health.** Uptime monitor on `https://api.yourdomain.com/health`.
- [ ] **Migrations.** `deploy/run-prod-migrator.ps1` run, so the `AddPilotCarrierFeatures` and
      `RepairConversationLastSequence` tenant migrations apply and role claims re-sync. Without it the
      trailer and data import permissions are missing and those pages return 403.
- [ ] **QuickBooks.** Production Intuit app keys and redirect URI, per
      [QuickBooks setup](../accounting-quickbooks-setup.md), and one end-to-end run against a sandbox
      company (still open per [the roadmap](../roadmap/quickbooks-sync.md)).
- [ ] **Samsara.** Production API token and webhook secret for each pilot tenant, per
      [Samsara setup](../eld-samsara-setup.md).
- [ ] **DAT.** Production DAT credentials, per [DAT setup](../loadboard-dat-setup.md), and the
      **Assumed** truck-posting rows in that guide checked against the live API.
- [ ] **AI.** An API key for the provider of the selected dispatch model (default `gpt-6-luna`, so
      `Llm__Providers__OpenAi__ApiKey`) and for `Llm__PolicyLearningModel` (default `deepseek-flash`,
      so the DeepSeek key).
- [ ] **IFTA rates.** Official rates for the current quarter entered in Admin portal > IFTA rates;
      `SeedData/ifta-tax-rates.json` holds approximations.
- [ ] **Tenant presets.** Each pilot tenant has its presets set (car hauler turns on vehicle
      transport, intermodal turns on containers).
- [ ] **Mapbox and notifications.** `Mapbox__AccessToken` and `NotificationEmail` set.
- [ ] **Firebase.** `FIREBASE_CREDENTIALS_JSON` secret holds the production Admin SDK key (push
      notifications to the driver app), and the driver app build uses the production Firebase
      project (Crashlytics).
- [ ] **Stripe.** Live keys: `Stripe__SecretKey` is `sk_live_...` and `Stripe__WebhookSecret`
      belongs to the live-mode endpoint (see [Stripe webhooks](../stripe-webhooks.md)).
- [ ] **Secrets.** `Impersonation__MasterPassword`, `TenantDatabaseDefaults__Password` and the
      Postgres password are real secrets, not the `.env.example` placeholders.
