# Partial Update (Kısmi Güncelleme) Rehberi

## Genel Bakış

Tüm UPDATE endpoint'leri **partial update** (kısmi güncelleme) destekler. Bu sayede sadece değiştirmek istediğiniz alanları göndermeniz yeterlidir. Gönderilmeyen alanlar mevcut değerlerini korur.

## Nasıl Çalışır?

Update command'larındaki tüm alanlar **nullable** (null kabul eder) olarak tanımlanmıştır. Handler'lar sadece `null` olmayan alanları günceller.

### Mantık
```csharp
// Sadece gönderilen (null olmayan) alanları güncelle
if (request.Name != null)
{
    product.Name = request.Name;
}

if (request.StockQuantity.HasValue)
{
    product.StockQuantity = request.StockQuantity.Value;
}
```

## Kullanım Örnekleri

### Product Update

#### Senaryo 1: Sadece Stok Miktarını Güncelleme
```http
PUT http://localhost:5132/api/products/1
Content-Type: application/json

{
  "id": 1,
  "stockQuantity": 50
}
```
**Sonuç:** Sadece `StockQuantity` güncellenir, `Name` ve `Description` değişmez.

#### Senaryo 2: Sadece İsmi Güncelleme
```http
PUT http://localhost:5132/api/products/1
Content-Type: application/json

{
  "id": 1,
  "name": "Yeni Laptop"
}
```
**Sonuç:** Sadece `Name` güncellenir, `Description` ve `StockQuantity` değişmez.

#### Senaryo 3: Birden Fazla Alanı Güncelleme
```http
PUT http://localhost:5132/api/products/1
Content-Type: application/json

{
  "id": 1,
  "name": "Yeni Laptop",
  "stockQuantity": 75
}
```
**Sonuç:** `Name` ve `StockQuantity` güncellenir, `Description` değişmez.

#### Senaryo 4: Tüm Alanları Güncelleme
```http
PUT http://localhost:5132/api/products/1
Content-Type: application/json

{
  "id": 1,
  "name": "Yeni Laptop",
  "description": "Güncellenmiş açıklama",
  "stockQuantity": 100
}
```
**Sonuç:** Tüm alanlar güncellenir.

### Category Update

#### Kısmi Güncelleme
```http
PUT http://localhost:5132/api/categories/1
Content-Type: application/json

{
  "categoryId": 1,
  "name": "Güncellenmiş Kategori"
}
```

### Product Attribute Update

#### Sadece Value Güncelleme
```http
PUT http://localhost:5132/api/product-attributes/1
Content-Type: application/json

{
  "id": 1,
  "value": "32GB DDR5"
}
```
**Sonuç:** Sadece `Value` güncellenir, `Key` değişmez.

#### Sadece Key Güncelleme
```http
PUT http://localhost:5132/api/product-attributes/1
Content-Type: application/json

{
  "id": 1,
  "key": "Memory"
}
```
**Sonuç:** Sadece `Key` güncellenir, `Value` değişmez.

## Command Yapısı

### UpdateProductCommand
```csharp
public record UpdateProductCommand : IRequest<Unit>
{
    public int Id { get; init; }              // Required
    public string? Name { get; init; }        // Optional (nullable)
    public string? Description { get; init; } // Optional (nullable)
    public int? StockQuantity { get; init; }  // Optional (nullable)
}
```

### UpdateCategoryCommand
```csharp
public record UpdateCategoryCommand : IRequest<Unit>
{
    public int CategoryId { get; init; }  // Required
    public string? Name { get; init; }    // Optional (nullable)
}
```

### UpdateProductAttributeCommand
```csharp
public record UpdateProductAttributeCommand : IRequest<Unit>
{
    public int Id { get; init; }       // Required
    public string? Key { get; init; }  // Optional (nullable)
    public string? Value { get; init; } // Optional (nullable)
}
```

## Swagger'da Kullanım

Swagger UI'da update endpoint'lerini test ederken:

1. Endpoint'i seçin (örn: `PUT /api/products/{id}`)
2. "Try it out" butonuna tıklayın
3. Request body'de **sadece güncellemek istediğiniz alanları** bırakın
4. Diğer alanları silin veya boş bırakın
5. "Execute" butonuna tıklayın

### Örnek Request Body (Swagger)
```json
{
  "id": 1,
  "stockQuantity": 50
}
```

**Not:** Swagger UI bazı alanları otomatik olarak doldurabilir. İstemediğiniz alanları manuel olarak silmeniz gerekebilir.

## Best Practices

### ✅ Yapılması Gerekenler

1. **Sadece Değişen Alanları Gönderin**
   ```json
   {
     "id": 1,
     "stockQuantity": 50
   }
   ```

2. **Boş String Yerine Null Kullanın**
   ```json
   {
     "id": 1,
     "name": null  // Bu alan güncellenmeyecek
   }
   ```

