# 🚀 Production Deployment Guide

Bu dokümantasyon, StockApp'in production ortamına nasıl deploy edileceğini açıklar.

## 📦 Redis Kurulum Seçenekleri

### Seçenek 1: Docker ile Redis (Önerilen) ⭐

#### Avantajlar:
- ✅ Kolay kurulum ve yönetim
- ✅ Otomatik restart
- ✅ Data persistence (volume ile)
- ✅ İzolasyon (container içinde)

#### Kurulum:

```bash
# docker-compose.yml dosyası ile
docker-compose up -d

# Veya sadece Redis
docker run -d \
  --name stockapp-redis \
  -p 6379:6379 \
  -v redis-data:/data \
  redis:7-alpine \
  redis-server --appendonly yes
```

#### Connection String:
```json
"Redis": "redis:6379"  // Docker network içinde
// veya
"Redis": "localhost:6379"  // Host'tan erişim için
```

---

### Seçenek 2: Sunucuya Redis Kurma

#### Linux (Ubuntu/Debian):
```bash
# Redis kurulumu
sudo apt-get update
sudo apt-get install redis-server

# Redis'i başlat
sudo systemctl start redis
sudo systemctl enable redis

# Durum kontrolü
sudo systemctl status redis
```

#### Linux (CentOS/RHEL):
```bash
# EPEL repository ekle
sudo yum install epel-release
sudo yum install redis

# Redis'i başlat
sudo systemctl start redis
sudo systemctl enable redis
```

#### Windows Server:
```powershell
# Chocolatey ile
choco install redis-64

# Veya manuel indirme
# https://github.com/microsoftarchive/redis/releases
```

#### Connection String:
```json
"Redis": "localhost:6379"  // Aynı sunucuda
// veya
"Redis": "192.168.1.100:6379"  // Farklı sunucuda
```

---

### Seçenek 3: Managed Redis Servisleri (Cloud) ☁️

#### Azure Redis Cache:
```json
"Redis": "your-cache.redis.cache.windows.net:6380,password=YOUR_PASSWORD,ssl=True"
```

#### AWS ElastiCache:
```json
"Redis": "your-cluster.xxxxx.cache.amazonaws.com:6379"
```

#### Redis Cloud (Redis Labs):
```json
"Redis": "redis-12345.c1.us-east-1-1.ec2.cloud.redislabs.com:12345,password=YOUR_PASSWORD"
```

#### Avantajlar:
- ✅ Yönetilen servis (bakım yok)
- ✅ Otomatik backup
- ✅ High availability
- ✅ Monitoring dahil

---

## 🔧 Production Yapılandırması

### 1. appsettings.Production.json Oluştur

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/app/data/stockapp.db",
    "Redis": "redis:6379"
  },
  "Jwt": {
    "SecretKey": "PRODUCTION_SECRET_KEY_FROM_ENVIRONMENT_VARIABLE",
    "Issuer": "StockApp",
    "Audience": "StockAppUsers"
  }
}
```

### 2. Environment Variables Kullan (Önerilen)

```bash
# Linux/Mac
export ConnectionStrings__Redis="redis:6379"
export Jwt__SecretKey="your-production-secret-key"

# Windows
set ConnectionStrings__Redis=redis:6379
set Jwt__SecretKey=your-production-secret-key
```

Veya `.env` dosyası:
```env
ConnectionStrings__Redis=redis:6379
Jwt__SecretKey=your-production-secret-key
```

### 3. Docker Compose ile Tam Stack

```yaml
version: '3.8'

services:
  redis:
    image: redis:7-alpine
    container_name: stockapp-redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    command: redis-server --appendonly yes
    restart: unless-stopped
    networks:
      - stockapp-network

  app:
    build: .
    container_name: stockapp-api
    ports:
      - "5134:5134"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Redis=redis:6379
      - ConnectionStrings__DefaultConnection=Data Source=/app/data/stockapp.db
    volumes:
      - app-data:/app/data
    depends_on:
      - redis
    restart: unless-stopped
    networks:
      - stockapp-network

volumes:
  redis-data:
  app-data:

networks:
  stockapp-network:
    driver: bridge
