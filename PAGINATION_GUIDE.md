# Pagination Kullanım Rehberi

## Genel Bakış

Tüm liste endpoint'lerinde pagination desteği eklenmiştir. Bu, büyük veri setlerini yönetmeyi ve performansı iyileştirmeyi sağlar.

## Pagination Parametreleri

Tüm liste endpoint'leri aşağıdaki query parametrelerini destekler:

- **pageNumber** (optional, default: 1): İstenen sayfa numarası
- **pageSize** (optional, default: 10): Sayfa başına döndürülecek kayıt sayısı
  - Minimum: 1
  - Maximum: 100 (otomatik sınırlanır)

## Pagination Response Formatı

Tüm paginated endpoint'ler aşağıdaki formatta response döner:

```json
{
  "items": [...],           // Sayfa içeriği
  "pageNumber": 1,          // Mevcut sayfa numarası
  "pageSize": 10,           // Sayfa başına kayıt sayısı
  "totalCount": 125,        // Toplam kayıt sayısı
  "totalPages": 13,         // Toplam sayfa sayısı
  "hasPreviousPage": false, // Önceki sayfa var mı?
  "hasNextPage": true       // Sonraki sayfa var mı?
}
```

## Özellikler

### 1. Otomatik Validasyon
- `pageNumber` negatif veya sıfır olamaz (minimum 1)
- `pageSize` negatif veya sıfır olamaz (minimum 1)
- `pageSize` 100'den fazla olamaz (otomatik olarak 100'e düşürülür)

### 2. Sayfalama Meta Verileri
- **totalCount**: Filtreleme sonrası toplam kayıt sayısı
- **totalPages**: Toplam sayfa sayısı (Math.Ceiling(totalCount / pageSize))
- **hasPreviousPage**: Önceki sayfaya gidilebilir mi?
- **hasNextPage**: Sonraki sayfaya gidilebilir mi?

### 3. Filtreleme ile Birlikte Kullanım
Pagination, filtreleme ve arama parametreleri ile birlikte kullanılabilir:

```http
GET /api/products?pageNumber=2&pageSize=20&categoryId=1&searchTerm=laptop
```

## Endpoint'ler

### Categories
```http
GET /api/categories?pageNumber=1&pageSize=10
```

**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "name": "Elektronik",
      "productCount": 25
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

### Products
```http
GET /api/products?pageNumber=1&pageSize=10&categoryId=1&searchTerm=laptop
```

**Filtreleme Parametreleri:**
- `categoryId`: Kategoriye göre filtrele
- `searchTerm`: Ürün adı veya açıklamasında ara

**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "name": "Laptop",
      "description": "15 inch laptop",
      "stockQuantity": 10,
      "categoryName": "Elektronik"
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

### Product Attributes
```http
GET /api/product-attributes?pageNumber=1&pageSize=10&productId=1&searchKey=RAM
```

**Filtreleme Parametreleri:**
- `productId`: Ürüne göre filtrele
- `searchKey`: Key alanında ara

**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "productId": 1,
      "productName": "Laptop",
      "key": "RAM",
      "value": "16GB"
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

## Implementasyon Detayları

### PaginatedList Sınıfı
```csharp
public class PaginatedList<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
```

### Extension Method
```csharp
public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
    this IQueryable<T> source,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken = default)
{
    var count = await source.CountAsync(cancellationToken);
    var items = await source
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

    return new PaginatedList<T>(items, count, pageNumber, pageSize);
}
```

## Best Practices

1. **Varsayılan Değerler Kullanın**
   - pageNumber: 1
   - pageSize: 10

2. **Uygun Sayfa Boyutu Seçin**
   - Mobil: 10-20
   - Web: 20-50
   - API: 50-100

3. **Filtreleme ile Birlikte Kullanın**
   - Önce filtrele, sonra paginate et
   - totalCount filtrelenmiş sonuç sayısını gösterir

4. **Meta Verileri Kullanın**
   - UI'da sayfa navigasyonu için `hasPreviousPage` ve `hasNextPage` kullanın
   - Toplam sayfa sayısını göstermek için `totalPages` kullanın

## Örnek Kullanım Senaryoları

### Senaryo 1: İlk Sayfa
```http
GET /api/products?pageNumber=1&pageSize=20
```

### Senaryo 2: Belirli Bir Sayfa
```http
GET /api/products?pageNumber=3&pageSize=20
```

### Senaryo 3: Filtreleme + Pagination
```http
GET /api/products?pageNumber=1&pageSize=20&categoryId=5&searchTerm=gaming
```

### Senaryo 4: Tüm Kategoriler (Küçük Veri Seti)
```http
GET /api/categories?pageNumber=1&pageSize=100
```

## Performance Notları

1. **Database İndeksleri**: `Id`, `CategoryId`, `ProductId` gibi alanlar için indexler mevcut
2. **Eager Loading**: İlişkili veriler için `Include()` kullanılıyor
3. **Select Projection**: Sadece gerekli alanlar seçiliyor
4. **Async Operations**: Tüm database işlemleri asenkron

## Frontend Entegrasyonu

### JavaScript Örneği
```javascript
async function getProducts(pageNumber = 1, pageSize = 10, categoryId = null) {
    const params = new URLSearchParams({
        pageNumber,
        pageSize,
        ...(categoryId && { categoryId })
    });
    
    const response = await fetch(`/api/products?${params}`);
    const data = await response.json();
    
    console.log(`Showing page ${data.pageNumber} of ${data.totalPages}`);
    console.log(`Total items: ${data.totalCount}`);
    console.log('Items:', data.items);
    
    return data;
}
```

### React Örneği
```jsx
const [page, setPage] = useState(1);
const [products, setProducts] = useState(null);

useEffect(() => {
    fetch(`/api/products?pageNumber=${page}&pageSize=20`)
        .then(res => res.json())
        .then(data => setProducts(data));
}, [page]);

return (
    <div>
        <ProductList items={products?.items} />
        <Pagination 
            current={products?.pageNumber}
            total={products?.totalPages}
            onPageChange={setPage}
            hasNext={products?.hasNextPage}
            hasPrev={products?.hasPreviousPage}
        />
    </div>
);
```

## Troubleshooting

### Problem: Boş Sayfa Dönüyor
**Çözüm**: `pageNumber` değerini kontrol edin. `totalPages`'den büyük olamaz.

### Problem: Çok Fazla Veri Dönüyor
**Çözüm**: `pageSize` parametresini düşürün veya filtreleme ekleyin.

### Problem: Performance Sorunları
**Çözüm**: 
- İndeksleri kontrol edin
- Select projections kullanın
- Gereksiz `Include()` çağrılarını kaldırın

