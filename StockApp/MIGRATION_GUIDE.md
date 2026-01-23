# 🔄 Identity Migration Rehberi

## Migration Oluşturma

Terminal'de şu komutları çalıştırın:

```bash
cd StockApp
dotnet ef migrations add AddIdentity
dotnet ef database update
```

## Migration Sonrası Kontroller

### 1. Veritabanını Kontrol Edin

SQLite veritabanında şu tablolar oluşmuş olmalı:

- `AspNetUsers` - Kullanıcılar
- `AspNetRoles` - Roller
- `AspNetUserRoles` - Kullanıcı-Rol ilişkileri
- `AspNetUserClaims` - Kullanıcı claim'leri
- `AspNetRoleClaims` - Rol claim'leri
- `AspNetUserLogins` - External login'ler
- `AspNetUserTokens` - User tokens
- `__EFMigrationsHistory` - Migration geçmişi

### 2. Seed Verilerini Kontrol Edin

Backend'i çalıştırdığınızda otomatik olarak:

- ✅ 3 rol oluşturulur: Admin, Manager, User
- ✅ Admin kullanıcısı oluşturulur:
  - Email: `admin@stockapp.com`
  - Username: `admin`
  - Password: `Admin123!`
  - Role: `Admin`

### 3. Test Edin

```bash
# Backend'i çalıştırın
dotnet run

# Swagger UI'da test edin
# http://localhost:5134/
```

Swagger'da `/api/auth/login` endpoint'ini test edin:

```json
{
  "email": "admin@stockapp.com",
  "password": "Admin123!"
}
```

## Sorun Giderme

### Migration Hatası Alırsanız

Eğer migration sırasında hata alırsanız:

```bash
# Tüm migration'ları geri al
dotnet ef database drop

# Migration'ları yeniden uygula
dotnet ef database update
```

### Veritabanı Lock Hatası

SQLite veritabanı kilitliyse:

```bash
# Backend'i durdurun
# Veritabanı dosyasını kontrol edin
ls -la stockapp.db

# Gerekirse veritabanını silip yeniden oluşturun
rm stockapp.db
dotnet ef database update
```

### Build Hatası

Eğer build hatası alırsanız:

```bash
# Paketleri restore edin
dotnet restore

# Temiz build yapın
dotnet clean
dotnet build
```

## Migration Dosyası İçeriği

Migration dosyası şunları içerir:

1. Identity tablolarının oluşturulması
2. ApplicationUser için ek kolonlar (FirstName, LastName, IsActive, RefreshToken, vb.)
3. Index'ler ve foreign key'ler
4. Unique constraint'ler

## Sonraki Adımlar

Migration başarılı olduktan sonra:

1. ✅ Backend'i çalıştırıp test edin
2. ✅ Frontend'de auth servisleri oluşturun
3. ✅ Login/Register sayfaları ekleyin
4. ✅ Token yönetimi implementasyonu







