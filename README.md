# Stock Management Application

Modern, full-stack stok yönetim sistemi. .NET 10 backend ve React + TypeScript frontend ile geliştirilmiştir.

## Özellikler

### Backend
- **CQRS Pattern** — MediatR ile komut/sorgu ayrımı
- **PostgreSQL** — Ana veritabanı; EF Core 10 + Npgsql
- **Redis** — Liste ve dashboard önbellekleme; nesil (generation) tabanlı cache geçersizleştirme
- **Elasticsearch** — Ürün, stok hareketi ve öznitelik araması; ürün listesi arama varsa ES, yoksa DB
- **MinIO** — Ürün görselleri için nesne depolama (S3 uyumlu); wwwroot kullanılmaz
- **Hangfire** — Arka plan işleri; kategori güncellemesi sonrası ES toplu yeniden indeksleme, PostgreSQL ile dayanıklı kuyruk
- **SignalR** — Gerçek zamanlı ürün ve dashboard güncellemeleri
- **JWT Kimlik Doğrulama** — Role dayalı yetkilendirme (Admin / Manager / User)
- **Swagger UI** — Geliştirme ortamında otomatik etkin
- **Pagination** — Tüm listeleme endpoint'lerinde sayfalama
- **Partial Update** — Sadece değişen alanları güncelleme
- **Excel Export** — Ürün, öznitelik ve stok hareketi çıktıları
- **PDF Rapor** — Kritik stok seviyesi raporu
- **Doğal Dil Raporlama** — Gemini API ile soru-cevap tabanlı rapor üretimi

### Frontend
- **React 19 + TypeScript** — UI
- **TanStack Query** — Sunucu durumu ve önbellekleme
- **Tailwind CSS** — Responsive tasarım
- **Recharts** — Fiyat geçmişi ve trend grafikleri
- **SignalR** — Gerçek zamanlı stok ve dashboard senkronizasyonu
- **Yönetim menüsü** — Admin paneli; Hangfire Dashboard bağlantısı dahil

### Mobil (React Native / Capacitor)
- Dashboard, Ürün, Kategori, Lokasyon, Stok Hareketi, Öznitelik ve Yapılacaklar ekranları
- Görsel yükleme (MinIO üzerinden)
- SignalR senkronizasyonu

---

## Teknoloji Stack

| Katman | Teknoloji |
|--------|-----------|
| Runtime | .NET 10 |
| ORM | Entity Framework Core 10 + Npgsql |
| Veritabanı | PostgreSQL 16 |
| Önbellek | Redis 7 |
| Arama | Elasticsearch 7.17 (NEST) |
| Görsel depolama | MinIO (S3 uyumlu) |
| Arka plan işler | Hangfire 1.8 + Hangfire.PostgreSql |
| Gerçek zamanlı | ASP.NET Core SignalR |
| Auth | ASP.NET Core Identity + JWT Bearer |
| UI | React 19, Vite 7, Tailwind CSS |
| Grafik | Recharts |
| HTTP | TanStack Query (React Query) |

---

## Proje Yapısı

```
stock-app/
├── backend/
│   ├── App/                        # CQRS Handlers (MediatR)
│   │   ├── Category/Command|Query
│   │   ├── Product/Command|Query
│   │   ├── ProductAttribute/Command|Query
│   │   ├── StockMovement/Command|Query
│   │   ├── Elasticsearch/Command   # Reindex komutları
│   │   ├── Dashboard/Query
│   │   └── Todo/Command|Query
│   ├── Controllers/                # API endpoint'leri
│   ├── Common/Constants|Models     # CacheKeys, PaginatedList vb.
│   ├── Database/                   # ApplicationDbContext
│   ├── Entities/                   # Domain modelleri
│   ├── Hub/                        # SignalR Hub
│   ├── Infrastructure/             # HangfireAdminAuthorizationFilter
│   ├── Middleware/                  # Merkezi hata yönetimi
│   ├── Migrations/                 # EF Core migrations (PostgreSQL)
│   ├── Options/                    # Yapılandırma sınıfları (JWT, MinIO vb.)
│   ├── Services/
│   │   ├── CacheService.cs         # Redis önbellekleme
│   │   ├── CategoryElasticsearchReindexService.cs  # Hangfire job servisi
│   │   ├── DatabaseSeeder.cs
│   │   ├── ElasticsearchService.cs # ES arama + bulk index
│   │   ├── ExcelService.cs
│   │   ├── ImageService.cs         # MinIO görsel yükleme/silme
│   │   ├── MinioFileService.cs     # MinIO S3 istemcisi
│   │   └── PdfService.cs
│   ├── docker-compose.yml          # PostgreSQL, Redis, ES, MinIO, API
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
│
└── frontend/
    └── src/
        ├── pages/
        ├── services/
        ├── utils/
        ├── components/
        ├── App.tsx
        └── main.tsx
```

---

## Kurulum

### Gereksinimler

- **.NET 10 SDK**
- **Node.js 20+** ve **npm**
- **Docker Desktop** (PostgreSQL, Redis, Elasticsearch, MinIO için)

### 1. Projeyi Klonla

```bash
git clone <repository-url>
cd stock-app
```

### 2. Altyapıyı Başlat (Docker)

```bash
cd backend
docker compose up -d
```

Bu komut şunları başlatır:
- **PostgreSQL 16** — `localhost:5432`
- **Redis 7** — `localhost:6379`
- **Elasticsearch 7.17** — `localhost:9200`
- **MinIO** — API `localhost:9000`, Console `localhost:9001`
- **Backend API** — `localhost:5134`