3. **ID Alanını Her Zaman Gönderin**
   ```json
   {
     "id": 1,  // Required
     "name": "Yeni İsim"
   }
   ```

### ❌ Yapılmaması Gerekenler

1. **Boş String ile Güncelleme Yapmayın**
   ```json
   {
     "id": 1,
     "name": ""  // Bu, name'i boş string yapar (istemiyorsanız göndermeyin)
   }
   ```

2. **0 ile Stok Sıfırlama (İstemeden)**
   ```json
   {
     "id": 1,
     "stockQuantity": 0  // Bu, stoğu 0 yapar (istemiyorsanız göndermeyin)
   }
   ```

3. **Tüm Alanları Varsayılan Değerlerle Gönderme**
   ```json
   {
     "id": 1,
     "name": "",
     "description": "",
     "stockQuantity": 0
   }
   ```

## Özel Durumlar

### Boş String vs Null

- **Null gönderirseniz:** Alan güncellenmez (mevcut değeri korur)
- **Boş string gönderirseniz:** Alan boş string olarak güncellenir

```json
// Null - Alan güncellenmez
{
  "id": 1,
  "name": null
}

// Boş string - Alan boş string olarak güncellenir
{
  "id": 1,
  "name": ""
}
```

### Sayısal Değerler (int?)

- **Null gönderirseniz:** Alan güncellenmez
- **0 gönderirseniz:** Alan 0 olarak güncellenir

```json
// Null - Alan güncellenmez
{
  "id": 1,
  "stockQuantity": null
}

// Sıfır - Alan 0 olarak güncellenir
{
  "id": 1,
  "stockQuantity": 0
}
```

## Validation

Güncelleme işlemlerinde bazı validasyonlar yapılabilir:

### İleride Eklenebilecek Validasyonlar

```csharp
// İsim boş olamaz
if (request.Name == string.Empty)
{
    throw new ValidationException("Name cannot be empty");
}

// Stok negatif olamaz
if (request.StockQuantity < 0)
{
    throw new ValidationException("Stock quantity cannot be negative");
}
```

## Test Senaryoları

### Test 1: Partial Update
```http
# 1. Ürün oluştur
POST http://localhost:5132/api/products
{
  "name": "Laptop",
  "description": "Gaming laptop",
  "stockQuantity": 10,
  "categoryId": 1
}

# 2. Sadece stoğu güncelle
PUT http://localhost:5132/api/products/1
{
  "id": 1,
  "stockQuantity": 50
}

# 3. Ürünü kontrol et - Name ve Description değişmemiş olmalı
GET http://localhost:5132/api/products/1
```

### Test 2: Birden Fazla Alan Güncelleme
```http
PUT http://localhost:5132/api/products/1
{
  "id": 1,
  "name": "Gaming Laptop Pro",
  "stockQuantity": 75
}
# Description değişmemeli
```

### Test 3: Boş String Testi
```http
PUT http://localhost:5132/api/products/1
{
  "id": 1,
  "description": ""
}
# Description boş string olmalı
```

## JavaScript/Frontend Entegrasyonu

### Fetch API ile Partial Update
```javascript
async function updateProduct(id, updates) {
    const response = await fetch(`/api/products/${id}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            id,
            ...updates  // Sadece değişen alanlar
        })
    });
    
    if (response.ok) {
        console.log('Product updated successfully');
    }
}

// Kullanım
updateProduct(1, { stockQuantity: 50 });
updateProduct(1, { name: "New Name", stockQuantity: 75 });
```

### React Hook Örneği
```jsx
const updateProduct = async (id, partialUpdate) => {
    try {
        const response = await fetch(`/api/products/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ id, ...partialUpdate })
        });
        
        if (response.ok) {
            toast.success('Product updated!');
            refetch();
        }
    } catch (error) {
        toast.error('Update failed');
    }
};

// Component içinde
<button onClick={() => updateProduct(1, { stockQuantity: stock + 1 })}>
    Increase Stock
</button>
```

## Avantajları

1. **Bandwidth Tasarrufu**: Sadece değişen alanlar gönderilir
2. **Hata Azaltma**: Yanlışlıkla diğer alanların üzerine yazılmaz
3. **Esneklik**: İstemci sadece bildiği alanları güncelleyebilir
4. **Optimistic UI**: Frontend'de sadece değişen alanları güncelleyebilir
5. **API Uyumluluğu**: REST best practices'e uygun

## Troubleshooting

### Problem: Alanlar Güncellenmiyor
**Çözüm:** JSON'da alanın `null` değil, gerçek bir değer olduğundan emin olun.

### Problem: İstemeden Alanlar Sıfırlanıyor
**Çözüm:** Swagger veya istemci kodunuzun varsayılan değerler göndermediğinden emin olun.

### Problem: Boş String ile Alan Temizlenmiyor
**Çözüm:** Bu kasıtlı bir davranış. Boş string göndermek alanı boş yapar, `null` göndermek ise alanı değiştirmez.

