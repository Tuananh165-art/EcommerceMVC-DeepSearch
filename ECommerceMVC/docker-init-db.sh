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

# Flags: -C trust cert, -f 65001 = UTF-8 input
SCMD="$SQLCMD -S localhost -U sa -P $SA_PASSWORD -C -f 65001"

echo "=== Custom SQL Server Entrypoint ==="
echo "Starting sqlservr in background..."

# Start SQL Server in background
/opt/mssql/bin/sqlservr &
SQLSERVR_PID=$!

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to accept connections..."
for i in $(seq 1 90); do
    if [ -n "$SQLCMD" ] && $SQLCMD -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" &>/dev/null; then
        echo "SQL Server is ready (took ${i}s)"
        break
    fi
    sleep 1
done

# Run init scripts if they exist
if ls /init-scripts/[0-9]*.sql 1>/dev/null 2>&1; then
    echo "=== Running initialization scripts ==="

    # Run ALL numbered scripts in order against the correct database
    for f in $(ls /init-scripts/[0-9]*.sql | sort); do
        FNAME=$(basename "$f")
        echo ">>> Running $FNAME..."

        # HShopScript.sql must run against master (it creates the DB)
        if echo "$FNAME" | grep -qi "HShopScript\|01-"; then
            echo "    (running against master - creates Hshop2023 database)"
            $SQLCMD -S localhost -U sa -P "$SA_PASSWORD" -C -i "$f" 2>&1 | tail -5 || echo "  WARNING: $FNAME had errors"
        else
            # All other scripts run against Hshop2023
            $SQLCMD -S localhost -U sa -P "$SA_PASSWORD" -C -d Hshop2023 -i "$f" 2>&1 | tail -5 || echo "  WARNING: $FNAME had errors"
        fi
    done

    echo "=== Initialization complete ==="
else
    echo "No init scripts found in /init-scripts/"
fi

# Verify database was created
echo "=== Verifying Hshop2023 database ==="
$SQLCMD -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT name FROM sys.databases WHERE name = 'Hshop2023'"

# Wait for sqlservr to keep the container alive
echo "Keeping sqlservr running (PID $SQLSERVR_PID)..."
wait $SQLSERVR_PID
