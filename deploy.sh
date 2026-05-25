#!/bin/bash
# =====================================================
# deploy.sh — Chạy trên SERVER (157.66.46.69)
# Cách dùng:
#   1. Upload source lên server:
#      scp -r F:\ECommerceMVC root@157.66.46.69:/root/ECommerceMVC
#   2. SSH vào server, cd /root/ECommerceMVC
#   3. Chỉnh .env cho đúng production secrets
#   4. chmod +x deploy.sh && ./deploy.sh
# =====================================================

set -e

PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$PROJECT_DIR"

echo "================================================"
echo "  HShop ECommerceMVC — Deploy Script"
echo "================================================"
echo ""

# --- Check prerequisites ---
echo "[1/5] Checking prerequisites..."
for cmd in docker docker-compose; do
    if ! command -v "$cmd" &>/dev/null; then
        echo "ERROR: $cmd not found. Install it first."
        exit 1
    fi
done
echo "  OK: docker and docker-compose found"

# --- Check .env ---
echo "[2/5] Checking .env file..."
if [ ! -f .env ]; then
    echo "ERROR: .env file not found!"
    echo "  Copy .env.production.template to .env and fill in your secrets:"
    echo "  cp .env.production.template .env"
    echo "  nano .env"
    exit 1
fi

# Warn if still using default passwords
if grep -q "CHANGE_ME" .env; then
    echo "WARNING: .env still contains 'CHANGE_ME' placeholders!"
    echo "  Please edit .env and set real passwords/secrets before deploying."
    read -p "  Continue anyway? (y/N): " confirm
    if [ "$confirm" != "y" ] && [ "$confirm" != "Y" ]; then
        echo "Aborted."
        exit 1
    fi
fi
echo "  OK: .env found"

# --- Pull / Build ---
echo "[3/5] Building Docker images..."
docker compose build --no-cache
echo "  OK: images built"

# --- Stop old containers ---
echo "[4/5] Stopping old containers (if any)..."
docker compose down --remove-orphans 2>/dev/null || true
echo "  OK: old containers stopped"

# --- Start ---
echo "[5/5] Starting services..."
docker compose up -d
echo ""

# --- Health check ---
echo "Waiting for SQL Server to be healthy..."
for i in $(seq 1 60); do
    if docker compose ps sqlserver 2>/dev/null | grep -q "healthy"; then
        echo "  SQL Server is healthy!"
        break
    fi
    if [ "$i" -eq 60 ]; then
        echo "  WARNING: SQL Server not healthy after 60s. Check logs:"
        echo "  docker compose logs sqlserver"
    fi
    sleep 2
done

echo ""
echo "================================================"
echo "  Deploy complete!"
echo "================================================"
echo ""
echo "  Web app:  http://157.66.46.69:8080"
echo "  SQL Server: 157.66.46.69:1434 (sa / your SA_PASSWORD)"
echo ""
echo "  Useful commands:"
echo "    docker compose ps           — xem trạng thái containers"
echo "    docker compose logs -f web  — xem log web app"
echo "    docker compose logs -f sqlserver — xem log database"
echo "    docker compose down         — dừng tất cả"
echo "    docker compose restart web  — restart web app"
echo ""
