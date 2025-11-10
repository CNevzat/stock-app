# 📦 Stock Management Application

Modern, full-stack stok yönetim sistemi. .NET 9 backend ve React + TypeScript frontend ile geliştirilmiştir.

## 🌟 Özellikler

### Backend API
- ✅ **CQRS Pattern** - MediatR ile komut/sorgu ayrımı
- ✅ **Pagination** - Tüm listeleme endpoint'lerinde sayfalama desteği
- ✅ **Partial Update** - Sadece değişen alanları güncelleme
- ✅ **Filtering & Search** - Gelişmiş filtreleme ve arama
- ✅ **Swagger UI** - API test ve dokümantasyon
- ✅ **SignalR** - Real-time güncellemeler
- ✅ **SQLite Database** - Kolay geliştirme ortamı
- ✅ **Excel Export** - Ürün ve öznitelik verilerini Excel'e aktarma
- ✅ **Image Upload** - Ürün resimlerini yükleme ve yönetme
- ✅ **Exception Handling** - Merkezi hata yönetimi
- ✅ **Fiyat Yönetimi** - Ürün bazlı alış/satış fiyatı takibi, geçmiş saklama, SignalR ile canlı güncellemeler

### Frontend
- ✅ **Complete CRUD Operations** - Tüm varlıklar için tam CRUD
- ✅ **Pagination** - Sayfa navigasyonu ile sayfalama
- ✅ **Filtering** - Kategori, lokasyon ve ürün bazlı filtreleme
- ✅ **Search** - Gelişmiş arama fonksiyonları
- ✅ **Modal Forms** - Create/Edit işlemleri için modal formlar
- ✅ **Responsive Design** - Tailwind CSS ile responsive tasarım
- ✅ **Real-time Updates** - React Query ile otomatik güncelleme
- ✅ **Dashboard** - İstatistikler ve grafiklerle dashboard
- ✅ **SignalR Integration** - Real-time stok güncellemeleri
- ✅ **Fiyat Kartları & Grafikler** - Alış/satış fiyatı, ortalama ve geçmiş grafikleri
- ✅ **Excel Export Geliştirmeleri** - Fiyat bilgilerini de içeren ürün & öznitelik çıktı dosyaları

### Mobil (React Native)
- ✅ **Drawer Menü** - Kategorilere ayrılmış yan menü ve stack navigasyon
- ✅ **Web ile Parite** - Dashboard, Ürün, Kategori, Lokasyon, Stok Hareketi, Öznitelik ve Yapılacaklar ekranları
- ✅ **Ürün Fiyat Yönetimi** - Ürün oluşturma/düzenlemede alış/satış fiyatı, stok hareketlerinde birim fiyat zorunluluğu
- ✅ **Görsel Yükleme** - Ürün oluşturma ve düzenlemede medya seçimi ile görsel ekleme
- ✅ **SignalR Senkronizasyonu** - Dashboard metrikleri ve ürün detayları için canlı veri akışı

## 🚀 Teknoloji Stack

### Backend
- **.NET 9** - Web API Framework
- **Entity Framework Core 9** - ORM
- **SQLite** - Veritabanı
- **MediatR 13.1.0** - CQRS Pattern
- **SignalR** - Real-time communication
- **Swagger/OpenAPI** - API Dokümantasyonu
- **PuppeteerSharp** - PDF oluşturma
- **ClosedXML** - Excel export
- **Markdig** - Markdown işleme

### Frontend
- **React 19** - UI Framework
- **TypeScript** - Type Safety
- **Vite 7** - Build Tool
- **TanStack Query (React Query)** - State Management & Data Fetching
- **React Router** - Routing
- **Tailwind CSS** - Styling
- **Axios** - HTTP Client
- **SignalR Client** - Real-time communication
- **Recharts** - Grafik ve görselleştirme

## 📁 Proje Yapısı

