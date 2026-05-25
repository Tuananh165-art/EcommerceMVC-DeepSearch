# Deploy ECommerceMVC Len Production Server (157.66.46.69)

## Tong quan
- **Web app**: http://157.66.46.69:8080
- **SQL Server**: 157.66.46.69:1434
- **Ports da tranh**: 80, 443 (nginx), 22 (sshd), 1433 (SQL Server cu), 3001 (next), 8081 (node), 5000 (dotnet localhost)

---

## Buoc 1: Chuan bi tren Windows (local)

### 1.1 Kiem tra cac file da co san
Trong `F:\ECommerceMVC\ECommerceMVC\` phai co:
- `Dockerfile` — multi-stage build cho ASP.NET 8
- `docker-compose.yml` — SQL Server + Web app
- `docker-init-db.sh` — script khoi tao database
- `.dockerignore` — loai bo file khong can thiet
- `.env.production.example` — mau bien moi truong
- `HShopScript.sql` + cac file `*.sql` khac — schema va du lieu

### 1.2 Chuan bi file .env tren server
Copy `.env.production.example` thanh `.env` va dien secret that:

```bash
# Tren local, tao file .env de deploy:
cd F:\ECommerceMVC\ECommerceMVC
copy .env.production.example .env
# CHINH SUA .env voi SMTP password that, VNPay secret that, etc.
```

---

## Buoc 2: Upload len Server

### 2.1 SSH vao server
```bash
ssh root@157.66.46.69
```

### 2.2 Tao thu muc tren server
```bash
mkdir -p /opt/ecommerce
cd /opt/ecommerce
```

### 2.3 Upload tu Windows (dung Git Bash / MobaXterm scp)
```bash
# Tu Git Bash tren Windows:
scp -r /f/ECommerceMVC/ECommerceMVC/* root@157.66.46.69:/opt/ecommerce/
# Hoac scp file .env rieng (vi no da bi .dockerignore loai):
scp /f/ECommerceMVC/ECommerceMVC/.env root@157.66.46.69:/opt/ecommerce/
```

### 2.4 Kiem tra tren server
```bash
ssh root@157.66.46.69
cd /opt/ecommerce
ls -la Dockerfile docker-compose.yml docker-init-db.sh .env HShopScript.sql
```

---

## Buoc 3: Build va Deploy

### 3.1 Kiem tra Docker Compose V2
```bash
docker compose version
# Neu bao loi "docker-compose: command not found" -> dung "docker compose" (V2)
# Neu can cai: apt install docker-compose-plugin
```

### 3.2 Build va chay (lan dau)
```bash
cd /opt/ecommerce
docker compose up -d --build
```

Lenh nay se:
1. Build ASP.NET image tu Dockerfile (phai mat ~2-5 phut)
2. Pull SQL Server 2022 image
3. Khoi dong SQL Server container
4. Chay docker-init-db.sh de tao DB Hshop2023 + chay 10 SQL scripts
5. Khoi dong Web app container (doi SQL Server healthy)

### 3.3 Theo doi logs
```bash
# Xem tat ca logs:
docker compose logs -f

# Chi xem web app:
docker compose logs -f web

# Chi xem SQL Server:
docker compose logs -f sqlserver
```

### 3.4 Kiem tra trang thai
```bash
docker compose ps
# Ket qua mong doi:
# NAME                 STATUS
# ecommerce-sqlserver  Up (healthy)
# ecommerce-web        Up
```

---

## Buoc 4: Verify

### 4.1 Kiem tra Web app
```bash
curl -I http://localhost:8080
# Mong doi: HTTP/1.1 200 OK

# Hoac tu browser: http://157.66.46.69:8080
```

### 4.2 Kiem tra SQL Server
```bash
docker exec ecommerce-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd" -C \
  -Q "SELECT name FROM sys.databases WHERE name = 'Hshop2023'"

# Kiem tra so luong san pham:
docker exec ecommerce-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd" -C -d Hshop2023 \
  -Q "SELECT COUNT(*) AS ProductCount FROM dbo.HangHoa"
```

### 4.3 Kiem tra ket noi giua Web va SQL
```bash
docker logs ecommerce-web 2>&1 | grep -i "error\|exception" || echo "Khong co loi"
```

---

## Buoc 5: Cau hinh them (tuy chon)

### 5.1 VNPay Production
Neu dung VNPay that (khong phai sandbox), cap nhat trong `.env`:
```
VNPAY_TMNCODE=...
VNPAY_HASH_SECRET=...
VNPAY_PAYMENT_URL=https://pay.vnpayment.vn/vpcpay.html
VNPAY_RETURN_URL=http://157.66.46.69:8080/cart/vnpay-return
VNPAY_IPN_URL=http://157.66.46.69:8080/cart/vnpay-ipn
```
Roi restart: `docker compose restart web`

### 5.2 Nginx Reverse Proxy (tuy chon)
Neu muon dung https qua nginx, them vao nginx config:
```nginx
server {
    listen 80;
    server_name yourdomain.com;
    location / {
        proxy_pass http://127.0.0.1:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

---

## Buoc 6: Thao tac hang ngay

### Cap nhat code moi
```bash
cd /opt/ecommerce
# Upload file moi len server, sau do:
docker compose up -d --build web  # Chi build lai web, giu nguyen SQL
```

### Xem logs
```bash
docker compose logs -f --tail=100 web
```

### Restart
```bash
docker compose restart web
docker compose restart sqlserver
```

### Dung tat ca
```bash
docker compose down
# Down + xoa data (CAN THAN):
docker compose down -v
```

### Backup database
```bash
docker exec ecommerce-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd" -C \
  -Q "BACKUP DATABASE Hshop2023 TO DISK = '/var/opt/mssql/backups/hshop2023.bak'"

# Copy ra khoi container:
docker cp ecommerce-sqlserver:/var/opt/mssql/backups/hshop2023.bak ./backup.bak
```

### Restore database
```bash
docker cp ./backup.bak ecommerce-sqlserver:/var/opt/mssql/backups/hshop2023.bak
docker exec ecommerce-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd" -C \
  -Q "RESTORE DATABASE Hshop2023 FROM DISK = '/var/opt/mssql/backups/hshop2023.bak' WITH REPLACE"
```

---

## Xu ly su co

### SQL Server khoi dong cham
```bash
docker compose logs sqlserver | grep -i "error\|fail"
# Tang thoi gian cho trong docker-compose.yml: start_period: 60s
```

### Web app khong ket noi duoc SQL
```bash
# Kiem tra network:
docker exec ecommerce-web ping -c 3 sqlserver

# Kiem tra connection string:
docker exec ecommerce-web printenv DB_CONNECTION_STRING
```

### Port 8080 da bi chiem
```bash
netstat -tlpun | grep 8080
# Neu bi chiem, doi port trong docker-compose.yml: "8081:8080" -> "8090:8080"
```

### Loi UTF-16 voi SQL scripts
Neu gap loi encoding, convert UTF-16 sang UTF-8:
```bash
iconv -f UTF-16LE -t UTF-8 HShopScript.sql > HShopScript_utf8.sql
```
