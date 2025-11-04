# MediatR Kullanım Rehberi

## MediatR Nedir?

MediatR, CQRS (Command Query Responsibility Segregation) pattern'ini implement etmek için kullanılan bir kütüphanedir. Request/Response ve notification pattern'lerini destekler.

## Proje Yapısı

```
StockApp/
├── App/
│   ├── Category/
│   │   ├── Command/
│   │   │   ├── CreateCategoryCommand.cs
│   │   │   ├── UpdateCategoryCommand.cs
│   │   │   └── DeleteCategoryCommand.cs
│   │   └── Query/
│   │       ├── GetCategoriesQuery.cs
│   │       └── GetCategoryByIdQuery.cs
│   ├── Product/
│   │   ├── Command/
│   │   │   ├── CreateProductCommand.cs
│   │   │   ├── UpdateProductCommand.cs
│   │   │   └── DeleteProductCommand.cs
│   │   └── Query/
│   │       ├── GetProductsQuery.cs
│   │       └── GetProductByIdQuery.cs
│   └── ProductAttribute/
│       ├── Command/
│       │   ├── CreateProductAttributeCommand.cs
│       │   ├── UpdateProductAttributeCommand.cs
│       │   └── DeleteProductAttributeCommand.cs
│       └── Query/
│           ├── GetProductAttributesQuery.cs
│           └── GetProductAttributeByIdQuery.cs
├── Common/
│   ├── Extensions/
│   │   └── QueryableExtensions.cs
│   └── Models/
│       ├── PaginatedList.cs
│       └── PaginationQuery.cs
```

## API Endpoints

### Categories

#### GET /api/categories
Tüm kategorileri paginated olarak listeler.

**Query Parameters:**
- `pageNumber` (optional, default: 1): Sayfa numarası
- `pageSize` (optional, default: 10): Sayfa başına kayıt sayısı (max: 100)

**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "name": "Elektronik",
      "productCount": 5
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

#### GET /api/categories/{id}
Belirli bir kategorinin detaylarını getirir.

**Response:**
```json
{
  "id": 1,
  "name": "Elektronik",
  "products": [
    {
      "id": 1,
      "name": "Laptop",
      "stockQuantity": 10
    }
  ]
}
```

#### POST /api/categories
Yeni kategori oluşturur.

**Request:**
```json
{
  "name": "Elektronik"
}
```

**Response:**
```json
{
  "id": 1
}
```

#### PUT /api/categories/{id}
Kategori günceller (Partial Update destekler).

**Not:** Sadece göndermek istediğiniz alanları ekleyin. Gönderilmeyen alanlar değişmez.

**Request:**
```json
{
  "categoryId": 1,
  "name": "Updated Elektronik"
}
```

**Response:**
```json
{
  "message": "Category updated successfully."
}
```

#### DELETE /api/categories/{id}
Kategori siler.

**Response:** 204 No Content

### Products

#### GET /api/products
Tüm ürünleri paginated olarak listeler. Filtreleme ve arama destekler.

**Query Parameters:**
- `pageNumber` (optional, default: 1): Sayfa numarası
- `pageSize` (optional, default: 10): Sayfa başına kayıt sayısı (max: 100)
- `categoryId` (optional): Kategoriye göre filtrele
- `searchTerm` (optional): Ürün adı veya açıklamasında arama yap

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

#### GET /api/products/{id}
Belirli bir ürünün detaylarını getirir.

**Response:**
```json
{
  "id": 1,
  "name": "Laptop",
  "description": "15 inch laptop",
  "stockQuantity": 10,
  "categoryId": 1,
  "categoryName": "Elektronik",
  "attributes": [
    {
      "id": 1,
      "key": "RAM",
      "value": "16GB"
    }
  ]
}
```

#### POST /api/products
Yeni ürün oluşturur.

**Request:**
```json
{
  "name": "Laptop",
  "description": "15 inch laptop",
  "stockQuantity": 10,
  "categoryId": 1
}
```