```
StockApp/
├── StockApp/                          # Backend (.NET Web API)
│   ├── App/                           # CQRS Handlers
│   │   ├── Category/
│   │   │   ├── Command/               # Create, Update, Delete
│   │   │   └── Query/                 # Get, List
│   │   ├── Product/
│   │   ├── ProductAttribute/
│   │   ├── Location/
│   │   ├── StockMovement/
│   │   ├── Todo/
│   │   └── Dashboard/
│   ├── ApiEndpoints/                   # Minimal API Endpoints
│   ├── Common/
│   │   ├── Extensions/
│   │   └── Models/                    # DTOs, PaginatedList
│   ├── Entities/                      # Domain Models
│   ├── Hub/                           # SignalR Hubs
│   ├── Middleware/                    # Exception Handling
│   ├── Migrations/                    # EF Core Migrations
│   ├── Services/                      # Business Services
│   │   ├── ExcelService.cs
│   │   ├── ImageService.cs
│   │   ├── PdfService.cs
│   │   └── MarkdownService.cs
│   └── Program.cs
│
├── frontend/                          # Frontend (React + TypeScript)
│   ├── src/
│   │   ├── pages/                     # Sayfa bileşenleri
│   │   │   ├── DashboardPage.tsx
│   │   │   ├── CategoriesPage.tsx
│   │   │   ├── LocationsPage.tsx
│   │   │   ├── ProductsPage.tsx
│   │   │   ├── ProductAttributesPage.tsx
│   │   │   ├── StockMovementsPage.tsx
│   │   │   └── TodosPage.tsx
│   │   ├── services/                  # API servisleri
│   │   ├── hooks/                     # Custom hooks
│   │   │   └── useSignalR.ts
│   │   ├── App.tsx
│   │   ├── Api.ts                     # Axios configuration
│   │   └── main.tsx
│   └── package.json
│
├── API_DOCUMENTATION.md               # API Dokümantasyonu
├── KULLANICI_KILAVUZU.md             # Kullanıcı Kılavuzu
├── MEDIATR_USAGE.md                  # MediatR Kullanım Kılavuzu
├── PAGINATION_GUIDE.md               # Pagination Rehberi
├── PARTIAL_UPDATE_GUIDE.md           # Partial Update Rehberi
└── README.md                          # Bu dosya
```

## 🛠️ Kurulum

### Gereksinimler

