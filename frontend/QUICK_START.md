# 🚀 Frontend Hızlı Başlangıç Rehberi

## Adım 1: Bağımlılıkları Yükle

```bash
cd frontend
npm install
```

## Adım 2: Backend'i Başlat (Başka bir terminalde)

```bash
cd ../StockApp
dotnet run
```

Backend şu adreste çalışacak:
- **API**: `http://localhost:5132/api`
- **Swagger UI**: `http://localhost:5132/`

## Adım 3: Frontend'i Başlat

```bash
cd frontend
npm run dev
```

Frontend şu adreste çalışacak:
- **Frontend**: `http://localhost:5173`

## ✅ Kontrol Listesi

- [ ] Node.js 20+ yüklü mü? (`node --version`)
- [ ] npm yüklü mü? (`npm --version`)
- [ ] Backend çalışıyor mu? (`http://localhost:5132` açılıyor mu?)
- [ ] Frontend çalışıyor mu? (`http://localhost:5173` açılıyor mu?)

## 🔧 Sorun Giderme

### npm install hatası alıyorsanız:

```bash
# node_modules ve package-lock.json'ı sil
rm -rf node_modules package-lock.json

# Tekrar yükle
npm install
```

### Port zaten kullanımda hatası:

```bash
# macOS/Linux
lsof -ti:5173 | xargs kill -9

# Windows
netstat -ano | findstr :5173
taskkill /PID <PID> /F
```

### CORS hatası alıyorsanız:

Backend'in çalıştığından emin olun ve `http://localhost:5173` adresinden eriştiğinizden emin olun.

## 📝 Diğer Komutlar

```bash
# Production build
npm run build

# Production build'i önizle
npm run preview

# Lint kontrolü
npm run lint
```

