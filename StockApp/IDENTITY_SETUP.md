# 🔐 .NET Identity ve JWT Authentication Kurulumu

Bu dokümantasyon, projeye eklenen .NET Identity ve JWT Authentication sistemini açıklar.

## ✅ Yapılan Değişiklikler

### 1. Paketler
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (v9.0.0)
- `Microsoft.AspNetCore.Authentication.JwtBearer` (v9.0.0)
- `System.IdentityModel.Tokens.Jwt` (v8.2.1)

### 2. Entity'ler
- `ApplicationUser` - IdentityUser'dan türetilen kullanıcı entity'si
  - FirstName, LastName
  - IsActive
  - RefreshToken, RefreshTokenExpiryTime

### 3. Servisler
- `IJwtTokenService` / `JwtTokenService` - JWT token oluşturma ve yönetimi
- Identity servisleri (UserManager, RoleManager, SignInManager)

### 4. Roller
- **Admin** - Tüm yetkilere sahip
- **Manager** - Yönetim yetkileri
- **User** - Temel kullanıcı yetkileri

### 5. API Endpoint'leri

#### Public Endpoints
- `POST /api/auth/login` - Kullanıcı girişi
- `POST /api/auth/register` - Kullanıcı kaydı
- `POST /api/auth/refresh-token` - Token yenileme

#### Protected Endpoints
- `GET /api/auth/users` - Tüm kullanıcıları listele (AdminOnly)
- `GET /api/auth/me` - Mevcut kullanıcı bilgileri (Authenticated)

### 6. Authorization Policies
- `AdminOnly` - Sadece Admin rolü
- `ManagerOrAdmin` - Manager veya Admin rolü
- `UserOrAbove` - User, Manager veya Admin rolü

## 🚀 Migration Oluşturma

Identity tablolarını veritabanına eklemek için migration oluşturun:

```bash
cd StockApp
dotnet ef migrations add AddIdentity
dotnet ef database update
```

## 🔑 Varsayılan Kullanıcı

Seed işlemi sırasında otomatik olarak oluşturulan admin kullanıcısı:

- **Email**: admin@stockapp.com
- **Username**: admin
- **Password**: Admin123!
- **Role**: Admin

## 📝 JWT Ayarları

`appsettings.json` içinde JWT ayarları:

```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration-MustBeAtLeast32CharactersLong!",
    "Issuer": "StockApp",
    "Audience": "StockAppUsers",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

**Önemli**: Production ortamında `SecretKey`'i environment variable olarak saklayın!

## 🔒 Endpoint Korumaları

Endpoint'lere authorization eklemek için:

```csharp
group.MapPost("/create", Create)
    .RequireAuthorization("AdminOnly"); // veya "ManagerOrAdmin", "UserOrAbove"
```

## 📊 Request/Response Örnekleri

### Login Request
```json
{
  "email": "admin@stockapp.com",
  "password": "Admin123!"
}
```

### Login Response
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64encodedrefreshtoken...",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": {
    "id": "user-id",
    "email": "admin@stockapp.com",
    "userName": "admin",
    "firstName": "Admin",
    "lastName": "User",
    "roles": ["Admin"]
  }
}
```

### Register Request
```json
{
  "email": "user@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "firstName": "John",
  "lastName": "Doe",
  "userName": "johndoe"
}
```

## 🛡️ Güvenlik Notları

1. **Production'da**:
   - `RequireHttpsMetadata = true` yapın
   - `SecretKey`'i environment variable'dan okuyun
   - `RequireConfirmedEmail = true` yapın
   - Rate limiting ekleyin

2. **Password Policy**:
   - Minimum 6 karakter
   - Büyük harf, küçük harf, rakam gereklidir
   - Özel karakter opsiyonel

3. **Token Security**:
   - Access token 60 dakika geçerli
   - Refresh token 7 gün geçerli
   - Token'lar HttpOnly cookie'de saklanabilir (frontend'de)

## 🔄 Swagger'da Test Etme

1. Swagger UI'da `/api/auth/login` endpoint'ini çağırın
2. Response'dan `accessToken`'ı kopyalayın
3. Swagger UI'ın sağ üst köşesindeki "Authorize" butonuna tıklayın
4. `Bearer {accessToken}` formatında token'ı girin
5. Artık korumalı endpoint'leri test edebilirsiniz

## 📚 Sonraki Adımlar

1. ✅ Migration oluştur ve uygula
2. ✅ Frontend'de login/register sayfaları oluştur
3. ✅ Token storage (localStorage veya httpOnly cookie)
4. ✅ Token refresh mekanizması
5. ✅ Mevcut endpoint'lere authorization ekle
6. ✅ Role-based access control (RBAC) uygula

