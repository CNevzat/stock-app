# Stock App - Windows Executable Oluşturma Kılavuzu

Bu kılavuz, Stock App uygulamasını Windows'ta çalıştırılabilir bir .exe dosyası olarak paketlemek için adım adım talimatlar içerir.

## 📋 Gereksinimler

### Geliştirme Makinesinde (Build için)
- ✅ .NET 9.0 SDK
- ✅ Node.js 18+ ve npm
- ✅ Windows İşletim Sistemi

### Hedef Makinede (Çalıştırmak için)
- ✅ Windows 10/11 (veya Windows Server 2016+)
- ✅ İnternet bağlantısı (sadece ilk çalıştırmada gerekebilir)
- ✅ .NET Runtime gerekmez (self-contained olduğu için)

## 🚀 Adım Adım Kurulum

### Yöntem 1: Batch Script Kullanarak (Önerilen)

1. **Script'i çalıştırın:**
   ```cmd
   build-windows.bat
   ```

2. Script otomatik olarak:
   - Frontend'i build eder
   - Frontend dosyalarını backend'e kopyalar
   - Backend'i Windows executable olarak publish eder
   - Veritabanı dosyasını kopyalar

3. **Çıktı klasörü:** `publish\win-x64\` klasöründe `StockApp.exe` dosyası oluşturulur

### Yöntem 2: PowerShell Script Kullanarak

1. **PowerShell'i yönetici olarak açın**

2. **Execution policy'yi ayarlayın (gerekirse):**
   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

3. **Script'i çalıştırın:**
   ```powershell
   .\build-windows.ps1
   ```

### Yöntem 3: Manuel Build

Eğer script kullanmak istemiyorsanız, aşağıdaki adımları manuel olarak takip edebilirsiniz:

#### 1. Frontend Build

```cmd
cd frontend
npm install
npm run build
```

#### 2. Frontend Dosyalarını Kopyala

```cmd
cd ..
xcopy /E /I /Y frontend\dist\* StockApp\wwwroot
```

#### 3. Backend Publish

```cmd
cd StockApp
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ..\publish\win-x64
```

#### 4. Veritabanı Dosyasını Kopyala

```cmd
copy /Y stockapp.db ..\publish\win-x64\stockapp.db
```

## 📦 Dağıtım Paketi Hazırlama

Build işlemi tamamlandıktan sonra:

1. **`publish\win-x64`** klasörünün tamamını kopyalayın
2. Bu klasörü ZIP olarak paketleyebilir veya doğrudan paylaşabilirsiniz
3. Klasör içeriği:
   ```
   publish/win-x64/
   ├── StockApp.exe          (Ana executable dosya)
   ├── stockapp.db          (Veritabanı dosyası)
   ├── appsettings.json     (Yapılandırma dosyası)
   ├── wwwroot/             (Frontend dosyaları ve resimler)
   └── ... (diğer runtime dosyaları)
   ```

## ▶️ Uygulamayı Çalıştırma

### Hedef Makinede:

1. **Paketi açın:** `publish\win-x64` klasörünü hedef makineye kopyalayın

2. **StockApp.exe'yi çalıştırın:**
   - Çift tıklayarak veya
   - Komut satırından: `StockApp.exe`

3. **Tarayıcıyı açın:**
   - Varsayılan olarak: `http://localhost:5134`
   - Port değiştirmek için `appsettings.json` dosyasını düzenleyin

## ⚙️ Yapılandırma

### Port Değiştirme

`publish\win-x64\appsettings.json` dosyasını düzenleyerek portu değiştirebilirsiniz:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  }
}
```

### Veritabanı Konumu

Veritabanı dosyası (`stockapp.db`) executable ile aynı klasörde olmalıdır. İlk çalıştırmada otomatik olarak oluşturulur.

## 🔧 Sorun Giderme

### "Port already in use" Hatası

Başka bir uygulama aynı portu kullanıyor olabilir:
- `appsettings.json` dosyasında portu değiştirin
- Veya çalışan diğer uygulamayı kapatın

### "Database file not found" Hatası

- Veritabanı dosyasının executable ile aynı klasörde olduğundan emin olun
- Uygulama ilk çalıştırmada veritabanını otomatik oluşturur

### Frontend Dosyaları Yüklenmiyor

- `wwwroot\dist` klasörünün mevcut olduğundan emin olun
- Build işlemini tekrar çalıştırın

### Antivirus Uyarısı

Self-contained executable'lar bazen antivirus yazılımları tarafından şüpheli olarak işaretlenebilir:
- Bu normal bir durumdur
- Güvenilir bir kaynaktan geldiğinden emin olun
- Gerekirse antivirus ayarlarından istisna ekleyin

## 📝 Notlar

- ✅ Self-contained deployment kullanıldığı için hedef makinede .NET Runtime gerekmez
- ✅ Tüm bağımlılıklar executable içine dahil edilir
- ✅ İlk çalıştırmada biraz daha uzun sürebilir (JIT compilation)
- ✅ Veritabanı dosyası yedeklenmesi önerilir

## 🎯 Alternatif Deployment Seçenekleri

### Framework-Dependent Deployment (Daha küçük dosya boyutu)

.NET Runtime'un yüklü olması gerektiğinde:

```cmd
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ..\publish\win-x64
```

### Linux/Mac için Build

```cmd
# Linux
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ../publish/linux-x64

# macOS
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o ../publish/osx-x64
```

## 📞 Destek

Sorun yaşarsanız:
1. Build loglarını kontrol edin
2. `.NET SDK` ve `Node.js` versiyonlarını kontrol edin
3. Tüm bağımlılıkların yüklü olduğundan emin olun

