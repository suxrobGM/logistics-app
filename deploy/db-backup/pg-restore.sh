#!/bin/sh
# Restores one pg_dump -Fc file into a database, creating it if needed.
#
#   docker compose -f deploy/docker-compose.backup.yml exec db-backup \
#     sh /scripts/pg-restore.sh /backups/daily/<day>/<db>.dump <target-database>
#
# The backup container has the client tools and the PG* connection variables. Restore into a NEW
# name first (e.g. acme_logisticsx_restore) for a drill, or to inspect data before swapping it in.
# Restoring over a live database: stop the api container first, then drop and recreate the target.
set -eu

if [ "$#" -ne 2 ]; then
  echo "usage: $0 <file.dump> <target-database>" >&2
  exit 2
fi

dump="$1"
target="$2"

if [ -z "$(psql -d postgres -At -c "SELECT 1 FROM pg_database WHERE datname = '$target'")" ]; then
  createdb "$target"
fi

# --no-owner/--no-acl: the target server's roles may differ; the app connects as one user anyway.
pg_restore --no-owner --no-acl --exit-on-error --single-transaction -d "$target" "$dump"
echo "restored $dump into $target"