### 3. Frontend Kurulumu

```bash
cd frontend
npm install
npm run dev
```

Frontend `http://localhost:5173` adresinde açılır.

---

## Yapılandırma

### appsettings.Development.json (örnek)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=stockapp;Username=stockapp;Password=stockapp",
    "Redis": "localhost:6379",
    "Elasticsearch": "http://localhost:9200"
  },
  "Minio": {
    "Enabled": true,
    "Endpoint": "localhost:9000",
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin",
    "Bucket": "stockapp-images",
    "BaseUrl": "http://localhost:9000",
    "UseSSL": false
  },
  "Jwt": {
    "SecretKey": "<en-az-32-karakter>",
    "Issuer": "StockApp",
    "Audience": "StockAppUsers",
    "ExpiryMinutes": 60
  }
}
```

> MinIO görsel yükleme için zorunludur; `Minio:Enabled` false iken görsel yükleme hata verir.

---

## Görsel Depolama (MinIO)

Ürün görselleri MinIO nesne deposuna yüklenir; `wwwroot` kullanılmaz.

- Bucket otomatik oluşturulur ve anonim okuma politikası uygulanır (tarayıcıdan doğrudan URL erişimi).
- Görseller `<BaseUrl>/<bucket>/<dosyaadı>` şeklinde URL olarak saklanır.
- MinIO Console: `http://localhost:9001` (kullanıcı: `minioadmin` / şifre: `minioadmin`)

---

## Elasticsearch

### Arama Davranışı

| Senaryo | Kaynak |
|---------|--------|
| Ürün listesi — arama metni var | Elasticsearch |
| Ürün listesi — arama metni yok | PostgreSQL |
| Stok hareketleri — `productId` dolu, arama yok | PostgreSQL |
| Stok hareketleri — genel liste / arama | Elasticsearch |

### İndeksleme

- **Ürün ekle / güncelle / sil** → anlık ES güncellemesi
- **Stok hareketi oluştur** → anlık ES indexleme
- **Kategori adı güncelle** → Hangfire kuyruğuna alınır; arka planda ilgili tüm ürünler ve stok hareketleri 500'er satır (bulk) olarak yeniden indekslenir

### Reindex (Manuel)

```
POST /api/elasticsearch/reindex-products
POST /api/elasticsearch/reindex-stock-movements
POST /api/elasticsearch/reindex-product-attributes
```

---

## Hangfire

Kategori gibi büyük veri kümelerini etkileyen işlemler Hangfire ile arka planda çalıştırılır:

- **Storage:** PostgreSQL (ek altyapı gerektirmez; uygulama yeniden başlasa da işler kaybolmaz)
- **Worker:** 2 thread, `category-reindex` ve `default` kuyrukları
- **Retry:** Hata durumunda otomatik yeniden deneme

### Dashboard

```
http://localhost:5134/hangfire
```

Localhost'tan erişimde giriş gerekmez. Uzaktan erişimde Admin rolü zorunludur.

---

## API Endpoint'leri

### Categories
- `GET /api/categories` — Listele
- `GET /api/categories/by-id?id={id}` — Detay
- `POST /api/categories` — Oluştur
- `PUT /api/categories` — Güncelle
- `DELETE /api/categories?id={id}` — Sil

### Locations
- `GET /api/locations` — Listele
- `GET /api/locations/by-id?id={id}` — Detay
- `POST /api/locations` — Oluştur
- `PUT /api/locations` — Güncelle
- `DELETE /api/locations?id={id}` — Sil

### Products
- `GET /api/products` — Listele (arama + kategori/lokasyon filtresi)
- `GET /api/products/by-id?id={id}` — Detay (fiyat geçmişi dahil)
- `POST /api/products` — Oluştur (`multipart/form-data`)
- `PUT /api/products` — Güncelle (`multipart/form-data`)
- `DELETE /api/products?id={id}` — Sil
- `GET /api/products/export/excel` — Excel'e aktar

### Product Attributes
- `GET /api/product-attributes` — Listele
- `POST /api/product-attributes` — Oluştur
- `PUT /api/product-attributes` — Güncelle
- `DELETE /api/product-attributes?id={id}` — Sil
- `GET /api/product-attributes/export/excel` — Excel'e aktar

### Stock Movements
- `GET /api/stock-movements` — Listele (arama + filtreler)
- `POST /api/stock-movements` — Oluştur
- `GET /api/stock-movements/export/excel` — Excel'e aktar

### Dashboard
- `GET /api/dashboard/stats` — İstatistikler

### Reports
- `GET /api/reports/critical-stock/pdf` — Kritik stok PDF raporu
- `POST /api/reports/natural-language` — Gemini destekli doğal dil raporu

### Auth
- `POST /api/auth/login` — Giriş
- `POST /api/auth/refresh` — Token yenile
- `POST /api/auth/change-password` — Şifre değiştir

### Elasticsearch (Admin)
- `POST /api/elasticsearch/reindex-products`
- `POST /api/elasticsearch/reindex-stock-movements`
- `POST /api/elasticsearch/reindex-product-attributes`
- `GET /api/elasticsearch/status`

---

## Geliştirme

```bash
# Backend
cd backend
dotnet restore
dotnet watch run

# Frontend
cd frontend
npm run dev
```

---

## Varsayılan Kullanıcılar (Seed)

| Kullanıcı | Şifre | Rol |
|-----------|-------|-----|
| admin@stockapp.com | Admin123! | Admin |
| manager@stockapp.com | Manager123! | Manager |
| user@stockapp.com | User123! | User |