- **.NET 9 SDK** ([İndir](https://dotnet.microsoft.com/download/dotnet/9.0))
- **Node.js 20+** ve **npm** ([İndir](https://nodejs.org/))
- **Git** (Opsiyonel)

### 1. Projeyi Klonlama

```bash
git clone <repository-url>
cd StockApp
```

### 2. Backend Kurulumu

```bash
cd StockApp
dotnet restore
dotnet ef database update  # Veritabanını oluştur
dotnet run
```

Backend çalışacak:
- **API**: `http://localhost:5132/api`
- **Swagger UI**: `http://localhost:5132/`

### 3. Frontend Kurulumu

Yeni bir terminal açın:

```bash
cd frontend
npm install
npm run dev
```

Frontend çalışacak: `http://localhost:5173`

## 📊 Veritabanı

### Entity'ler

#### Category (Kategori)
- `Id` (int) - Primary Key
- `Name` (string) - Kategori adı
- `CreatedAt` (DateTime) - Oluşturulma tarihi
- `UpdatedAt` (DateTime?) - Güncellenme tarihi
- `Products` (List<Product>) - İlişkili ürünler

#### Location (Lokasyon)
- `Id` (int) - Primary Key
- `Name` (string) - Lokasyon adı
- `Description` (string?) - Açıklama
- `CreatedAt` (DateTime) - Oluşturulma tarihi
- `UpdatedAt` (DateTime?) - Güncellenme tarihi
- `Products` (List<Product>) - İlişkili ürünler

#### Product (Ürün)
- `Id` (int) - Primary Key
- `Name` (string) - Ürün adı
- `StockCode` (string) - Benzersiz stok kodu (örn: ABC433)
- `Description` (string) - Açıklama
- `StockQuantity` (int) - Stok miktarı
- `LowStockThreshold` (int) - Düşük stok eşiği (varsayılan: 5)
- `ImagePath` (string?) - Ürün resmi yolu
- `CreatedAt` (DateTime) - Oluşturulma tarihi
- `UpdatedAt` (DateTime?) - Güncellenme tarihi
- `CategoryId` (int) - Foreign Key
- `LocationId` (int?) - Foreign Key (opsiyonel)
- `Category` (Category) - Navigation property
- `Location` (Location?) - Navigation property
- `Attributes` (List<ProductAttribute>) - Ürün öznitelikleri

#### ProductAttribute (Ürün Özniteliği)
- `Id` (int) - Primary Key
- `ProductId` (int) - Foreign Key
- `Key` (string) - Öznitelik anahtarı (örn: "RAM", "Ekran Boyutu")
- `Value` (string) - Öznitelik değeri (örn: "16GB", "15.6 inç")
- `Product` (Product) - Navigation property

#### StockMovement (Stok Hareketi)
- `Id` (int) - Primary Key
- `ProductId` (int) - Foreign Key
- `CategoryId` (int) - Foreign Key
- `Type` (StockMovementType) - Hareket tipi (In/Out)
- `Quantity` (int) - Miktar
- `Description` (string?) - Açıklama
- `CreatedAt` (DateTime) - Oluşturulma tarihi

#### TodoItem (Yapılacaklar)
- `Id` (int) - Primary Key
- `Title` (string) - Başlık
- `Description` (string?) - Açıklama
- `Status` (TodoStatus) - Durum (Todo/InProgress/Completed)
- `Priority` (TodoPriority) - Öncelik (Low/Medium/High)
- `CreatedAt` (DateTime) - Oluşturulma tarihi
- `UpdatedAt` (DateTime?) - Güncellenme tarihi

### Migration Komutları

```bash
cd StockApp

# Migration oluştur
dotnet ef migrations add MigrationName

# Migration uygula
dotnet ef database update

# Son migration'ı geri al
dotnet ef migrations remove

# Veritabanını sıfırla ve yeniden oluştur
dotnet ef database drop
dotnet ef database update
```

## 🔌 API Endpoints

Detaylı API dokümantasyonu için [API_DOCUMENTATION.md](./API_DOCUMENTATION.md) dosyasına bakın.

### Özet Endpoint'ler

#### Categories
- `GET /api/categories` - Kategorileri listele
- `GET /api/categories/by-id?id={id}` - Kategori detayı
- `POST /api/categories` - Yeni kategori oluştur
- `PUT /api/categories` - Kategori güncelle
- `DELETE /api/categories?id={id}` - Kategori sil

#### Locations
- `GET /api/locations` - Lokasyonları listele
- `GET /api/locations/by-id?id={id}` - Lokasyon detayı
- `POST /api/locations` - Yeni lokasyon oluştur
- `PUT /api/locations` - Lokasyon güncelle
- `DELETE /api/locations?id={id}` - Lokasyon sil

#### Products
- `GET /api/products` - Ürünleri listele
- `GET /api/products/by-id?id={id}` - Ürün detayı
- `POST /api/products` - Yeni ürün oluştur (multipart/form-data)
- `PUT /api/products` - Ürün güncelle (multipart/form-data)
- `DELETE /api/products?id={id}` - Ürün sil
- `GET /api/products/export/excel` - Excel'e aktar

#### Product Attributes
- `GET /api/product-attributes` - Öznitelikleri listele
- `GET /api/product-attributes/by-id?id={id}` - Öznitelik detayı
- `POST /api/product-attributes` - Yeni öznitelik oluştur
- `PUT /api/product-attributes` - Öznitelik güncelle
- `DELETE /api/product-attributes?id={id}` - Öznitelik sil
- `GET /api/product-attributes/export/excel` - Excel'e aktar

#### Stock Movements
- `GET /api/stock-movements` - Stok hareketlerini listele
- `POST /api/stock-movements` - Yeni stok hareketi oluştur

#### Todos
- `GET /api/todos` - Yapılacakları listele
- `POST /api/todos` - Yeni yapılacak oluştur
- `PUT /api/todos/{id}` - Yapılacak güncelle
- `DELETE /api/todos/{id}` - Yapılacak sil

#### Dashboard
- `GET /api/dashboard/stats` - Dashboard istatistikleri

## 🎯 Önemli Özellikler

### 1. Pagination (Sayfalama)

Tüm listeleme endpoint'leri paginated response döner:

```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 50,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### 2. Partial Update (Kısmi Güncelleme)

Sadece değişen alanları gönderin:

```json
// Sadece stok miktarını güncelle
PUT /api/products
{
  "id": 1,
  "stockQuantity": 50
}
```

### 3. Filtering & Search (Filtreleme ve Arama)

```
// Kategoriye göre filtrele
GET /api/products?categoryId=1

// Arama yap
GET /api/products?searchTerm=laptop

// Kombine filtreler
GET /api/products?pageNumber=1&categoryId=1&locationId=2&searchTerm=gaming
```

### 4. Real-time Updates (SignalR)

Dashboard istatistikleri ve stok güncellemeleri real-time olarak tüm bağlı client'lara gönderilir.

## 📚 Dokümantasyon

- **[API_DOCUMENTATION.md](./API_DOCUMENTATION.md)** - Kapsamlı API dokümantasyonu
- **[KULLANICI_KILAVUZU.md](./KULLANICI_KILAVUZU.md)** - Kullanıcı kılavuzu
- **[MEDIATR_USAGE.md](./MEDIATR_USAGE.md)** - MediatR & CQRS dokümantasyonu
- **[PAGINATION_GUIDE.md](./PAGINATION_GUIDE.md)** - Pagination detaylı rehberi
- **[PARTIAL_UPDATE_GUIDE.md](./PARTIAL_UPDATE_GUIDE.md)** - Partial update örnekleri

## 🧪 Test Etme

### Swagger UI ile Test

1. Backend'i çalıştırın: `dotnet run`
2. Tarayıcıda açın: `http://localhost:5132/`
3. Swagger UI'da endpoint'leri test edin

### HTTP Client ile Test

`StockApp/test-requests.http` dosyasını IDE'nizin HTTP client'ı ile kullanabilirsiniz.

## 🔒 CORS Yapılandırması

Backend şu origin'lerden gelen isteklere izin verir:
- `http://localhost:5173` (Vite default)
- `http://localhost:5174` (Vite alternatif)
- `http://localhost:3000` (Create React App default)

Yeni origin eklemek için `StockApp/Program.cs` dosyasını düzenleyin:

```csharp
policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "https://yourdomain.com")
```

## 🛠️ Geliştirme

### Backend Geliştirme

```bash
cd StockApp

# Watch mode (otomatik rebuild)
dotnet watch run

# Build
dotnet build

# Test
dotnet test
```

### Frontend Geliştirme

```bash
cd frontend

# Development server
npm run dev

# Production build
npm run build

# Preview production build
npm run preview

# Lint
npm run lint
```

## 📈 Performans

- **Backend**: EF Core eager loading ve projection ile optimize edilmiş sorgular
- **Frontend**: React Query ile otomatik caching ve refetching
- **Pagination**: Veri transferini azaltır ve response sürelerini iyileştirir
- **Partial Updates**: Sadece değişen veriler gönderilir

## 🐛 Sorun Giderme

### Backend Port Zaten Kullanımda

```bash
# macOS/Linux
lsof -ti:5132 | xargs kill -9

# Windows
netstat -ano | findstr :5132
taskkill /PID <PID> /F
```

### Frontend CORS Hatası

Kontrol edin:
1. Backend çalışıyor mu?
2. CORS policy frontend URL'inizi içeriyor mu?
3. `Program.cs`'de `UseCors()` `UseHttpsRedirection()`'dan önce çağrılıyor mu?

### Veritabanı Sorunları

```bash
# Veritabanını sil ve yeniden oluştur
rm StockApp/stockapp.db
cd StockApp
dotnet ef database update
```

### Migration Sorunları

```bash
# Tüm migration'ları geri al
dotnet ef database drop

# Migration'ları yeniden uygula
dotnet ef database update
```

## 📝 Lisans

MIT License

## 👥 Katkıda Bulunma

1. Repository'yi fork edin
2. Feature branch oluşturun (`git checkout -b feature/AmazingFeature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add some AmazingFeature'`)
4. Branch'inizi push edin (`git push origin feature/AmazingFeature`)
5. Pull Request açın

## 📞 İletişim

Sorularınız veya destek için lütfen GitHub'da issue açın.

## 🙏 Teşekkürler

Bu projeyi kullandığınız için teşekkürler! ⭐
