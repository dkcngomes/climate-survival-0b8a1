---
title: Climate Survival API
emoji: 🌍
colorFrom: green
colorTo: blue
sdk: docker
app_file: backend/Dockerfile
app_port: 7860
---

# Climate Survival 🌍🛒🌱

**Climate-adaptive growing & prepping for what's coming.**

Know what to buy and stock before prices rise, plus which crops to plant that will survive the coming weather changes — with harvest dates starting from this week.

## Live Sites

| Layer | URL | Stack |
|-------|-----|-------|
| **Frontend** | https://climate-survival.netlify.app | Next.js 16 (static export) → Netlify |
| **Backend API** | https://nipunadkcn-climate-survival-api.hf.space | .NET 9 → Hugging Face Spaces (Docker, free) |

> Netlify proxies `/api/*` requests to the Hugging Face Space backend — so the frontend works seamlessly with no CORS issues.

## Architecture

```
┌──────────────────────────┐     ┌─────────────────────────┐     ┌─────────────────────┐
│   Next.js 16 Frontend     │────▶│    .NET 9 Backend API   │────▶│   Open-Meteo APIs   │
│  (Static Export → Netlify)│     │  (Docker → HF Spaces)  │     │  Seasonal/Forecast  │
│                           │     │                         │     │  (free, no key)     │
│  • Browser Geolocation    │     │  • Climate Analysis     │     └─────────────────────┘
│  • Climate Overview       │     │  • Rule Engine + LLM    │     ┌─────────────────────┐
│  • Stock Recommendations  │     │  • Crop Recommendation  │────▶│   Google Gemini     │
│  • Crop Recommendations   │     │  • HydroMeteo Data      │     │  (free tier, opt.)  │
│  • Interactive Charts     │     │  • Contact Form (SMTP)  │     └─────────────────────┘
│  • PDF Survival Report    │     │  • CBSL Price Data      │     ┌─────────────────────┐
│  • Daraz Affiliate Links  │     │  • Caching (Polly)      │────▶│   CBSL Daily Price  │
│  • Multi-language (5)     │     └─────────────────────────┘     │   PDF (free)        │
└──────────────────────────┘                                      └─────────────────────┘
```

## ✨ Features

- **🌡️ Climate Risk Assessment** — Detects El Niño, La Niña, drought, flood, heatwave, cold spell signals from seasonal forecasts
- **🛒 Smart Stock-Up Advice** — Recommends what consumer goods to pre-purchase before prices rise
- **🌱 Crop Recommendations** — Suggests crops that will survive forecast weather changes with planting/harvest dates
- **🤖 AI-Enhanced Recommendations** — Optional Gemini LLM re-ranking for smarter crop suggestions
- **📈 Interactive Climate Charts** — Recharts-based visualizations: anomaly bars, risk gauges, soil moisture, sensor data
- **📄 PDF Survival Report** — Client-side PDF generation (jsPDF + html2canvas) with all recommendations
- **🇱🇰 Sri Lanka Market Prices** — Live daily wholesale/retail prices from CBSL across 5 markets (Dambulla, Nuwara Eliya, Kandy, Pettah, Jaffna)
- **🛍️ Daraz Affiliate Integration** — "Buy on Daraz" links (LK users only, Member ID: `155412816`)
- **🌐 Multi-Language** — English, Sinhala, Spanish, French, Chinese with auto-detect from location
- **📧 Contact Form** — Forwards submissions via Gmail SMTP to dkcngomes@gmail.com
- **📊 Google Analytics** — Tracking via G-ECE9GWGK7E

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | Next.js 16, React, TypeScript, Tailwind CSS, Recharts |
| **Backend** | .NET 9, ASP.NET Core Web API, PdfPig, MailKit |
| **APIs (free)** | Open-Meteo Seasonal, World Bank, BigDataCloud, ip-api.com, Wikipedia, Google Gemini (free tier), CBSL PDF |
| **Frontend Hosting** | Netlify (free, static export) |
| **Backend Hosting** | Hugging Face Spaces (free Docker, port 7860) |
| **Analytics** | Google Analytics (G-ECE9GWGK7E) |
| **Monetization** | Daraz Affiliate (LK only, Member ID: 155412816) |

## Project Structure

```
climate-advisor/
├── backend/                    # .NET 9 Web API
│   ├── Controllers/            # API endpoints
│   │   ├── RecommendationsController.cs
│   │   ├── CropsController.cs
│   │   ├── LocationsController.cs
│   │   ├── PricesController.cs     # CBSL price data
│   │   └── ContactController.cs
│   ├── Models/                 # Domain models
│   ├── Services/               # Business logic
│   │   ├── ClimateService.cs
│   │   ├── RecommendationService.cs
│   │   ├── CropRecommendationService.cs
│   │   ├── GeminiService.cs        # LLM re-ranking
│   │   ├── SriLankaPriceService.cs # CBSL PDF parser
│   │   ├── EmailService.cs         # Gmail SMTP
│   │   └── HydroMeteoService.cs
│   ├── Dockerfile
│   └── Program.cs
├── frontend/                   # Next.js 16 app
│   ├── src/
│   │   ├── app/                # Pages
│   │   ├── components/         # UI components
│   │   ├── i18n/               # Localization (5 languages)
│   │   ├── services/           # API client
│   │   ├── config/             # Affiliate config
│   │   └── types/              # TypeScript types
│   └── next.config.ts
└── infrastructure/             # Legacy (no longer used)
```

## Quick Start (Local Development)

### Prerequisites
- .NET 9 SDK
- Node.js 20+
- Docker (optional)

### Backend
```bash
cd backend
dotnet run --urls "http://localhost:8080"
# Health check: http://localhost:8080/api/recommendations/health
# Test: http://localhost:8080/api/recommendations?lat=6.9271&lng=79.8612
```

### Frontend
```bash
cd frontend
npm install
npm run dev
# Open: http://localhost:3000
```

### Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `Gemini__ApiKey` | No | Gemini API key for LLM crop re-ranking |
| `Email__SmtpPass` | No | Gmail App Password for contact form forwarding |
| `NEXT_PUBLIC_API_URL` | No | Set to `http://localhost:8080` for local dev |

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/recommendations?lat=X&lng=X` | GET | Climate overview + stock-up recommendations |
| `/api/crops?lat=X&lng=X&countryCode=XX` | GET | Crop recommendations for the area |
| `/api/locations/countries` | GET | List supported countries |
| `/api/locations/ip-country` | GET | Detect country from IP |
| `/api/locations/locale/{countryCode}` | GET | Locale info (currency, language) |
| `/api/locations/languages` | GET | Supported languages |
| `/api/prices/sri-lanka` | GET | CBSL daily market prices (32 commodities, 5 markets) |
| `/api/contact` | POST | Submit contact form (forwards to email) |

## 🌍 Live URLs

- **Frontend**: https://climate-survival.netlify.app
- **Backend API**: https://nipunadkcn-climate-survival-api.hf.space
- **Contact page**: https://climate-survival.netlify.app/contact
- **GitHub**: https://github.com/dkcngomes/climate-survival
- **HF Space**: https://huggingface.co/spaces/nipunadkcn/climate-survival-api

## 📦 Deployment

### Backend (Hugging Face Spaces)
```bash
git remote add hf https://huggingface.co/spaces/nipunadkcn/climate-survival-api
git push hf main --force
```

### Frontend (Netlify)
The frontend auto-deploys when pushing to `dkcngomes/climate-survival-0b8a1`:
```bash
git remote add netlify-repo https://github.com/dkcngomes/climate-survival-0b8a1
git push netlify-repo main
```

## 📝 License

MIT
