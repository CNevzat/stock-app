# 🔐 Gelişmiş Yetkilendirme ve Kullanıcı Yönetimi Sistemi

## ✅ Özellikler

### 1. **Claim-Based Yetkilendirme**
- Role-based yetkilendirmenin yanında claim-based sistem
- Admin, Manager veya `CanCreateUser` claim'ine sahip kullanıcılar kullanıcı oluşturabilir
- Dinamik yetki yönetimi

### 2. **Zorunlu Şifre Değiştirme**
- İlk girişte şifre değiştirme zorunluluğu (`MustChangePassword` flag)
- Yeni oluşturulan kullanıcılar default şifre ile giriş yapamaz
- Şifre değiştirilene kadar token verilmez

### 3. **Güvenli Şifre Yönetimi**
- .NET Identity otomatik olarak şifreleri hash'ler (PBKDF2)
- Şifreler veritabanında asla plain text olarak saklanmaz
- Password validation (min 6 karakter, büyük/küçük harf, rakam)

### 4. **Korumalı Kullanıcı Oluşturma**
- Public register endpoint'i kaldırıldı
- Sadece yetkili kullanıcılar (Admin/Manager veya claim sahibi) kullanıcı oluşturabilir
- İlk admin kullanıcısı seed data ile oluşturulur

## 📋 API Endpoint'leri

### Public Endpoints
- `POST /api/auth/login` - Kullanıcı girişi
- `POST /api/auth/refresh-token` - Token yenileme

### Protected Endpoints

#### Kullanıcı Yönetimi
- `POST /api/auth/users` - Yeni kullanıcı oluştur (CanCreateUser policy)
- `GET /api/auth/users` - Tüm kullanıcıları listele (AdminOnly)
- `GET /api/auth/me` - Mevcut kullanıcı bilgileri

#### Şifre Yönetimi
- `POST /api/auth/change-password` - Şifre değiştir (mevcut şifre ile)
- `POST /api/auth/force-change-password` - Zorunlu şifre değiştirme (ilk giriş)

#### Claim Yönetimi
- `POST /api/auth/users/claims` - Kullanıcıya claim ekle (CanManageUsers)
- `DELETE /api/auth/users/claims?userId=xxx&claimType=xxx` - Claim kaldır (CanManageUsers)

## 🔑 Authorization Policies

### Role-Based
- `AdminOnly` - Sadece Admin
- `ManagerOrAdmin` - Manager veya Admin
- `UserOrAbove` - User, Manager veya Admin

### Claim-Based
- `CanCreateUser` - Admin, Manager veya `Permission:CanCreateUser` claim'i
- `CanManageUsers` - Admin veya `Permission:CanManageUsers` claim'i
- `CanManageRoles` - Admin veya `Permission:CanManageRoles` claim'i

## 🔄 Kullanıcı Oluşturma Akışı

1. **Admin/Manager giriş yapar**
2. **Kullanıcı oluşturma sayfasına gider**
3. **Yeni kullanıcı bilgilerini girer:**
   - Email, Ad, Soyad
   - Default şifre (örn: "Temp123!")
   - Roller (User, Manager, Admin)
4. **Kullanıcı oluşturulur:**
   - `MustChangePassword = true` olarak işaretlenir
   - Şifre hash'lenerek kaydedilir
5. **Yeni kullanıcı giriş yapar:**
   - Default şifre ile giriş yapar
   - `MustChangePassword = true` olduğu için token verilmez
   - Şifre değiştirme sayfasına yönlendirilir
6. **Şifre değiştirir:**
   - Yeni şifre belirler
   - `MustChangePassword = false` olur
   - Token alır ve sisteme giriş yapar

## 🔐 Şifre Hash'leme

.NET Identity otomatik olarak şifreleri hash'ler:
- **Algoritma**: PBKDF2
- **Salt**: Her kullanıcı için unique salt
- **Iterations**: 10,000+ (güvenlik için)
- **Hash Format**: `[algorithm]$[iterations]$[salt]$[hash]`

**Örnek:**
```
Plain: "Admin123!"
Hashed: "AQAAAAEAACcQAAAAE..."
```

## 📝 Seed Data

İlk admin kullanıcısı:
- **Email**: admin@stockapp.com
- **Username**: admin
- **Password**: Admin123!
- **Role**: Admin
- **MustChangePassword**: true (ilk girişte değiştirmeli)

## 🛡️ Güvenlik Notları

1. **Şifreler asla plain text saklanmaz** - Identity otomatik hash'ler
2. **Default şifreler güçlü olmalı** - En az 6 karakter, büyük/küçük harf, rakam
3. **İlk girişte şifre değiştirme zorunlu** - Güvenlik için
4. **Claim-based yetkilendirme** - Esnek yetki yönetimi
5. **Token expiration** - 60 dakika (configurable)

## 🚀 Migration

```bash
cd StockApp
dotnet ef migrations add AddIdentityAndPasswordChange
dotnet ef database update
```

## 📊 Veritabanı Değişiklikleri

### ApplicationUser Tablosu
- `MustChangePassword` (bool) - Yeni kolon eklendi

### AspNetUserClaims Tablosu
- Claim'ler bu tabloda saklanır
- Format: `Permission:CanCreateUser`

## 🔄 Frontend Entegrasyonu

1. Login sayfası - MustChangePassword kontrolü
2. Password change modal - İlk giriş için
3. User management sayfası - Admin panel
4. Role/Claim management - Dinamik yetki yönetimi
5. Protected routes - Yetki kontrolü







