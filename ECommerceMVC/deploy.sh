#!/bin/bash
# ===========================================================
# Deploy script for ECommerceMVC — auto-detects Compose V1/V2
# ===========================================================
set -e

cd "$(dirname "$0")"

echo "=== ECommerceMVC Deploy ==="

# Detect docker compose command
COMPOSE_CMD=""
if command -v docker &>/dev/null && docker compose version &>/dev/null 2>&1; then
    COMPOSE_CMD="docker compose"
    echo "Using Docker Compose V2 (plugin)"
elif command -v docker-compose &>/dev/null; then
    COMPOSE_CMD="docker-compose"
    echo "Using Docker Compose V1 (standalone)"
else
    echo "ERROR: Docker Compose not found"
    exit 1
fi

# Check .env exists
if [ ! -f .env ]; then
    echo "ERROR: .env file not found!"
    echo "Copy .env.production.example to .env and fill in secrets."
    exit 1
fi

# Build
echo ""
echo ">>> Building images..."
$COMPOSE_CMD build --no-cache

# Stop old containers
echo ""
echo ">>> Stopping old containers..."
$COMPOSE_CMD down --remove-orphans 2>/dev/null || true

# Start
echo ""
echo ">>> Starting services..."
$COMPOSE_CMD up -d

# Wait for web app
echo ""
echo ">>> Waiting for services to be healthy..."
sleep 10

# Status
echo ""
echo "=== Service status ==="
$COMPOSE_CMD ps

echo ""
echo "=== Web app logs (last 20 lines) ==="
$COMPOSE_CMD logs --tail=20 web

echo ""
echo "=== Deploy complete ==="
echo "Web app: http://localhost:${WEB_HOST_PORT:-8080}"
echo "SQL Server: localhost:${SQL_HOST_PORT:-1434}"
echo ""
echo "View logs: $COMPOSE_CMD logs -f"
echo "Stop:      $COMPOSE_CMD down"
