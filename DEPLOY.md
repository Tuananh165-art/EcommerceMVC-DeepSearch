# HShop ECommerceMVC — Hướng dẫn Deploy Docker lên Server

## Port Mapping (đã tránh port đang dùng trên server)

| Service      | Host Port | Container Port | Ghi chú                          |
|-------------|-----------|----------------|----------------------------------|
| SQL Server  | 1434      | 1433           | Server đã có 1433 (instance cũ) |
| Web App     | 8080      | 8080           | Server đã có 80, 443, 8081      |

**Port trên server đang dùng (TRÁNH):**
- `80, 443` — nginx
- `22` — sshd
- `1433` — SQL Server cũ
- `3001` — Next.js
- `5433` — PostgreSQL
- `8081` — Node.js

---

## Cách 1: Deploy tự động bằng script (khuyên dùng)

### Bước 1: Upload source code lên server

Từ máy local (Windows, Git Bash):

```bash
# Upload toàn bộ project lên server
scp -r /f/ECommerceMVC root@157.66.46.69:/root/ECommerceMVC
```

Hoặc dùng WinSCP / MobaXterm file transfer.

### Bước 2: SSH vào server

```bash
ssh root@157.66.46.69
cd /root/ECommerceMVC
```

### Bước 3: Tạo file .env với secrets thật

```bash
# Copy template
cp .env.production.template .env

# Chỉnh sửa — ĐẶC BIỆT thay đổi password và API keys
nano .env
```

**Bắt buộc thay đổi:**
- `SA_PASSWORD` — password mạnh cho SQL Server
- `DB_CONNECTION_STRING` — phải khớp SA_PASSWORD ở trên
- `EMAIL_SMTP_USER`, `EMAIL_SMTP_PASSWORD` — SMTP thật
- `VNPAY_TMNCODE`, `VNPAY_HASH_SECRET` — VNPay sandbox keys
- `MOMO_PARTNER_CODE`, `MOMO_ACCESS_KEY`, `MOMO_SECRET_KEY` — MoMo keys
- `ADMIN_SECRET_CODE` — secret code cho admin panel

### Bước 4: Chạy deploy script

```bash
chmod +x deploy.sh
./deploy.sh
```

Script sẽ tự động:
1. Kiểm tra Docker/docker-compose
2. Validate .env
3. Build images
4. Stop containers cũ (nếu có)
5. Start services mới
6. Wait cho SQL Server healthy

### Bước 5: Kiểm tra

```bash
# Xem trạng thái containers
docker compose ps

# Xem log web app
docker compose logs -f web

# Test web app
curl -I http://localhost:8080

# Từ máy local, mở trình duyệt:
# http://157.66.46.69:8080
```

---

## Cách 2: Deploy thủ công (từng bước)

### Bước 1: Cài Docker trên server (nếu chưa có)

```bash
# Ubuntu/Debian
curl -fsSL https://get.docker.com | sh
systemctl enable docker
systemctl start docker

# Cài docker compose plugin
apt install -y docker-compose-plugin
```

### Bước 2: Upload source code

```bash
# Từ local
scp -r /f/ECommerceMVC root@157.66.46.69:/root/ECommerceMVC
```

### Bước 3: SSH và chuẩn bị .env

```bash
ssh root@157.66.46.69
cd /root/ECommerceMVC
cp .env.production.template .env
nano .env  # Điền secrets thật
```

### Bước 4: Build và chạy

```bash
# Build images
docker compose build --no-cache

# Dọn containers cũ
docker compose down --remove-orphans

# Start
docker compose up -d

# Xem logs
docker compose logs -f
```

---

## Cập nhật code (redeploy)

Khi có code mới, chỉ cần:

```bash
ssh root@157.66.46.69
cd /root/ECommerceMVC

# Pull code mới (nếu dùng git)
git pull

# Hoặc upload lại từ local
# scp -r /f/ECommerceMVC/* root@157.66.46.69:/root/ECommerceMVC/

# Build lại và restart
docker compose build --no-cache
docker compose up -d --force-recreate

# Hoặc dùng script
./deploy.sh
```

---

## Quản lý containers

```bash
# Xem trạng thái
docker compose ps

# Xem logs realtime
docker compose logs -f web
docker compose logs -f sqlserver

# Restart 1 service
docker compose restart web

# Dừng tất cả
docker compose down

# Dừng + xóa volume (mất data!)
docker compose down -v

# Xem disk usage
docker system df

# Dọn image cũ
docker system prune -f
```

---

## Backup / Restore Database

### Backup

```bash
# Backup database ra file .bak
docker exec hshop-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' -C \
  -Q "BACKUP DATABASE Hshop2023 TO DISK = '/var/opt/mssql/Hshop2023.bak'"

# Copy ra host
docker cp hshop-sqlserver:/var/opt/mssql/Hshop2023.bak ./backup.bak
```

### Restore

```bash
# Copy file .bak vào container
docker cp ./backup.bak hshop-sqlserver:/var/opt/mssql/

# Restore
docker exec hshop-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Passw0rd' -C \
  -Q "RESTORE DATABASE Hshop2023 FROM DISK = '/var/opt/mssql/backup.bak' WITH REPLACE"
```

---

## Troubleshooting

### SQL Server không start được

```bash
docker compose logs sqlserver
# Kiểm tra SA_PASSWORD đủ mạnh (ít nhất 8 ký tự, có upper, lower, digit, symbol)
```

### Web app không kết nối được database

```bash
# Kiểm tra connection string trong .env
# Server phải là "sqlserver" (tên service), KHÔNG phải "localhost"
grep DB_CONNECTION_STRING .env
```

### Port 8080 bị chiếm

```bash
# Kiểm tra port nào đang dùng
ss -tlnp | grep 8080

# Nếu bị chiếm, đổi port trong docker-compose.yml
# Ví dụ: "9090:8080"
```

### Container bị OOM (out of memory)

```bash
# SQL Server cần ít nhất 2GB RAM
# Kiểm tra memory usage
docker stats

# Nếu server yếu, giảm memory limit trong docker-compose.yml
```

### Xem database từ bên ngoài

```bash
# Kết nối từ local qua port 1434
# SQL Server Management Studio (SSMS) hoặc Azure Data Studio:
# Server: 157.66.46.69,1434
# User: sa
# Password: (SA_PASSWORD của bạn)
```

---

## Cấu trúc file quan trọng

```
ECommerceMVC/
├── docker-compose.yml          # Docker Compose config
├── Dockerfile                  # .NET 8 build + runtime
├── .env                        # Environment variables (KHÔNG commit!)
├── .env.example                # Template mẫu
├── .env.production.template    # Template cho production
├── .dockerignore               # Files bỏ qua khi build
├── deploy.sh                   # Script deploy tự động
└── DEPLOY.md                   # File hướng dẫn này
```
