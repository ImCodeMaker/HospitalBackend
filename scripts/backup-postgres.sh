#!/usr/bin/env sh
set -eu

BACKUP_DIR="${BACKUP_DIR:-./backups}"
POSTGRES_DB="${POSTGRES_DB:-lova_salud}"
POSTGRES_USER="${POSTGRES_USER:-postgres}"
STAMP="$(date +%Y%m%d-%H%M%S)"

mkdir -p "$BACKUP_DIR"
docker compose exec -T postgres pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" --format=custom > "$BACKUP_DIR/${POSTGRES_DB}-${STAMP}.dump"
echo "Backup written to $BACKUP_DIR/${POSTGRES_DB}-${STAMP}.dump"
