# TradingApp — Borsa Takip & Analiz Platformu

Modern, ölçeklenebilir bir **borsa takip uygulaması**. BIST, ABD hisseleri ve kripto paraları takip eder; portföyünüzü canlı izler; fiyat alarmları kurar; teknik analiz ve "tavan potansiyeli" skoru üretir.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)
![React](https://img.shields.io/badge/React-19-61DAFB)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791)
![Redis](https://img.shields.io/badge/Redis-7-DC382D)

---

## ⚠️ Yasal Uyarı

**Bu uygulama demo amaçlıdır ve YATIRIM TAVSİYESİ DEĞİLDİR.**

- Gerçek alım-satım yapmaz (emir motoru pasif)
- Kullanıcı bakiyeleri simülasyon amaçlıdır
- Veriler **Yahoo Finance**, **Stooq**, **CoinMarketCap** ve **BIST Data Service** açık kaynak servislerden gelir; doğruluk garantisi yoktur
- Üretimde kullanmadan önce yasal düzenlemeleri (SPK, KVKK, GDPR) kontrol edin

---

## Mimari
┌─────────────────┐ ┌──────────────────┐
│ React Frontend │────▶│ .NET 9 API │
│ (Nginx + Vite) │ │ (Clean Arch.) │
└─────────────────┘ └────────┬─────────┘
│
┌────────────────┼────────────────┐
│ │ │
┌──────▼─────┐ ┌──────▼─────┐ ┌──────▼─────┐
│ PostgreSQL │ │ Redis │ │ SignalR │
│ (EF Core) │ │ (cache) │ │ (real-time)│
└────────────┘ └────────────┘ └────────────┘
│
┌────────▼─────────┐
│ Market Data │
│ Yahoo / Stooq / │
│ CoinMarketCap / │
│ BIST Data Svc │
└──────────────────┘

**Katmanlar:**
- `TradingApp.Domain` — Entity, Value Object, iş kuralları (dış bağımlılık yok)
- `TradingApp.Application` — CQRS (MediatR), Validator, DTO (sadece Domain'e bağlı)
- `TradingApp.Infrastructure` — EF Core, Redis, HTTP client'lar, background service'ler
- `TradingApp.Api` — HTTP, Middleware, DI, SignalR Hub'ları
- `frontend/trading-app-web` — React 19 + Vite + TypeScript + Tailwind v4

---

## Hızlı Başlangıç

### Gereksinimler

- **Docker Desktop** (4.30+) — Compose v2 dahil
- **.NET 9 SDK** — [indir](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 22+** ve **npm** — [indir](https://nodejs.org/)

### 1. Klonla ve env dosyasını hazırla

```bash
git clone <repo-url> TradingApp
cd TradingApp
cp .env.example .env
# .env içindeki JWT_KEY'i değiştir (üretimde şart):
#   openssl rand -base64 48

2. Tek komutla çalıştır

docker compose up -d

3. Adresler
Servis	URL
Frontend	http://localhost:3000
API Swagger	http://localhost:5000/swagger
Health	http://localhost:5000/health/ready
pgAdmin	http://localhost:5050 (admin@local.dev / admin)

Demo giriş: swagger@trading.local / Passw0rd!

Geliştirme Modu (Docker'sız)
Backend
bash

# Sadece altyapı
docker compose up -d postgres redis

# API
dotnet run --project src/TradingApp.Api

API çalışır: http://localhost:5000
Frontend
bash

cd frontend/trading-app-web
npm install
npm run dev

Frontend çalışır: http://localhost:5173 (Vite proxy ile API'ye bağlanır)
Testler
bash

dotnet test

Veri Kaynakları
Kaynak	Kapsam	Ücretsiz	Not
Yahoo Finance	BIST (.IS), ABD, kripto	✅	Resmi API değil; rate limit ~100-2000/dk
Stooq	ABD, global	✅	CSV formatı, keyless
CoinMarketCap	Kripto	✅	/public-api keyless, 35+ endpoint
BIST Data Service	~620 BIST hissesi	✅	Ayrı repo, Docker ile çalışır

Fallback zinciri: Yahoo → BIST → CoinMarketCap → Stooq → Mock

BIST Data Service'i etkinleştirmek için:
bash

git clone https://github.com/Armert-Labs/bist-data-service.git bist-data-service
cd bist-data-service
cp .env.example .env
# .env'de REDIS_PASSWORD ve API_HOST değerlerini ayarla
docker compose up -d redis api updater

API Endpoint'leri
Auth

    POST /api/auth/register

    POST /api/auth/login

    POST /api/auth/refresh

    POST /api/auth/logout

Market

    GET /api/assets

    GET /api/assets/{symbol}

    GET /api/market/{symbol}

    GET /api/market/{symbol}/candles?interval=1D&limit=300

Portföy

    GET /api/portfolio

    POST /api/portfolio/positions

    PUT /api/portfolio/positions/{id}

    DELETE /api/portfolio/positions/{id}

Watchlist

    GET /api/watchlist

    POST /api/watchlist/{symbol}

    DELETE /api/watchlist/{assetId}

Alarmlar

    GET /api/price-alerts

    POST /api/price-alerts

    DELETE /api/price-alerts/{id}

Bildirimler

    GET /api/notifications

    PUT /api/notifications/{id}/read

Analiz

    GET /api/analysis/{symbol}

    GET /api/analysis/scan?assetType=Stock

Yönetim

    POST /api/admin/sync-bist-assets — Tüm BIST hisselerini DB'ye çeker

SignalR

    /hubs/market — Canlı fiyat yayını (MarketPriceUpdated)

    /hubs/notifications — Kullanıcı bildirimleri (NotificationReceived, JWT gerekir)

Öne Çıkan Özellikler

    ✅ Clean Architecture + CQRS (MediatR) + DDD prensipleri

    ✅ JWT + Refresh Token rotasyonu

    ✅ SignalR ile canlı fiyat ve bildirim akışı

    ✅ Redis cache + pub/sub

    ✅ Çoklu veri kaynağı (Yahoo + Stooq + CMC + BIST)

    ✅ Teknik analiz motoru (RSI, MACD, MA, Bollinger, hacim)

    ✅ Tavan potansiyeli skoru (0-100, ağırlıklı sinyal birleşimi)

    ✅ Manuel portföy takibi + canlı P/L

    ✅ Fiyat alarmları + otomatik bildirim

    ✅ BIST tüm hisseler (620+ enstrüman)

    ✅ PostgreSQL + EF Core migrations

    ✅ Serilog yapılandırılmış loglama

    ✅ Docker Compose ile tek komut kurulum

Yol Haritası

Tamamlanan:

    ☑

    Auth + JWT + Refresh Token
    ☑

    Çoklu market data kaynağı
    ☑

    SignalR canlı fiyat
    ☑

    Watchlist + Price Alerts
    ☑

    Manuel Portföy + P/L
    ☑

    Teknik Analiz + Tavan Potansiyeli
    ☑

    React frontend (TradingView grafik)
    ☑

    Docker production setup

Gelecek (opsiyonel):

    □

    Gerçek broker entegrasyonu (Alpaca, Interactive Brokers)
    □

    KYC/AML akışı
    □

    Ödeme sağlayıcı (Iyzico, Stripe)
    □

    Prometheus + Grafana monitoring
    □

    Kubernetes deployment
    □

    Mobil uygulama (React Native)

Lisans

MIT — Demo/eğitim amaçlı. Üretim kullanımı için yasal düzenlemeleri kontrol edin.
Katkı

Pull request'ler memnuniyetle karşılanır. Büyük değişiklikler için önce bir issue açın.

Son Güncelleme: 2026-09-21
Versiyon: 1.0.0
text


**Kaydet.**

---

## BÖLÜM 4 — Frontend'de Yasal Uyarı

**Dosya:** `frontend/trading-app-web/src/components/Layout.tsx`

**Sidebar'ın en altına**, `user` bloğunun **üstüne** ekle:

**Şu satırı bul:**

```typescript
        <div className="p-3 border-t border-[#1f2a44]">

Hemen üstüne ekle:
typescript

        <div className="px-3 py-2 mx-3 mb-2 rounded-lg bg-[#2a1a0a] border border-[#3a2010]">
          <div className="text-[10px] text-[#fbbf24] leading-tight">
            ⚠️ Demo uygulama. Yatırım tavsiyesi değildir.
          </div>
        </div>

Kaydet.
BÖLÜM 5 — Migration Otomatik Uygulanıyor mu?

Kontrol: Program.cs içinde:
csharp

await db.Database.MigrateAsync();

Bu satır olmadan container başlarken migration çalışmaz. Var mı kontrol et.

Dosya: src/TradingApp.Api/Program.cs

app.Run() satırından önce şu blok olmalı:
csharp

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(db, hasher, loggerFactory.CreateLogger("DataSeeder"));
}

Yoksa ekle. Varsa dokunma.
BÖLÜM 6 — Frontend'i Yeniden Build + Test
powershell

cd F:\proje\TradingApp

# 1) Frontend'i yeniden build et (Layout değişti)
docker compose build frontend

# 2) Yeniden başlat
docker compose up -d

# 3) Durum
docker compose ps

Tarayıcıda http://localhost:3000 → hard refresh (Ctrl+Shift+R).

Beklenen: Sidebar'ın alt kısmında sarı uyarı kutusu görünür.
BÖLÜM 7 — Son Kontrol Listesi
#	Kontrol	Beklenen
1	docker compose ps	5 container Up
2	http://localhost:3000	Login ekranı
3	Login	Dashboard açılır
4	Sidebar alt	⚠️ Uyarı kutusu görünür
5	Piyasalar	600+ hisse
6	THYAO	Grafik + analiz + canlı
7	İzleme Listesi	Ekle/kaldır çalışıyor
8	Portföyüm	Pozisyon ekle
9	Alarmlar	Alarm kur → 30 sn'de tetiklen
10	Bildirim zili	Canlı bildirim gelir
11	README.md	GitHub'da render olur
12	.env.example	Var
BÖLÜM 8 — Git Commit
powershell

cd F:\proje\TradingApp

git add .
git commit -m "Phase 11-13: React frontend + Docker + docs + disclaimer"