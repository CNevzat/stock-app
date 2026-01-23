# 🌱 Veritabanı Seed Rehberi

Bu proje, veritabanına otomatik olarak örnek veriler ekleyen bir seed mekanizması içerir.

## 📋 Eklenen Veriler

### Kategoriler (5 adet)
- Elektronik
- Bilgisayar
- Telefon
- Ofis Malzemeleri
- Yazılım

### Lokasyonlar (4 adet)
- Ana Depo
- Şube 1 (Kadıköy)
- Şube 2 (Beşiktaş)
- Showroom

### Ürünler (10 adet)
1. **MacBook Pro 16"** - 5 adet stokta
2. **iPhone 15 Pro** - 12 adet stokta
3. **Samsung Galaxy S24 Ultra** - 8 adet stokta
4. **Dell XPS 15** - 3 adet stokta
5. **Logitech MX Master 3S** - 25 adet stokta
6. **Keychron K8 Pro** - 15 adet stokta
7. **HP LaserJet Pro** - 7 adet stokta
8. **Visual Studio Code Lisansı** - 50 adet stokta
9. **iPad Air** - 2 adet stokta (kritik seviye)
10. **Sony WH-1000XM5** - 18 adet stokta

### Ürün Öznitelikleri
Her ürün için detaylı öznitelikler (işlemci, RAM, depolama, ekran, renk vb.)

### Fiyat Geçmişi
Bazı ürünler için geçmiş fiyat kayıtları

### Stok Hareketleri (9 adet)
Giriş ve çıkış hareketleri ile örnek stok hareket geçmişi

### Yapılacaklar (6 adet)
Farklı durum ve öncelik seviyelerinde örnek görevler

## 🚀 Seed İşlemini Çalıştırma

### Yöntem 1: Otomatik (Önerilen)
Backend'i çalıştırdığınızda, eğer veritabanı boşsa otomatik olarak seed yapılır:

```bash
cd StockApp
dotnet run
```

### Yöntem 2: Veritabanını Sıfırlayıp Yeniden Seed
Mevcut verileri silip yeniden seed yapmak için:

```bash
cd StockApp

# Veritabanını sil
rm stockapp.db

# Migration'ları yeniden uygula
dotnet ef database update

# Backend'i çalıştır (otomatik seed yapacak)
dotnet run
```

### Yöntem 3: Manuel Seed
Eğer veritabanında veri varsa ve yeniden seed yapmak istiyorsanız:

```bash
cd StockApp

# Veritabanını sil
rm stockapp.db

# Backend'i çalıştır
dotnet run
```

## ⚠️ Önemli Notlar

1. **Seed sadece Development ortamında çalışır** - Production'da seed yapılmaz
2. **Veri kontrolü** - Eğer veritabanında zaten veri varsa, seed işlemi atlanır
3. **Tek seferlik** - Seed işlemi sadece boş veritabanı için çalışır

## 🔍 Seed Verilerini Kontrol Etme

### Swagger UI ile
1. Backend'i çalıştırın: `dotnet run`
2. Tarayıcıda açın: `http://localhost:5134/`
3. API endpoint'lerini test edin

### SQLite ile
```bash
cd StockApp
sqlite3 stockapp.db

# Kategorileri görüntüle
SELECT * FROM Categories;

# Ürünleri görüntüle
SELECT * FROM Products;

# Çıkış
.quit
```

## 📊 Örnek Veri İstatistikleri

- **5 Kategori**
- **4 Lokasyon**
- **10 Ürün** (farklı kategorilerde, farklı stok seviyelerinde)
- **40+ Ürün Özniteliği**
- **3 Fiyat Geçmişi Kaydı**
- **9 Stok Hareketi**
- **6 Yapılacak Görev**

## 🛠️ Seed Verilerini Özelleştirme

Seed verilerini değiştirmek için `StockApp/Services/DatabaseSeeder.cs` dosyasını düzenleyin.