```

---

## 🔐 Güvenlik Önerileri

### 1. Redis Şifre Koruması

```bash
# redis.conf dosyası
requirepass YOUR_STRONG_PASSWORD
```

Connection string:
```json
"Redis": "redis:6379,password=YOUR_STRONG_PASSWORD"
```

### 2. Redis Network İzolasyonu

```yaml
# docker-compose.yml
services:
  redis:
    # Sadece internal network'te erişilebilir
    ports: []  # Port expose etme
    networks:
      - stockapp-network
```

### 3. JWT Secret Key

```bash
# Environment variable'dan oku
export Jwt__SecretKey="$(openssl rand -base64 32)"
```

---

## 📊 Deployment Senaryoları

### Senaryo 1: Tek Sunucu (VPS/Dedicated)

```
┌─────────────────────────┐
│   Sunucu (Linux)        │
│                         │
│  ┌──────────────┐       │
│  │  .NET App    │       │
│  │  (Port 5134) │       │
│  └──────┬───────┘       │
│         │                │
│  ┌──────▼───────┐       │
│  │   Redis      │       │
│  │  (Port 6379) │       │
│  └──────────────┘       │
│                         │
│  ┌──────────────┐       │
│  │  SQLite DB   │       │
│  └──────────────┘       │
└─────────────────────────┘
```

**Kurulum:**
```bash
# Redis kur
sudo apt-get install redis-server

# App'i çalıştır
dotnet run --environment Production
```

---

### Senaryo 2: Docker Compose

```
┌─────────────────────────────┐
│   Docker Host               │
│                             │
│  ┌──────────────┐          │
│  │  App Container│          │
│  │  (Port 5134)  │          │
│  └──────┬───────┘          │
│         │                   │
│  ┌──────▼───────┐          │
│  │ Redis Container│         │
│  │  (Port 6379)   │          │
│  └───────────────┘          │
└─────────────────────────────┘
```

**Kurulum:**
```bash
docker-compose up -d
```

---

### Senaryo 3: Cloud (Azure/AWS)

```
┌─────────────────┐
│  App Service    │
│  (Azure/AWS)    │
└──────┬──────────┘
       │
┌──────▼──────────┐
│  Managed Redis  │
│  (Azure/AWS)    │
└─────────────────┘
```

**Kurulum:**
- Azure: App Service + Azure Redis Cache
- AWS: ECS/EC2 + ElastiCache

---

## 🧪 Test Etme

### 1. Redis Bağlantısını Test Et

```bash
# Redis CLI ile
redis-cli ping
# PONG dönmeli

# Veya password ile
redis-cli -a YOUR_PASSWORD ping
```

### 2. Uygulama Log'larını Kontrol Et

```bash
# Redis bağlantı log'ları
dotnet run

# Başarılı bağlantı:
# info: Microsoft.Extensions.Caching.StackExchangeRedis...
```

### 3. Cache'in Çalıştığını Doğrula

```bash
# Redis CLI
redis-cli
KEYS StockApp:*
# Key'ler görünmeli
```

---

## 📝 Checklist

- [ ] Redis kuruldu ve çalışıyor
- [ ] `appsettings.Production.json` oluşturuldu
- [ ] Connection string doğru yapılandırıldı
- [ ] JWT Secret Key güvenli (environment variable)
- [ ] Redis şifre koruması aktif (production için)
- [ ] Firewall kuralları ayarlandı (gerekirse)
- [ ] Redis persistence aktif (AOF veya RDB)
- [ ] Monitoring kuruldu (opsiyonel)

---

## 🔍 Troubleshooting

### Redis'e Bağlanamıyor

```bash
# Redis çalışıyor mu?
sudo systemctl status redis

# Port açık mı?
netstat -tuln | grep 6379

# Firewall kontrolü
sudo ufw status
```

### Connection String Formatı

```json
// Basit
"Redis": "localhost:6379"

// Password ile
"Redis": "localhost:6379,password=YOUR_PASSWORD"

// SSL ile
"Redis": "your-redis.com:6380,password=YOUR_PASSWORD,ssl=True"

// Database seçimi (0-15)
"Redis": "localhost:6379,defaultDatabase=0"
```

---

## 📚 Kaynaklar

- [Redis Official Documentation](https://redis.io/docs/)
- [.NET Distributed Caching](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
- [Docker Redis Image](https://hub.docker.com/_/redis)
- [Azure Redis Cache](https://azure.microsoft.com/en-us/services/cache/)
- [AWS ElastiCache](https://aws.amazon.com/elasticache/)

