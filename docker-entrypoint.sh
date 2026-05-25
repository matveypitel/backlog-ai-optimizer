#!/bin/sh
set -e

# ─── Database migrations ──────────────────────────────────────────────────────
# efbundle is a self-contained EF Core migration runner compiled during image build.
# It reads the connection string from ConnectionStrings__DefaultConnection (env var).

if [ -f "/app/efbundle" ]; then
    echo "[entrypoint] Running EF Core migrations..."
    /app/efbundle --connection "${ConnectionStrings__DefaultConnection}"
    echo "[entrypoint] Migrations complete."
else
    echo "[entrypoint] WARNING: migration bundle not found, skipping migrations."
fi

# ─── Start the API ────────────────────────────────────────────────────────────
echo "[entrypoint] Starting BacklogOptimizer API..."
exec dotnet BacklogOptimizer.Api.dll "$@"
