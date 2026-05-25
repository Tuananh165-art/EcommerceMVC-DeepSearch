#!/bin/bash
# ===========================================================
# Custom SQL Server entrypoint — runs init scripts against
# Hshop2023 DB, then hands off to sqlservr.
# ===========================================================
set -e

SA_PASSWORD="${MSSQL_SA_PASSWORD:-${SA_PASSWORD:-YourStrong@Passw0rd}}"

# Find sqlcmd (mssql-tools18 or legacy mssql-tools)
SQLCMD=""
for p in /opt/mssql-tools18/bin/sqlcmd /opt/mssql-tools/bin/sqlcmd; do
    if [ -x "$p" ]; then SQLCMD="$p"; break; fi
done
if [ -z "$SQLCMD" ]; then
    echo "ERROR: sqlcmd not found!"
    exit 1
fi

# Wait for SQL Server to be ready
echo "=== Waiting for SQL Server ==="
for i in $(seq 1 60); do
    $SQLCMD -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" &>/dev/null && break
    sleep 1
done
echo "=== SQL Server ready ==="

# Create database if needed
$SQLCMD -S localhost -U sa -P "$SA_PASSWORD" -C -Q "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'Hshop2023') CREATE DATABASE Hshop2023;"

# Run all init scripts against Hshop2023
for f in /init-scripts/*.sql; do
    [ -f "$f" ] || continue
    echo ">>> Running $(basename "$f") against Hshop2023..."
    $SQLCMD -S localhost -U sa -P "$SA_PASSWORD" -C -d Hshop2023 -i "$f" 2>&1 | tail -5 || echo "  WARNING: $(basename "$f") had errors"
done

echo "=== DB init complete ==="
