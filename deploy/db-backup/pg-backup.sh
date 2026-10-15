#!/bin/sh
# Dumps the master database and every tenant database (pg_dump -Fc), keeps 7 daily and
# 4 weekly copies locally, and optionally copies the run to S3-compatible storage (Cloudflare R2).
# Tenant databases are discovered from the server on each run, so a tenant provisioned
# yesterday is in tonight's backup without any config change.
#
# Runs from host cron or from deploy/docker-compose.backup.yml. Restore: deploy/db-backup/pg-restore.sh
# and docs/deployment/observability-and-backups.md.
#
# Connection: the standard libpq variables PGHOST, PGPORT, PGUSER, PGPASSWORD (or ~/.pgpass).
#
#   BACKUP_DIR            local root (default /var/backups/logisticsx)
#   BACKUP_DB_PATTERN     SQL LIKE pattern of databases to dump (default %_logisticsx, which
#                         matches master_logisticsx and the {tenant}_logisticsx template)
#   BACKUP_KEEP_DAILY     daily runs kept locally (default 7)
#   BACKUP_KEEP_WEEKLY    Sunday runs kept locally (default 4)
#   BACKUP_S3_BUCKET      set to also upload; needs the aws CLI and AWS_ACCESS_KEY_ID /
#                         AWS_SECRET_ACCESS_KEY
#   BACKUP_S3_ENDPOINT    e.g. https://<account-id>.r2.cloudflarestorage.com (omit for AWS S3)
#   BACKUP_S3_PREFIX      key prefix in the bucket (default postgres)
#   BACKUP_HEARTBEAT_URL  pinged after a fully successful run (healthchecks.io, Better Stack, ...)
set -eu

BACKUP_DIR="${BACKUP_DIR:-/var/backups/logisticsx}"
BACKUP_DB_PATTERN="${BACKUP_DB_PATTERN:-%_logisticsx}"
BACKUP_KEEP_DAILY="${BACKUP_KEEP_DAILY:-7}"
BACKUP_KEEP_WEEKLY="${BACKUP_KEEP_WEEKLY:-4}"
BACKUP_S3_PREFIX="${BACKUP_S3_PREFIX:-postgres}"

log() { echo "$(date -u '+%Y-%m-%dT%H:%M:%SZ') $*"; }

# Deletes all but the newest $2 run folders under $1 (folder names sort by date).
prune() {
  [ -d "$1" ] || return 0
  ls -1 "$1" | sort -r | tail -n +"$(($2 + 1))" | while read -r old; do
    log "pruning $1/$old"
    rm -rf "${1:?}/$old"
  done
}

day="$(date -u '+%Y-%m-%d')"
run_dir="$BACKUP_DIR/daily/$day"
mkdir -p "$run_dir"

databases="$(psql -d postgres -At -c \
  "SELECT datname FROM pg_database WHERE NOT datistemplate AND datname LIKE '$BACKUP_DB_PATTERN' ORDER BY datname")"

if [ -z "$databases" ]; then
  log "no databases match '$BACKUP_DB_PATTERN' on ${PGHOST:-localhost}"
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

if [ "$(date -u '+%u')" = "7" ]; then
  mkdir -p "$BACKUP_DIR/weekly"
  rm -rf "$BACKUP_DIR/weekly/$day"
  cp -R "$run_dir" "$BACKUP_DIR/weekly/$day"
fi

prune "$BACKUP_DIR/daily" "$BACKUP_KEEP_DAILY"
prune "$BACKUP_DIR/weekly" "$BACKUP_KEEP_WEEKLY"

if [ -n "${BACKUP_S3_BUCKET:-}" ]; then
  endpoint=""
  [ -n "${BACKUP_S3_ENDPOINT:-}" ] && endpoint="--endpoint-url $BACKUP_S3_ENDPOINT"
  # Remote retention is a bucket lifecycle rule on these prefixes, not a delete here.
  # shellcheck disable=SC2086
  if aws s3 cp --recursive --only-show-errors $endpoint "$run_dir" "s3://$BACKUP_S3_BUCKET/$BACKUP_S3_PREFIX/daily/$day/"; then
    log "uploaded to s3://$BACKUP_S3_BUCKET/$BACKUP_S3_PREFIX/daily/$day/"
    if [ "$(date -u '+%u')" = "7" ]; then
      # shellcheck disable=SC2086
      aws s3 cp --recursive --only-show-errors $endpoint "$run_dir" "s3://$BACKUP_S3_BUCKET/$BACKUP_S3_PREFIX/weekly/$day/" || failed=1
    fi
  else
    log "FAILED upload to s3://$BACKUP_S3_BUCKET"
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
