# 📖 Kullanıcı Kılavuzu

Stock Management Application kullanım kılavuzu. Bu kılavuz, uygulamanın tüm özelliklerini ve nasıl kullanılacağını adım adım açıklar.

## 📋 İçindekiler

- [Giriş](#giriş)
- [Sisteme Giriş](#sisteme-giriş)
- [Dashboard](#dashboard)
- [Kategoriler](#kategoriler)
- [Lokasyonlar](#lokasyonlar)
- [Ürünler](#ürünler)
- [Ürün Öznitelikleri](#ürün-öznitelikleri)
- [Stok Hareketleri](#stok-hareketleri)
- [Yapılacaklar](#yapılacaklar)
- [İpuçları ve Püf Noktaları](#ipuçları-ve-püf-noktaları)
- [Sık Sorulan Sorular](#sık-sorulan-sorular)

---

## Giriş

Stock Management Application, stok yönetiminizi kolaylaştıran modern bir web uygulamasıdır. Bu uygulama ile:

- Ürünlerinizi kategorilere göre organize edebilirsiniz
- Stok miktarlarını takip edebilirsiniz
- Stok hareketlerini kayıt altına alabilirsiniz
- Düşük stok uyarıları alabilirsiniz
- Lokasyon bazlı stok yönetimi yapabilirsiniz
- Ürün özniteliklerini tanımlayabilirsiniz
- Yapılacaklar listenizi yönetebilirsiniz
- Dashboard ile genel durumu izleyebilirsiniz

---

## Sisteme Giriş

### İlk Kurulum

1. **Backend'i başlatın:**
   ```bash
   cd StockApp
   dotnet restore
   dotnet ef database update
   dotnet run
   ```

2. **Frontend'i başlatın:**
   ```bash
   cd frontend
   npm install
   npm run dev
   ```

3. Tarayıcınızda `http://localhost:5173` adresine gidin.

### Arayüz Genel Bakış

Uygulama açıldığında üst kısımda navigasyon menüsü görünür:

- **Dashboard** - Genel istatistikler ve grafikler
- **Kategoriler** - Ürün kategorilerini yönetme
- **Lokasyonlar** - Depo/lokasyon yönetimi
- **Ürünler** - Ürün yönetimi
- **Öznitelikler** - Ürün özniteliklerini yönetme
- **Stok Hareketleri** - Stok giriş/çıkış kayıtları
- **Yapılacaklar** - Görev yönetimi

---

## Dashboard

Dashboard sayfası, stok sisteminizin genel durumunu gösterir.

### Önemli Metrikler

Dashboard'da şu metrikleri görebilirsiniz:

- **Toplam Ürün** - Sistemdeki toplam ürün sayısı
- **Toplam Kategori** - Oluşturulan kategori sayısı
- **Toplam Lokasyon** - Tanımlı lokasyon sayısı
- **Düşük Stok** - Düşük stok seviyesinde olan ürün sayısı

### Grafikler ve Görselleştirmeler

1. **Kategori Dağılımı** - Ürünlerin kategorilere göre dağılımını gösteren pasta grafiği
2. **Stok Durumu** - Stok durumunu gösteren bar grafiği
3. **Son Hareketler** - En son yapılan stok hareketlerinin listesi
4. **Lokasyon Bazlı Dağılım** - Ürünlerin lokasyonlara göre dağılımı

### Real-time Güncellemeler

Dashboard, SignalR ile real-time olarak güncellenir. Sağ üst köşedeki durum göstergesi:
- **Yeşil nokta**: Canlı bağlantı aktif
- **Kırmızı nokta**: Bağlantı yok

### PDF Rapor İndirme

Kritik stok raporunu PDF olarak indirmek için "PDF İndir" butonuna tıklayın.

### Yapay Zekâ Raporu (Gemini)

Doğal dilde soru sorarak anlık rapor almak için:
1. Swagger arayüzünden `POST /api/reports/natural-language` endpoint'ini açın.
2. `question` alanına sorunuzu yazın (örn. "Bu ay en karlı ürün hangisi?").
3. İsteği göndererek Gemini servisinden gelen maddeli raporu inceleyin.

> Not: Bu özellik için sistem yöneticisinin `GEMINI_API_KEY` anahtarını tanımlamış olması gerekir. Tanımlı değilse endpoint bilgilendirici bir mesaj döner.

---

## Kategoriler

Kategoriler sayfasında ürünlerinizi organize edebilirsiniz.

### Kategori Listesi

Kategoriler sayfasında:
- Tüm kategorilerin listesi görüntülenir
- Sayfalama ile gezinme yapabilirsiniz
- Arama kutusu ile kategori arayabilirsiniz
- Her kategorinin oluşturulma tarihi gösterilir

### Yeni Kategori Ekleme

1. **"Yeni Kategori"** butonuna tıklayın
2. Modal pencerede kategori adını girin
3. **"Kaydet"** butonuna tıklayın

**Örnek Kategoriler:**
- Elektronik
- Giyim
- Gıda
- Ev Eşyaları
- Spor Ürünleri

### Kategori Düzenleme

1. Kategori listesinde düzenlemek istediğiniz kategorinin yanındaki **"Düzenle"** butonuna tıklayın
2. Modal pencerede kategori adını değiştirin
3. **"Güncelle"** butonuna tıklayın

### Kategori Silme

1. Silmek istediğiniz kategorinin yanındaki **"Sil"** butonuna tıklayın
2. Onay mesajında **"Evet, Sil"** butonuna tıklayın

**⚠️ Uyarı:** Kategori silindiğinde ilişkili tüm ürünler de silinir. Bu işlem geri alınamaz!

### Kategori Detayı

Bir kategoriye tıkladığınızda kategori detayları görüntülenir:
- Kategori adı
- Oluşturulma tarihi
- Güncellenme tarihi
- İlişkili ürün sayısı

---

## Lokasyonlar

Lokasyonlar sayfasında depo veya mağaza gibi fiziksel konumları yönetebilirsiniz.

### Lokasyon Listesi

Lokasyonlar sayfasında:
- Tüm lokasyonların listesi görüntülenir
- Sayfalama ile gezinme yapabilirsiniz
- Arama kutusu ile lokasyon arayabilirsiniz

### Yeni Lokasyon Ekleme

1. **"Yeni Lokasyon"** butonuna tıklayın
2. Modal pencerede:
   - **Lokasyon Adı**: Lokasyon adını girin (örn: "Depo A", "Mağaza 1")
   - **Açıklama**: Opsiyonel açıklama ekleyin (örn: "Üst kat, sol taraf")
3. **"Kaydet"** butonuna tıklayın

**Örnek Lokasyonlar:**
- Depo A
- Depo B
- Mağaza Merkez
- Şube Kadıköy
- Online Depo

### Lokasyon Düzenleme

1. Lokasyon listesinde düzenlemek istediğiniz lokasyonun yanındaki **"Düzenle"** butonuna tıklayın
2. Modal pencerede bilgileri güncelleyin
3. **"Güncelle"** butonuna tıklayın

### Lokasyon Silme

1. Silmek istediğiniz lokasyonun yanındaki **"Sil"** butonuna tıklayın
2. Onay mesajında **"Evet, Sil"** butonuna tıklayın

**⚠️ Uyarı:** Lokasyon silindiğinde ilişkili ürünlerin lokasyon bilgisi kaldırılır, ancak ürünler silinmez.

---

## Ürünler

Ürünler sayfası, stok yönetiminin merkezidir. Burada tüm ürünleri görüntüleyebilir, yeni ürün ekleyebilir ve mevcut ürünleri güncelleyebilirsiniz.

### Ürün Listesi

Ürünler sayfasında:
- Tüm ürünler kart görünümünde listelenir
- Her ürün kartında şu bilgiler görünür:
  - Ürün resmi (varsa)
  - Ürün adı
  - Stok kodu
  - Stok miktarı
  - Kategori (renkli etiket ile)
  - Lokasyon (varsa)
  - Düşük stok uyarısı (kırmızı arka plan)

### Filtreleme ve Arama

**Kategori Filtreleme:**
1. Üst kısımdaki kategori dropdown'ından bir kategori seçin
2. Sadece seçili kategorideki ürünler gösterilir
3. "Tümü" seçeneği ile filtreyi kaldırabilirsiniz

**Lokasyon Filtreleme:**
1. Lokasyon arama kutusuna lokasyon adı yazın
2. Dropdown'dan bir lokasyon seçin
3. Sadece seçili lokasyondaki ürünler gösterilir

**Arama:**
1. Arama kutusuna ürün adı, stok kodu veya açıklama yazın
2. Arama otomatik olarak gerçekleşir (800ms debounce)
3. Sonuçlar anlık olarak filtrelenir

### Yeni Ürün Ekleme

1. **"Yeni Ürün"** butonuna tıklayın
2. Modal pencerede formu doldurun:

   **Zorunlu Alanlar:**
   - **Ürün Adı**: Ürünün adını girin
   - **Stok Miktarı**: Başlangıç stok miktarını girin
   - **Kategori**: Dropdown'dan bir kategori seçin

   **Opsiyonel Alanlar:**
   - **Açıklama**: Ürün açıklaması
   - **Düşük Stok Eşiği**: Varsayılan 5, değiştirebilirsiniz
   - **Lokasyon**: Dropdown'dan bir lokasyon seçin
   - **Ürün Resmi**: Resim yükleyin (JPEG, PNG, WebP)

3. **"Kaydet"** butonuna tıklayın

**Stok Kodu:** Ürün kaydedildiğinde otomatik olarak benzersiz bir stok kodu atanır (örn: ABC433).

### Ürün Düzenleme

1. Ürün kartında **"Düzenle"** butonuna tıklayın
2. Modal pencerede değiştirmek istediğiniz alanları güncelleyin:
   - Sadece değiştirmek istediğiniz alanları doldurun
   - Diğer alanlar mevcut değerlerini korur
3. Yeni resim yükleyebilir veya mevcut resmi değiştirebilirsiniz
4. **"Güncelle"** butonuna tıklayın

**Not:** Lokasyonu kaldırmak için lokasyon alanını boş bırakın.

### Ürün Silme

1. Ürün kartında **"Sil"** butonuna tıklayın
2. Onay mesajında **"Evet, Sil"** butonuna tıklayın

**⚠️ Uyarı:** Ürün silindiğinde:
- Ürün resmi silinir
- İlişkili tüm öznitelikler silinir
- İlişkili stok hareketleri korunur (geçmiş kayıt)
- Bu işlem geri alınamaz!

### Ürün Detayı

Ürün kartına tıkladığınızda ürün detayları görüntülenir:
- Ürün bilgileri
- Stok durumu
- Kategori ve lokasyon bilgisi
- Ürün öznitelikleri listesi
- Oluşturulma ve güncellenme tarihleri

### Excel'e Aktarma

Ürünleri Excel dosyasına aktarmak için:
1. Üst kısımdaki **"Excel'e Aktar"** butonuna tıklayın
2. Dosya otomatik olarak indirilir
3. Dosya adı: `Urunler_YYYYMMDD_HHMMSS.xlsx`

### Düşük Stok Uyarıları

Stok miktarı, düşük stok eşiğinin altına düştüğünde:
- Ürün kartı kırmızı arka planla gösterilir
- Dashboard'da "Düşük Stok" metrikleri güncellenir
- Uyarı mesajı görüntülenir

---

## Ürün Öznitelikleri

Ürün öznitelikleri sayfasında ürünlerinize dinamik özellikler ekleyebilirsiniz.

### Öznitelik Listesi

Öznitelikler sayfasında:
- Tüm öznitelikler tablo formatında görüntülenir
- Her öznitelik şu bilgileri içerir:
  - Ürün adı
  - Anahtar (Key) - Öznitelik tipi (örn: "RAM", "Ekran Boyutu")
  - Değer (Value) - Öznitelik değeri (örn: "16GB", "15.6 inç")

### Filtreleme

**Ürün Filtreleme:**
1. Üst kısımdaki ürün dropdown'ından bir ürün seçin
2. Sadece seçili ürüne ait öznitelikler gösterilir

**Arama:**
1. Arama kutusuna anahtar kelimesi yazın
2. Sonuçlar filtrelenir

### Yeni Öznitelik Ekleme

1. **"Yeni Öznitelik"** butonuna tıklayın
2. Modal pencerede:
   - **Ürün**: Dropdown'dan bir ürün seçin
   - **Anahtar**: Öznitelik tipini girin (örn: "RAM", "Ekran Boyutu", "İşlemci")
   - **Değer**: Öznitelik değerini girin (örn: "16GB", "15.6 inç", "Intel Core i7")
3. **"Kaydet"** butonuna tıklayın

**Örnek Öznitelikler:**
- RAM: 16GB
- Ekran Boyutu: 15.6 inç
- İşlemci: Intel Core i7
- Renk: Siyah
- Ağırlık: 2.5 kg
- Depolama: 512GB SSD

### Öznitelik Düzenleme

1. Tabloda düzenlemek istediğiniz özniteliğin yanındaki **"Düzenle"** butonuna tıklayın
2. Modal pencerede bilgileri güncelleyin
3. **"Güncelle"** butonuna tıklayın

### Öznitelik Silme

1. Silmek istediğiniz özniteliğin yanındaki **"Sil"** butonuna tıklayın
2. Onay mesajında **"Evet, Sil"** butonuna tıklayın

### Excel'e Aktarma

Öznitelikleri Excel dosyasına aktarmak için:
1. Üst kısımdaki **"Excel'e Aktar"** butonuna tıklayın
2. Dosya otomatik olarak indirilir
3. Dosya adı: `Urun_Oznitelikleri_YYYYMMDD_HHMMSS.xlsx`

---

## Stok Hareketleri

Stok hareketleri sayfasında stok giriş ve çıkışlarını kayıt altına alabilirsiniz.

### Hareket Listesi

Stok hareketleri sayfasında:
- Tüm stok hareketleri tablo formatında görüntülenir
- Her hareket şu bilgileri içerir:
  - Ürün adı
  - Kategori
  - Hareket tipi (Giriş/Çıkış)
  - Miktar
  - Açıklama
  - Tarih

### Filtreleme

**Ürün Filtreleme:**
1. Üst kısımdaki ürün dropdown'ından bir ürün seçin
2. Sadece seçili ürüne ait hareketler gösterilir

**Kategori Filtreleme:**
1. Kategori dropdown'ından bir kategori seçin
2. Sadece seçili kategorideki ürünlere ait hareketler gösterilir

**Hareket Tipi Filtreleme:**
1. Hareket tipi dropdown'ından seçim yapın:
   - **Tümü**: Tüm hareketler
   - **Giriş**: Sadece stok girişleri
   - **Çıkış**: Sadece stok çıkışları

### Yeni Stok Hareketi Oluşturma

1. **"Yeni Hareket"** butonuna tıklayın
2. Modal pencerede formu doldurun:

   **Zorunlu Alanlar:**
   - **Ürün**: Dropdown'dan bir ürün seçin
   - **Kategori**: Otomatik olarak ürünün kategorisi seçilir
   - **Hareket Tipi**: Giriş veya Çıkış seçin
   - **Miktar**: Hareket miktarını girin

   **Opsiyonel Alanlar:**
   - **Açıklama**: Hareket açıklaması (örn: "Tedarikçiden gelen stok", "Satış")

3. **"Kaydet"** butonuna tıklayın

**Stok Güncelleme:** Stok hareketi kaydedildiğinde ürünün stok miktarı otomatik olarak güncellenir:
- **Giriş**: Stok miktarı artar
- **Çıkış**: Stok miktarı azalır (yeterli stok yoksa hata mesajı gösterilir)

### Hareket Detayları

Stok hareketi kaydedildikten sonra:
- Ürünün stok miktarı otomatik olarak güncellenir ve tablodaki değer güncel stok seviyesini yansıtır.
- Hareket geçmiş listesine eklenir; açıklama alanından işlem notları görüntülenebilir.

### Excel'e Aktarma

Stok hareketlerini Excel dosyasına aktarmak için:
1. Sayfanın üst kısmındaki **"Excel'e Aktar"** butonuna tıklayın.
2. Dosya otomatik olarak indirilir.
3. Dosya adı `Stok_Hareketleri_YYYYMMDD_HHMMSS.xlsx` formatındadır.

---

## Yapılacaklar

Yapılacaklar sayfasında görevlerinizi yönetebilirsiniz.

### Görev Listesi

Yapılacaklar sayfasında:
- Tüm görevler kart görünümünde listelenir
- Her görev kartında şu bilgiler görünür:
  - Başlık
  - Açıklama
  - Durum (Yapılacak/Devam Ediyor/Tamamlandı)
  - Öncelik (Düşük/Orta/Yüksek)
  - Oluşturulma tarihi

### Filtreleme

**Durum Filtreleme:**
1. Durum dropdown'ından bir durum seçin:
   - **Tümü**: Tüm görevler
   - **Yapılacak**: Henüz başlanmamış görevler
   - **Devam Ediyor**: Devam eden görevler
   - **Tamamlandı**: Tamamlanan görevler

**Öncelik Filtreleme:**
1. Öncelik dropdown'ından bir öncelik seçin:
   - **Tümü**: Tüm görevler
   - **Düşük**: Düşük öncelikli görevler
   - **Orta**: Orta öncelikli görevler
   - **Yüksek**: Yüksek öncelikli görevler

### Yeni Görev Ekleme

1. **"Yeni Görev"** butonuna tıklayın
2. Modal pencerede formu doldurun:

   **Zorunlu Alanlar:**
   - **Başlık**: Görev başlığını girin

   **Opsiyonel Alanlar:**
   - **Açıklama**: Görev açıklaması
   - **Durum**: Varsayılan "Yapılacak", değiştirebilirsiniz
   - **Öncelik**: Varsayılan "Orta", değiştirebilirsiniz

3. **"Kaydet"** butonuna tıklayın

### Görev Düzenleme

1. Görev kartında **"Düzenle"** butonuna tıklayın
2. Modal pencerede bilgileri güncelleyin:
   - Durumu değiştirebilirsiniz (örn: "Yapılacak" → "Devam Ediyor" → "Tamamlandı")
   - Önceliği değiştirebilirsiniz
   - Başlık ve açıklamayı güncelleyebilirsiniz
3. **"Güncelle"** butonuna tıklayın

### Görev Silme

1. Görev kartında **"Sil"** butonuna tıklayın
2. Onay mesajında **"Evet, Sil"** butonuna tıklayın

### Görev Durumları

- **Yapılacak**: Henüz başlanmamış görevler (gri arka plan)
- **Devam Ediyor**: Devam eden görevler (mavi arka plan)
- **Tamamlandı**: Tamamlanan görevler (yeşil arka plan, üstü çizili)

### Görev Öncelikleri

- **Düşük**: Düşük öncelikli görevler (sarı etiket)
- **Orta**: Orta öncelikli görevler (mavi etiket)
- **Yüksek**: Yüksek öncelikli görevler (kırmızı etiket)

---

## İpuçları ve Püf Noktaları

### 1. Hızlı Navigasyon

- Üst menüden herhangi bir sayfaya tek tıkla geçebilirsiniz
- Ürünler sayfasından kategoriye tıklayarak kategori filtreleme yapabilirsiniz
- URL parametreleri ile belirli kategori veya lokasyona doğrudan gelebilirsiniz:
  - `http://localhost:5173/products?categoryId=1`
  - `http://localhost:5173/products?locationId=2`

### 2. Toplu İşlemler

- Excel export ile ürün ve öznitelik verilerini toplu olarak dışa aktarabilirsiniz
- Filtreleme yaparak belirli bir grubu seçip Excel'e aktarabilirsiniz

### 3. Stok Takibi

- Düşük stok eşiğini her ürün için özelleştirebilirsiniz
- Dashboard'da düşük stoklu ürünleri görebilirsiniz
- Stok hareketleri ile geçmişi takip edebilirsiniz

### 4. Ürün Organizasyonu

- Kategoriler ile ürünleri organize edin
- Lokasyonlar ile fiziksel konumları takip edin
- Öznitelikler ile ürün detaylarını zenginleştirin

### 5. Arama ve Filtreleme

- Arama kutusu ürün adı, stok kodu ve açıklamada arama yapar
- Birden fazla filtreyi birlikte kullanabilirsiniz
- Sayfalama ile büyük listelerde kolayca gezinebilirsiniz

### 6. Real-time Güncellemeler

- SignalR bağlantısı aktif olduğunda dashboard otomatik güncellenir
- Başka bir kullanıcı değişiklik yaptığında sayfayı yenilemeden güncellemeleri görebilirsiniz

---

## Sık Sorulan Sorular

### S: Ürün silindiğinde ne olur?

**C:** Ürün silindiğinde:
- Ürün resmi silinir
- İlişkili tüm öznitelikler silinir
- İlişkili stok hareketleri korunur (geçmiş kayıt için)
- Bu işlem geri alınamaz

### S: Stok hareketi nasıl çalışır?

**C:** Stok hareketi oluşturulduğunda:
- **Giriş**: Ürünün stok miktarı artar
- **Çıkış**: Ürünün stok miktarı azalır (yeterli stok yoksa hata mesajı gösterilir)
- Hareket geçmişe kaydedilir

### S: Düşük stok uyarısı nasıl çalışır?

**C:** Bir ürünün stok miktarı, düşük stok eşiğinin altına düştüğünde:
- Ürün kartı kırmızı arka planla gösterilir
- Dashboard'da "Düşük Stok" metrikleri güncellenir
- Uyarı mesajı görüntülenir

### S: Resim yükleme limiti var mı?

**C:** Şu anda spesifik bir limit yoktur, ancak önerilen:
- Maksimum dosya boyutu: 5MB
- Desteklenen formatlar: JPEG, PNG, WebP
- Önerilen boyut: 800x600 piksel

### S: Excel export nasıl çalışır?

**C:** Excel export:
- Mevcut filtreleri dikkate alır
- Tüm sayfalardaki verileri içerir (sayfalama yok)
- Otomatik olarak indirilir
- Dosya adı tarih/saat içerir

### S: Kategori silindiğinde ürünler ne olur?

**C:** Kategori silindiğinde:
- İlişkili tüm ürünler silinir
- Bu işlem geri alınamaz
- Silmeden önce ürünleri başka kategorilere taşımanız önerilir

### S: Lokasyon silindiğinde ürünler ne olur?

**C:** Lokasyon silindiğinde:
- Ürünler silinmez
- Ürünlerin lokasyon bilgisi kaldırılır
- Ürünler lokasyonsuz olarak devam eder

### S: Arama nasıl çalışır?

**C:** Arama:
- Ürün adı, stok kodu ve açıklamada arama yapar
- Büyük/küçük harf duyarsızdır
- 800ms debounce ile çalışır (yazarken otomatik arama)
- Filtrelerle birlikte kullanılabilir

### S: SignalR bağlantısı kesilirse ne olur?

**C:** SignalR bağlantısı kesilirse:
- Dashboard real-time güncellemeleri durur
- Sayfayı yenilediğinizde bağlantı tekrar kurulur
- Diğer sayfalar normal çalışmaya devam eder

### S: Sayfalama nasıl çalışır?

**C:** Sayfalama:
- Her sayfada varsayılan 10 kayıt gösterilir
- Sayfa numarası ile gezinebilirsiniz
- Toplam sayfa sayısı gösterilir
- Önceki/sonraki sayfa butonları mevcuttur

---

## Destek ve İletişim

Sorunlarınız veya önerileriniz için:
- GitHub'da issue açabilirsiniz
- Dokümantasyon dosyalarını kontrol edebilirsiniz
- API dokümantasyonuna bakabilirsiniz

---

**Son Güncelleme:** 2024-01-01

**Versiyon:** 1.0.0

