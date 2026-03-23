# 📦 Stock Management Application

Modern, full-stack stok yönetim sistemi. .NET 10 backend ve React + TypeScript frontend ile geliştirilmiştir.

##  Özellikler

### Backend API
- **CQRS Pattern** - MediatR ile komut/sorgu ayrımı
- **Pagination** - Tüm listeleme endpoint'lerinde sayfalama desteği
- **Partial Update** - Sadece değişen alanları güncelleme
- **Filtering & Search** - Gelişmiş filtreleme ve arama
- **Swagger UI** - API test ve dokümantasyon
- **SignalR** - Real-time güncellemeler
- **SQLite Database** - Kolay geliştirme ortamı
- **Excel Export** - Ürün ve öznitelik verilerini Excel'e aktarma
- **Stock Movement Export** - Tüm stok hareketlerini Excel'e aktarma
- **Image Upload** - Ürün resimlerini yükleme ve yönetme
- **Exception Handling** - Merkezi hata yönetimi
- **Fiyat Yönetimi** - Ürün bazlı alış/satış fiyatı takibi, geçmiş saklama, SignalR ile canlı güncellemeler
- **Doğal Dil Raporlama** - Gemini API ile soru-cevap tabanlı rapor üretimi

### Frontend
- **Complete CRUD Operations** - Tüm varlıklar için tam CRUD
- **Pagination** - Sayfa navigasyonu ile sayfalama
- **Filtering** - Kategori, lokasyon ve ürün bazlı filtreleme
- **Search** - Gelişmiş arama fonksiyonları
- **Modal Forms** - Create/Edit işlemleri için modal formlar
- **Responsive Design** - Tailwind CSS ile responsive tasarım
- **Real-time Updates** - React Query ile otomatik güncelleme
- **Dashboard** - İstatistikler ve grafiklerle dashboard
- **SignalR Integration** - Real-time stok güncellemeleri
- **Fiyat Kartları & Grafikler** - Alış/satış fiyatı, ortalama ve geçmiş grafikleri
- **Excel Export Geliştirmeleri** - Fiyat bilgilerini de içeren ürün, öznitelik ve stok hareketi çıktı dosyaları

### Mobil (React Native)
- **Drawer Menü** - Kategorilere ayrılmış yan menü ve stack navigasyon
- **Web ile Parite** - Dashboard, Ürün, Kategori, Lokasyon, Stok Hareketi, Öznitelik ve Yapılacaklar ekranları
- **Ürün Fiyat Yönetimi** - Ürün oluşturma/düzenlemede alış/satış fiyatı, stok hareketlerinde birim fiyat zorunluluğu
- **Görsel Yükleme** - Ürün oluşturma ve düzenlemede medya seçimi ile görsel ekleme
- **SignalR Senkronizasyonu** - Dashboard metrikleri ve ürün detayları için canlı veri akışı

##  Teknoloji Stack

### Backend
- **.NET 10** - Web API Framework
- **Entity Framework Core 10** - ORM
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

Monorepo: **backend** ve **frontend** ayrı klasörlerde; bağımsız geliştirilebilir ve ileride ayrı repolara taşınabilir.

```
stock-app/                             # Depo kökü
├── backend/                           # Backend (.NET Web API, proje adı: StockApp)
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
│   ├── Controllers/                   # API controllers
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

- **.NET 10 SDK** ([İndir](https://dotnet.microsoft.com/download/dotnet/10.0))
- **Node.js 20+** ve **npm** ([İndir](https://nodejs.org/))


### 1. Projeyi Klonlama

```bash
git clone <repository-url>
cd stock-app
```

### 2. Backend Kurulumu

```bash
cd backend
dotnet restore
dotnet ef database update  # Veritabanını oluştur
dotnet run
```

### 3. Frontend Kurulumu

```bash
cd frontend
npm install
npm run dev
```

##  Veritabanı

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


##  API Endpoints

### Endpoint'ler

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
- `GET /api/stock-movements/export/excel` - Excel'e aktar

#### Reports
- `GET /api/reports/critical-stock/pdf` - Kritik stok PDF raporu
- `POST /api/reports/natural-language` - Gemini destekli doğal dil raporu üret

#### Todos
- `GET /api/todos` - Yapılacakları listele
- `POST /api/todos` - Yeni yapılacak oluştur
- `PUT /api/todos/{id}` - Yapılacak güncelle
- `DELETE /api/todos/{id}` - Yapılacak sil

#### Dashboard
- `GET /api/dashboard/stats` - Dashboard istatistikleri


## 🛠️ Geliştirme

### Backend Geliştirme

```bash
cd backend
dotnet watch run
dotnet build
dotnet test
```

### Frontend Geliştirme

```bash
cd frontend
npm run dev
npm run build
npm run preview
```
