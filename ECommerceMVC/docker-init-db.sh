#!/bin/bash
# ===========================================================
# Custom entrypoint for SQL Server container
# Starts sqlservr, waits, runs init scripts, then keeps running
# ===========================================================
set -e

SA_PASSWORD="${MSSQL_SA_PASSWORD:-${SA_PASSWORD:-YourStrong@Passw0rd}}"

# Find sqlcmd
SQLCMD=""
for p in /opt/mssql-tools18/bin/sqlcmd /opt/mssql-tools/bin/sqlcmd; do
    if [ -x "$p" ]; then SQLCMD="$p"; break; fi
done

# Common sqlcmd flags: -C = trust cert, -f 65001 = UTF-8 encoding
SQLCMD_FLAGS="-S localhost -U sa -P \"$SA_PASSWORD\" -C -f 65001"

echo "=== Custom SQL Server Entrypoint ==="
echo "Starting sqlservr in background..."

# Start SQL Server in background
/opt/mssql/bin/sqlservr &
SQLSERVR_PID=$!

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to accept connections..."
for i in $(seq 1 90); do
    if [ -n "$SQLCMD" ] && $SQLCMD -Q "SELECT 1" &>/dev/null; then
        echo "SQL Server is ready (took ${i}s)"
        break
    fi
    sleep 1
done

# Run init scripts if they exist
if ls /init-scripts/*.sql 1>/dev/null 2>&1; then
    echo "=== Running initialization scripts ==="

    # HShopScript.sql creates the database, run it first against master
    if [ -f /init-scripts/01-HShopScript.sql ]; then
        echo ">>> Running HShopScript.sql (creates Hshop2023 database + schema)..."
        eval "$SQLCMD -i /init-scripts/01-HShopScript.sql" 2>&1 | tail -3 || echo "  WARNING: HShopScript.sql had errors"
    fi

    # Remaining scripts run against Hshop2023
    for f in /init-scripts/02-*.sql /init-scripts/03-*.sql /init-scripts/04-*.sql /init-scripts/05-*.sql /init-scripts/06-*.sql /init-scripts/07-*.sql /init-scripts/08-*.sql /init-scripts/09-*.sql /init-scripts/10-*.sql; do
        [ -f "$f" ] || continue
        echo ">>> Running $(basename "$f") against Hshop2023..."
        eval "$SQLCMD -d Hshop2023 -i \"$f\"" 2>&1 | tail -3 || echo "  WARNING: $(basename "$f") had errors"
    done

    echo "=== Initialization complete ==="
else
    echo "No init scripts found in /init-scripts/"
fi

# Wait for sqlservr to keep the container alive
echo "Keeping sqlservr running (PID $SQLSERVR_PID)..."
wait $SQLSERVR_PID