**Response:**
```json
{
  "id": 1
}
```

#### PUT /api/products/{id}
Mevcut ürünü günceller (Partial Update destekler).

**Not:** Sadece göndermek istediğiniz alanları ekleyin. Gönderilmeyen alanlar değişmez.

**Örnek 1 - Sadece Stok Güncelleme:**
```json
{
  "id": 1,
  "stockQuantity": 50
}
```

**Örnek 2 - Tüm Alanları Güncelleme:**
```json
{
  "id": 1,
  "name": "Updated Laptop",
  "description": "Updated description",
  "stockQuantity": 15
}
```

**Response:**
```json
{
  "message": "Product updated successfully."
}
```

#### DELETE /api/products/{id}
Ürünü siler.

**Response:** 204 No Content

### Product Attributes

#### GET /api/product-attributes
Tüm ürün özelliklerini paginated olarak listeler. Filtreleme ve arama destekler.

**Query Parameters:**
- `pageNumber` (optional, default: 1): Sayfa numarası
- `pageSize` (optional, default: 10): Sayfa başına kayıt sayısı (max: 100)
- `productId` (optional): Ürüne göre filtrele
- `searchKey` (optional): Key alanında arama yap

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

#### GET /api/product-attributes/{id}
Belirli bir ürün özelliğinin detaylarını getirir.

**Response:**
```json
{
  "id": 1,
  "productId": 1,
  "productName": "Laptop",
  "key": "RAM",
  "value": "16GB"
}
```

#### POST /api/product-attributes
Yeni ürün özelliği oluşturur.

**Request:**
```json
{
  "productId": 1,
  "key": "RAM",
  "value": "16GB"
}
```

**Response:**
```json
{
  "id": 1
}
```

#### PUT /api/product-attributes/{id}
Ürün özelliğini günceller (Partial Update destekler).

**Not:** Sadece göndermek istediğiniz alanları ekleyin. Gönderilmeyen alanlar değişmez.

**Örnek 1 - Sadece Value Güncelleme:**
```json
{
  "id": 1,
  "value": "32GB"
}
```

**Örnek 2 - Her İki Alanı Güncelleme:**
```json
{
  "id": 1,
  "key": "Memory",
  "value": "32GB DDR5"
}
```

**Response:**
```json
{
  "message": "ProductAttribute updated successfully."
}
```

#### DELETE /api/product-attributes/{id}
Ürün özelliğini siler.

**Response:** 204 No Content

## Yeni Command/Query Ekleme

### 1. Command Oluşturma

```csharp
// CreateProductCommand.cs
public record CreateProductCommand : IRequest<CreateProductResponse>
{
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
}

public record CreateProductResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
```

### 2. Handler Oluşturma

```csharp
// CreateProductCommandHandler.cs
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    private readonly ApplicationDbContext _context;

    public CreateProductCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateProductResponse
        {
            Id = product.Id,
            Name = product.Name
        };
    }
}
```

### 3. Endpoint Oluşturma

```csharp
// Program.cs
app.MapPost("/api/products", async (IMediator mediator, CreateProductCommand command) =>
{
    var response = await mediator.Send(command);
    return Results.Created($"/api/products/{response.Id}", response);
})
.WithName("CreateProduct")
.WithTags("Products");
```

## MediatR Avantajları

1. **Separation of Concerns**: Business logic'i endpoint'lerden ayırır
2. **Testability**: Handler'lar kolayca unit test edilebilir
3. **Single Responsibility**: Her handler tek bir işten sorumlu
4. **Clean Code**: Kod daha okunabilir ve maintainable olur
5. **Pipeline Behaviors**: Validation, logging gibi cross-cutting concerns kolayca eklenebilir

## Çalıştırma

```bash
cd StockApp
dotnet run
```

API'ye şu adresten erişebilirsiniz: `https://localhost:5001` veya `http://localhost:5000`

OpenAPI dokümantasyonu: `https://localhost:5001/openapi/v1.json`

