# 📱 Mobil Uygulama Build Kılavuzu

Bu kılavuz, Stock App'in Android APK'sını oluşturmak için gereken adımları açıklar.

## 📋 Gereksinimler

### 1. Node.js ve npm
- Node.js 20.19+ veya 22.12+
- npm veya yarn

### 2. Android Studio
- Android Studio (en son sürüm)
- Android SDK (API Level 33+)
- Java JDK 17+

### 3. Capacitor
- Capacitor paketleri zaten yüklü

## 🚀 Build Adımları

### 1. Projeyi Build Et

```bash
cd frontend
npm run build
```

Bu komut `dist` klasöründe production build oluşturur.

### 2. Capacitor Sync

```bash
npm run cap:sync
```

Bu komut web dosyalarını Android projesine kopyalar.

### 3. Android Studio'da Aç

```bash
npm run cap:open:android
```

Veya manuel olarak:
```bash
cd frontend/android
# Android Studio'yu aç ve android klasörünü import et
```

### 4. Android Studio'da Build

1. Android Studio açıldıktan sonra Gradle sync'in tamamlanmasını bekleyin
2. **Build** > **Build Bundle(s) / APK(s)** > **Build APK(s)** seçin
3. Build tamamlandığında APK dosyası şu konumda olacak:
   ```
   android/app/build/outputs/apk/debug/app-debug.apk
   ```

### 5. Release APK Oluşturma (İsteğe Bağlı)

Release APK için:

1. **Build** > **Generate Signed Bundle / APK** seçin
2. **APK** seçin
3. Keystore oluşturun veya mevcut keystore'u kullanın
4. Build tamamlandığında APK şu konumda olacak:
   ```
   android/app/build/outputs/apk/release/app-release.apk
   ```

## 🔧 API URL Yapılandırması

### Development (Emulator)

Android Emulator için varsayılan IP: `http://10.0.2.2:5134`

### Gerçek Cihaz

Gerçek Android cihazda kullanmak için:

1. Bilgisayarınızın IP adresini öğrenin:
   ```bash
   # macOS/Linux
   ifconfig | grep "inet "
   
   # Windows
   ipconfig
   ```

2. `.env` dosyası oluşturun veya environment variable ayarlayın:
   ```env
   VITE_MOBILE_API_URL=http://192.168.1.100:5134
   ```
   (192.168.1.100 yerine kendi IP adresinizi yazın)

3. Yeniden build edin:
   ```bash
   npm run build
   npm run cap:sync
   ```

### Production

Production'da API URL'i environment variable ile ayarlanmalı:
```env
VITE_API_BASE_URL=https://your-api-domain.com
```

## 📱 APK'yı Telefona Yükleme

### Yöntem 1: USB ile

1. Android telefonunuzda **Geliştirici Seçenekleri**'ni açın
2. **USB Debugging**'i etkinleştirin
3. Telefonu bilgisayara USB ile bağlayın
4. Android Studio'da **Run** > **Run 'app'** seçin
5. Cihazınızı seçin ve yükleyin

### Yöntem 2: APK Dosyasını Transfer Et

1. APK dosyasını telefonunuza kopyalayın (USB, email, cloud storage vb.)
2. Telefonda **Bilinmeyen Kaynaklardan Uygulama Yükleme**'yi etkinleştirin
3. APK dosyasına tıklayın ve yükleyin

## 🐛 Sorun Giderme

### Build Hatası

- Gradle sync yapın: **File** > **Sync Project with Gradle Files**
- Clean build: **Build** > **Clean Project**, sonra **Build** > **Rebuild Project**

### API Bağlantı Hatası

- Emulator kullanıyorsanız: `http://10.0.2.2:5134`
- Gerçek cihaz kullanıyorsanız: Bilgisayarınızın IP adresini kullanın
- Backend'in çalıştığından emin olun
- Firewall ayarlarını kontrol edin

### Network Security Config Hatası

`network_security_config.xml` dosyası zaten oluşturuldu. Eğer hata alırsanız:
- AndroidManifest.xml'de `android:networkSecurityConfig="@xml/network_security_config"` olduğundan emin olun

## 📝 Hızlı Komutlar

```bash
# Build ve sync
npm run cap:build

# Sadece sync
npm run cap:sync

# Android Studio'yu aç
npm run cap:open:android
```

## 🔐 Release Build İçin Keystore Oluşturma

```bash
keytool -genkey -v -keystore stock-app-release.keystore -alias stock-app -keyalg RSA -keysize 2048 -validity 10000
```

Keystore bilgilerini `capacitor.config.ts` dosyasına ekleyin veya Android Studio'da manuel olarak girin.

## 📚 Daha Fazla Bilgi

- [Capacitor Dokümantasyonu](https://capacitorjs.com/docs)
- [Android Studio Kullanım Kılavuzu](https://developer.android.com/studio)




