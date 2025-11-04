# 📡 API Dokümantasyonu

Bu dokümantasyon Stock Management Application API'sinin tüm endpoint'lerini ve kullanımlarını detaylı olarak açıklar.

## 📋 İçindekiler

- [Temel Bilgiler](#temel-bilgiler)
- [Authentication](#authentication)
- [Response Formatları](#response-formatları)
- [Hata Yönetimi](#hata-yönetimi)
- [Categories API](#categories-api)
- [Locations API](#locations-api)
- [Products API](#products-api)
- [Product Attributes API](#product-attributes-api)
- [Stock Movements API](#stock-movements-api)
- [Todos API](#todos-api)
- [Dashboard API](#dashboard-api)
- [SignalR Hub](#signalr-hub)

## Temel Bilgiler

### Base URL

```
Development: http://localhost:5132
Production: https://yourdomain.com
```

### API Versiyonu

API versiyonu: **v1**

### Content-Type

API çoğunlukla JSON formatında veri alır ve döner:

```
Content-Type: application/json
```

Ancak ürün oluşturma ve güncelleme endpoint'leri `multipart/form-data` kullanır (resim yükleme için).

### Swagger UI

Development ortamında Swagger UI mevcuttur:

```
http://localhost:5132/
```

## Authentication

Şu anda API authentication gerektirmez. Production ortamında JWT veya başka bir authentication mekanizması eklenebilir.

## Response Formatları

### Başarılı Response

```json
{
  "id": 1,
  "name": "Example",
  ...
}
```

### Paginated Response

Listeleme endpoint'leri paginated response döner:

```json
{
  "items": [
    {
      "id": 1,
      "name": "Example 1"
    },
    {
      "id": 2,
      "name": "Example 2"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 50,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Hata Response

```json
{
  "error": "Error message",
  "statusCode": 400
}
```

## Hata Yönetimi

### HTTP Status Kodları

- `200 OK` - İşlem başarılı
- `400 Bad Request` - Geçersiz istek
- `404 Not Found` - Kaynak bulunamadı
- `500 Internal Server Error` - Sunucu hatası

### Hata Örnekleri

#### 404 Not Found

```json
{
  "error": "Product with ID 999 not found.",
  "statusCode": 404
}
```

#### 400 Bad Request

```json
{
  "error": "Validation failed",
  "statusCode": 400,
  "errors": {
    "name": ["Name field is required"]
  }
}
```

---

## Categories API

### Kategorileri Listele

Tüm kategorileri sayfalama ile listeler.

**Endpoint:** `GET /api/categories`

**Query Parameters:**

| Parametre | Tip | Gerekli | Varsayılan | Açıklama |
|-----------|-----|---------|------------|----------|
| `pageNumber` | int | Hayır | 1 | Sayfa numarası |
| `pageSize` | int | Hayır | 10 | Sayfa başına kayıt sayısı |
| `searchTerm` | string | Hayır | - | Arama terimi (isimde arama) |

**Örnek İstek:**

```http
GET /api/categories?pageNumber=1&pageSize=10&searchTerm=elektronik
```

**Örnek Response:**

```json
{
  "items": [
    {
      "id": 1,
      "name": "Elektronik",
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": null
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Kategori Detayı

Belirli bir kategorinin detaylarını getirir.

**Endpoint:** `GET /api/categories/by-id`

**Query Parameters:**

| Parametre | Tip | Gerekli | Açıklama |
|-----------|-----|---------|----------|
| `id` | int | Evet | Kategori ID |

**Örnek İstek:**

```http
GET /api/categories/by-id?id=1
```

**Örnek Response:**

```json
{
  "id": 1,
  "name": "Elektronik",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null,
  "productCount": 15
}
```

### Yeni Kategori Oluştur

**Endpoint:** `POST /api/categories`

**Request Body:**

```json
{
  "name": "Elektronik"
}
```

**Örnek Response:**

```json
{
  "categoryId": 1,
  "message": "Category created successfully"
}
```

### Kategori Güncelle

**Endpoint:** `PUT /api/categories`

**Request Body:**

```json
{
  "id": 1,
  "name": "Elektronik Ürünler"
}
```

**Not:** Partial update desteklenir. Sadece değiştirmek istediğiniz alanları gönderebilirsiniz.

**Örnek Response:**

```json
{
  "categoryId": 1,
  "message": "Category updated successfully"
}
```

### Kategori Sil

**Endpoint:** `DELETE /api/categories`

**Query Parameters:**

| Parametre | Tip | Gerekli | Açıklama |
|-----------|-----|---------|----------|
| `id` | int | Evet | Kategori ID |

**Örnek İstek:**

```http
DELETE /api/categories?id=1
```

**Örnek Response:**

```json
{
  "categoryId": 1,
  "message": "Category deleted successfully"
}
```

---

## Locations API

### Lokasyonları Listele

**Endpoint:** `GET /api/locations`

**Query Parameters:**

| Parametre | Tip | Gerekli | Varsayılan | Açıklama |
|-----------|-----|---------|------------|----------|
| `pageNumber` | int | Hayır | 1 | Sayfa numarası |
| `pageSize` | int | Hayır | 10 | Sayfa başına kayıt sayısı |
| `searchTerm` | string | Hayır | - | Arama terimi |

**Örnek İstek:**

```http
GET /api/locations?pageNumber=1&pageSize=10
```

**Örnek Response:**

```json
{
  "items": [
    {
      "id": 1,
      "name": "Depo A",
      "description": "Ana depo",
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": null
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 5,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### Lokasyon Detayı

**Endpoint:** `GET /api/locations/by-id`

**Query Parameters:**

| Parametre | Tip | Gerekli | Açıklama |
|-----------|-----|---------|----------|
| `id` | int | Evet | Lokasyon ID |

**Örnek İstek:**

```http
GET /api/locations/by-id?id=1
```

### Yeni Lokasyon Oluştur

**Endpoint:** `POST /api/locations`

**Request Body:**

```json
{
  "name": "Depo A",
  "description": "Ana depo, üst kat"
}
```

**Örnek Response:**

```json
{
  "locationId": 1,
  "message": "Location created successfully"
}
```

### Lokasyon Güncelle

**Endpoint:** `PUT /api/locations`

**Request Body:**

```json
{
  "id": 1,
  "name": "Depo A - Güncellenmiş",
  "description": "Yeni açıklama"
}
```

### Lokasyon Sil

**Endpoint:** `DELETE /api/locations`

**Query Parameters:**

| Parametre | Tip | Gerekli | Açıklama |
|-----------|-----|---------|----------|
| `id` | int | Evet | Lokasyon ID |

---

## Products API

### Ürünleri Listele

**Endpoint:** `GET /api/products`

**Query Parameters:**

| Parametre | Tip | Gerekli | Varsayılan | Açıklama |
|-----------|-----|---------|------------|----------|
| `pageNumber` | int | Hayır | 1 | Sayfa numarası |
| `pageSize` | int | Hayır | 10 | Sayfa başına kayıt sayısı |
| `categoryId` | int | Hayır | - | Kategori ID ile filtrele |
| `locationId` | int | Hayır | - | Lokasyon ID ile filtrele |
| `searchTerm` | string | Hayır | - | Arama terimi (isim, stok kodu, açıklama) |

**Örnek İstek:**

```http
GET /api/products?pageNumber=1&pageSize=10&categoryId=1&searchTerm=laptop
```

**Örnek Response:**

```json
{
  "items": [
    {
      "id": 1,
      "name": "Gaming Laptop",
      "stockCode": "ABC433",
      "description": "Yüksek performanslı gaming laptop",
      "stockQuantity": 15,
      "lowStockThreshold": 5,
      "imagePath": "/images/products/1.jpg",
      "categoryId": 1,
      "categoryName": "Elektronik",
      "locationId": 1,
      "locationName": "Depo A",
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": null
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 50,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Ürün Detayı

**Endpoint:** `GET /api/products/by-id`

**Query Parameters:**

| Parametre | Tip | Gerekli | Açıklama |
|-----------|-----|---------|----------|
| `id` | int | Evet | Ürün ID |

**Örnek İstek:**

```http
GET /api/products/by-id?id=1
```

**Örnek Response:**

```json
{
  "id": 1,
  "name": "Gaming Laptop",
  "stockCode": "ABC433",
  "description": "Yüksek performanslı gaming laptop",
  "stockQuantity": 15,
  "lowStockThreshold": 5,
  "imagePath": "/images/products/1.jpg",
  "categoryId": 1,
  "categoryName": "Elektronik",
  "locationId": 1,
  "locationName": "Depo A",
  "attributes": [
    {
      "id": 1,
      "key": "RAM",
      "value": "16GB"
    },
    {
      "id": 2,
      "key": "Ekran Boyutu",
      "value": "15.6 inç"
    }
  ],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

### Yeni Ürün Oluştur

**Endpoint:** `POST /api/products`

**Content-Type:** `multipart/form-data`

**Form Data:**

| Alan | Tip | Gerekli | Açıklama |
|------|-----|---------|----------|
| `name` | string | Evet | Ürün adı |
| `description` | string | Hayır | Ürün açıklaması |
| `stockQuantity` | int | Evet | Stok miktarı |
| `lowStockThreshold` | int | Hayır | Düşük stok eşiği (varsayılan: 5) |
| `categoryId` | int | Evet | Kategori ID |
| `locationId` | int | Hayır | Lokasyon ID |
| `image` | file | Hayır | Ürün resmi (JPEG, PNG, WebP) |

**Örnek İstek (cURL):**

```bash
curl -X POST http://localhost:5132/api/products \
  -F "name=Gaming Laptop" \
  -F "description=Yüksek performanslı gaming laptop" \
  -F "stockQuantity=15" \
  -F "lowStockThreshold=5" \
  -F "categoryId=1" \
  -F "locationId=1" \
  -F "image=@/path/to/image.jpg"
```

**Örnek Response:**

```json
{
  "productId": 1,
  "message": "Product created successfully"
}
```

### Ürün Güncelle

**Endpoint:** `PUT /api/products`

**Content-Type:** `multipart/form-data`

**Form Data:**

| Alan | Tip | Gerekli | Açıklama |
|------|-----|---------|----------|
| `id` | int | Evet | Ürün ID |
| `name` | string | Hayır | Ürün adı |
| `description` | string | Hayır | Ürün açıklaması |
| `stockQuantity` | int | Hayır | Stok miktarı |
| `lowStockThreshold` | int | Hayır | Düşük stok eşiği |
| `locationId` | int | Hayır | Lokasyon ID (boş string gönderilirse kaldırılır) |
| `image` | file | Hayır | Yeni ürün resmi |

**Not:** Partial update desteklenir. Sadece değiştirmek istediğiniz alanları gönderebilirsiniz. Eski resim otomatik olarak silinir.

**Örnek İstek:**

```bash
curl -X PUT http://localhost:5132/api/products \
  -F "id=1" \
  -F "stockQuantity=20" \
  -F "image=@/path/to/new-image.jpg"
```

### Ürün Sil

**Endpoint:** `DELETE /api/products`

**Query Parameters:**

| Parametre | Tip | Gerekli | Açıklama |
|-----------|-----|---------|----------|
| `id` | int | Evet | Ürün ID |

**Örnek İstek:**

```http
DELETE /api/products?id=1
```

**Örnek Response:**

```json
{
  "productId": 1,
  "message": "Product deleted successfully"
}
```

**Not:** Ürün silindiğinde ilişkili resim ve öznitelikler de silinir.

### Ürünleri Excel'e Aktar

**Endpoint:** `GET /api/products/export/excel`

**Örnek İstek:**

```http
GET /api/products/export/excel
```

**Response:** Excel dosyası (.xlsx) indirilir.

**Dosya Adı Formatı:** `Urunler_YYYYMMDD_HHMMSS.xlsx`

---

## Product Attributes API

### Öznitelikleri Listele

**Endpoint:** `GET /api/product-attributes`

**Query Parameters:**

| Parametre | Tip | Gerekli | Varsayılan | Açıklama |
|-----------|-----|---------|------------|----------|
| `pageNumber` | int | Hayır | 1 | Sayfa numarası |
| `pageSize` | int | Hayır | 10 | Sayfa başına kayıt sayısı |
| `productId` | int | Hayır | - | Ürün ID ile filtrele |
| `searchKey` | string | Hayır | - | Anahtar kelimesinde arama |

**Örnek İstek:**

```http
GET /api/product-attributes?pageNumber=1&pageSize=10&productId=1
```

**Örnek Response:**

```json
{
  "items": [
    {
      "id": 1,
      "productId": 1,
      "productName": "Gaming Laptop",
      "key": "RAM",
      "value": "16GB"
    },
    {
      "id": 2,
      "productId": 1,
      "productName": "Gaming Laptop",
      "key": "Ekran Boyutu",
      "value": "15.6 inç"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 2,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### Öznitelik Detayı

**Endpoint:** `GET /api/product-attributes/by-id`

**Query Parameters:**

| Parametre | Tip | Gerekli | Açıklama |
|-----------|-----|---------|----------|
| `id` | int | Evet | Öznitelik ID |

### Yeni Öznitelik Oluştur

**Endpoint:** `POST /api/product-attributes`

**Request Body:**

```json
{
  "productId": 1,
  "key": "İşlemci",
  "value": "Intel Core i7"
}
```

**Örnek Response:**

```json
{
  "attributeId": 1,
  "message": "Product attribute created successfully"
}
```

### Öznitelik Güncelle

**Endpoint:** `PUT /api/product-attributes`

**Request Body:**

```json
{
  "id": 1,
  "key": "RAM",
  "value": "32GB"
}
```

### Öznitelik Sil

**Endpoint:** `DELETE /api/product-attributes`

**Query Parameters:**

| Parametre | Tip | Gerekli | Açıklama |
|-----------|-----|---------|----------|
| `id` | int | Evet | Öznitelik ID |

### Öznitelikleri Excel'e Aktar

**Endpoint:** `GET /api/product-attributes/export/excel`

**Örnek İstek:**

```http
GET /api/product-attributes/export/excel
```

**Response:** Excel dosyası (.xlsx) indirilir.

**Dosya Adı Formatı:** `Urun_Oznitelikleri_YYYYMMDD_HHMMSS.xlsx`

---

## Stock Movements API

### Stok Hareketlerini Listele

**Endpoint:** `GET /api/stock-movements`

**Query Parameters:**

| Parametre | Tip | Gerekli | Varsayılan | Açıklama |
|-----------|-----|---------|------------|----------|
| `pageNumber` | int | Hayır | 1 | Sayfa numarası |
| `pageSize` | int | Hayır | 10 | Sayfa başına kayıt sayısı |
| `productId` | int | Hayır | - | Ürün ID ile filtrele |
| `categoryId` | int | Hayır | - | Kategori ID ile filtrele |
| `type` | StockMovementType | Hayır | - | Hareket tipi (1=In, 2=Out) |

**StockMovementType Enum:**
- `1` - In (Giriş)
- `2` - Out (Çıkış)

**Örnek İstek:**

```http
GET /api/stock-movements?pageNumber=1&pageSize=10&productId=1&type=1
```

**Örnek Response:**

```json
{
  "items": [
    {
      "id": 1,
      "productId": 1,
      "productName": "Gaming Laptop",
      "categoryId": 1,
      "categoryName": "Elektronik",
      "type": 1,
      "typeName": "Giriş",
      "quantity": 10,
      "description": "Yeni stok girişi",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 50,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Yeni Stok Hareketi Oluştur

**Endpoint:** `POST /api/stock-movements`

**Request Body:**

```json
{
  "productId": 1,
  "categoryId": 1,
  "type": 1,
  "quantity": 10,
  "description": "Yeni stok girişi"
}
```

**Örnek Response:**

```json
{
  "stockMovementId": 1,
  "message": "Stock movement created successfully",
  "newStockQuantity": 25
}
```

**Not:** Stok hareketi oluşturulduğunda ürünün stok miktarı otomatik olarak güncellenir:
- `type=1` (In): Stok miktarı artar
- `type=2` (Out): Stok miktarı azalır (eğer yeterli stok yoksa hata döner)

---

## Todos API

### Yapılacakları Listele

**Endpoint:** `GET /api/todos`

**Query Parameters:**

| Parametre | Tip | Gerekli | Varsayılan | Açıklama |
|-----------|-----|---------|------------|----------|
| `pageNumber` | int | Hayır | 1 | Sayfa numarası |
| `pageSize` | int | Hayır | 10 | Sayfa başına kayıt sayısı |
| `status` | TodoStatus | Hayır | - | Durum filtresi |
| `priority` | TodoPriority | Hayır | - | Öncelik filtresi |

**TodoStatus Enum:**
- `1` - Todo (Yapılacak)
- `2` - InProgress (Devam Ediyor)
- `3` - Completed (Tamamlandı)

**TodoPriority Enum:**
- `1` - Low (Düşük)
- `2` - Medium (Orta)
- `3` - High (Yüksek)

**Örnek İstek:**

```http
GET /api/todos?pageNumber=1&pageSize=10&status=1&priority=3
```

**Örnek Response:**

```json
{
  "items": [
    {
      "id": 1,
      "title": "Stok kontrolü yap",
      "description": "Tüm ürünlerin stok durumunu kontrol et",
      "status": 1,
      "statusName": "Yapılacak",
      "priority": 3,
      "priorityName": "Yüksek",
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": null
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 15,
  "totalPages": 2,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Yeni Yapılacak Oluştur

**Endpoint:** `POST /api/todos`

**Request Body:**

```json
{
  "title": "Stok kontrolü yap",
  "description": "Tüm ürünlerin stok durumunu kontrol et",
  "status": 1,
  "priority": 3
}
```

**Örnek Response:**

```json
{
  "todoId": 1,
  "message": "Todo created successfully"
}
```

### Yapılacak Güncelle

**Endpoint:** `PUT /api/todos/{id}`

**Path Parameters:**

| Parametre | Tip | Gerekli | Açıklama |
|-----------|-----|---------|----------|
| `id` | int | Evet | Todo ID |

**Request Body:**

```json
{
  "title": "Stok kontrolü yap - Güncellendi",
  "status": 2,
  "priority": 2
}
```

**Örnek Response:**

```json
{
  "todoId": 1,
  "message": "Todo updated successfully"
}
```

### Yapılacak Sil

**Endpoint:** `DELETE /api/todos/{id}`

**Path Parameters:**

| Parametre | Tip | Gerekli | Açıklama |
|-----------|-----|---------|----------|
| `id` | int | Evet | Todo ID |

**Örnek İstek:**

```http
DELETE /api/todos/1
```

**Örnek Response:**

```json
{
  "todoId": 1,
  "message": "Todo deleted successfully"
}
```

---

## Dashboard API

### Dashboard İstatistikleri

**Endpoint:** `GET /api/dashboard/stats`

**Örnek İstek:**

```http
GET /api/dashboard/stats
```

**Örnek Response:**

```json
{
  "totalProducts": 150,
  "totalCategories": 10,
  "totalLocations": 5,
  "lowStockProducts": 8,
  "totalStockValue": 250000.50,
  "recentMovements": [
    {
      "id": 1,
      "productName": "Gaming Laptop",
      "type": 1,
      "quantity": 10,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "categoryDistribution": [
    {
      "categoryName": "Elektronik",
      "productCount": 50
    },
    {
      "categoryName": "Giyim",
      "productCount": 30
    }
  ]
}
```

**Not:** Bu endpoint SignalR üzerinden tüm bağlı client'lara real-time olarak broadcast edilir.

---

## SignalR Hub

### Hub Endpoint

```
/hubs/stock
```

### Client Bağlantısı

**JavaScript/TypeScript:**

```typescript
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
  .withUrl('http://localhost:5132/hubs/stock')
  .build();

await connection.start();
```

### Event'ler

#### DashboardStatsUpdated

Dashboard istatistikleri güncellendiğinde tetiklenir.

```typescript
connection.on('DashboardStatsUpdated', (stats) => {
  console.log('Dashboard stats updated:', stats);
  // Stats'ı kullanarak UI'ı güncelle
});
```

**Payload:**

```json
{
  "totalProducts": 150,
  "totalCategories": 10,
  "totalLocations": 5,
  "lowStockProducts": 8,
  "totalStockValue": 250000.50,
  ...
}
```

---

## Örnek Kullanım Senaryoları

### Senaryo 1: Yeni Ürün Ekleme

1. **Kategori oluştur** (eğer yoksa):
```http
POST /api/categories
{
  "name": "Elektronik"
}
```

2. **Lokasyon oluştur** (eğer yoksa):
```http
POST /api/locations
{
  "name": "Depo A",
  "description": "Ana depo"
}
```

3. **Ürün oluştur**:
```http
POST /api/products (multipart/form-data)
name: Gaming Laptop
description: Yüksek performanslı gaming laptop
stockQuantity: 15
lowStockThreshold: 5
categoryId: 1
locationId: 1
image: [file]
```

4. **Ürün öznitelikleri ekle**:
```http
POST /api/product-attributes
{
  "productId": 1,
  "key": "RAM",
  "value": "16GB"
}
```

### Senaryo 2: Stok Girişi

```http
POST /api/stock-movements
{
  "productId": 1,
  "categoryId": 1,
  "type": 1,
  "quantity": 10,
  "description": "Yeni stok girişi"
}
```

Stok miktarı otomatik olarak güncellenir.

### Senaryo 3: Düşük Stok Kontrolü

```http
GET /api/products?pageNumber=1&pageSize=100
```

Response'daki `items` array'ini filtreleyerek `stockQuantity <= lowStockThreshold` olan ürünleri bulun.

---

## Rate Limiting

Şu anda rate limiting yoktur. Production ortamında eklenmesi önerilir.

## Güvenlik Notları

- Production ortamında HTTPS kullanın
- Authentication ve Authorization ekleyin
- Input validation'ı güçlendirin
- SQL injection koruması için EF Core kullanılıyor (parametreli sorgular)
- XSS koruması için frontend'de input sanitization yapın

## Sorun Giderme

### CORS Hatası

Backend'in CORS policy'sinde frontend URL'iniz tanımlı olmalı. `Program.cs` dosyasını kontrol edin.

### Resim Yükleme Hatası

- Dosya boyutu limitini kontrol edin
- Desteklenen formatlar: JPEG, PNG, WebP
- `wwwroot/images` klasörünün yazılabilir olduğundan emin olun

### Pagination Hatası

- `pageNumber` ve `pageSize` pozitif sayılar olmalı
- `pageSize` çok büyük değerler performans sorunlarına yol açabilir (max 100 önerilir)

---

**Son Güncelleme:** 2024-01-01

