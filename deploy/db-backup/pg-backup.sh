#!/bin/sh
# Dumps the master database and every tenant database (pg_dump -Fc), keeps the newest daily runs
# locally, and optionally copies each run to S3-compatible storage (Cloudflare R2).
# Tenant databases are discovered from the server on each run, so a tenant provisioned
# yesterday is in tonight's backup without any config change.
#
# Runs inside deploy/docker-compose.backup.yml, which sets the PG* connection variables and the
# BACKUP_* settings; deploy/.env.example lists them. Restore: deploy/db-backup/pg-restore.sh and
# docs/deployment/observability-and-backups.md.
set -eu

backup_dir=/backups
keep_daily="${BACKUP_KEEP_DAILY:-7}"
# Matches master_logisticsx and every tenant database named by the {tenant}_logisticsx template.
db_pattern='%_logisticsx'

log() { echo "$(date -u '+%Y-%m-%dT%H:%M:%SZ') $*"; }

day="$(date -u '+%Y-%m-%d')"
run_dir="$backup_dir/daily/$day"
mkdir -p "$run_dir"

databases="$(psql -d postgres -At -c \
  "SELECT datname FROM pg_database WHERE NOT datistemplate AND datname LIKE '$db_pattern' ORDER BY datname")"

if [ -z "$databases" ]; then
  log "no databases match '$db_pattern' on ${PGHOST:-localhost}"
  exit 1
fi

failed=0

# Roles and their grants live outside every database dump; restoring onto a new server needs them.
if pg_dumpall --globals-only --no-role-passwords > "$run_dir/globals.sql.tmp"; then
  mv "$run_dir/globals.sql.tmp" "$run_dir/globals.sql"
else
  log "FAILED globals"
  failed=1
fi

for db in $databases; do
  # Written under a temp name and checked with pg_restore --list, so a half-written or corrupt
  # dump never replaces a good one.
  if pg_dump -Fc -d "$db" -f "$run_dir/$db.dump.tmp" && pg_restore --list "$run_dir/$db.dump.tmp" > /dev/null; then
    mv "$run_dir/$db.dump.tmp" "$run_dir/$db.dump"
    log "dumped $db ($(du -h "$run_dir/$db.dump" | cut -f1))"
  else
    rm -f "$run_dir/$db.dump.tmp"
    log "FAILED $db"
    failed=1
  fi
done

# Run folders are named by date, so the newest sort first.
ls -1 "$backup_dir/daily" | sort -r | tail -n +"$((keep_daily + 1))" | while read -r old; do
  log "pruning $backup_dir/daily/$old"
  rm -rf "${backup_dir:?}/daily/$old"
done

if [ -n "${BACKUP_S3_BUCKET:-}" ]; then
  endpoint=""
  [ -n "${BACKUP_S3_ENDPOINT:-}" ] && endpoint="--endpoint-url $BACKUP_S3_ENDPOINT"
  remote="s3://$BACKUP_S3_BUCKET/postgres/$day/"
  # Remote retention is a bucket lifecycle rule, not a delete here.
  # shellcheck disable=SC2086
  if aws s3 cp --recursive --only-show-errors $endpoint "$run_dir" "$remote"; then
    log "uploaded to $remote"
  else
    log "FAILED upload to $remote"
    failed=1
  fi
fi

if [ "$failed" -ne 0 ]; then
  log "backup finished with failures"
  exit 1
fi

if [ -n "${BACKUP_HEARTBEAT_URL:-}" ]; then
  wget -q -O /dev/null "$BACKUP_HEARTBEAT_URL" || log "heartbeat ping failed"
fi

log "backup finished: $run_dir"
