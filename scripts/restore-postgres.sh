#!/usr/bin/env sh
set -eu

if [ "${1:-}" = "" ]; then
  echo "Usage: scripts/restore-postgres.sh path/to/backup.dump" >&2
  exit 1
fi

POSTGRES_DB="${POSTGRES_DB:-lova_salud}"
POSTGRES_USER="${POSTGRES_USER:-postgres}"

docker compose exec -T postgres pg_restore -U "$POSTGRES_USER" -d "$POSTGRES_DB" --clean --if-exists < "$1"
echo "Restored $1 into $POSTGRES_DB"
