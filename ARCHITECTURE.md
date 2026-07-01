# Climate Survival — Technical Architecture Document

> **Version:** 1.0  
> **Prepared for:** Prospective Investors  
> **Date:** June 29, 2026  
> **Live Site:** [climate-survival.netlify.app](https://climate-survival.netlify.app)  
> **Repository:** [github.com/dkcngomes/climate-survival](https://github.com/dkcngomes/climate-survival)

---

## 1. Executive Summary

**Climate Survival** is a climate-adaptive intelligence platform that helps individuals and communities make data-driven decisions about:

- **What food and supplies to stockpile** before climate-driven price surges
- **Which crops to plant** that will survive forecasted weather anomalies
- **When to buy** based on real-time market price data from local sources

The platform aggregates **8+ free public data sources** — including global climate models, satellite-derived hydrometeorological data, national market price reports, and Wikipedia product imagery — processes them through a **hybrid rule-engine + Gemini AI** pipeline, and delivers personalized, localized recommendations via a polished web interface.

Built on a **zero-operating-cost** infrastructure (Netlify + Hugging Face Spaces) and monetized through **Daraz affiliate commerce** in Sri Lanka, Climate Survival is designed for rapid global scalability with no cloud vendor lock-in.

---

## 2. System Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         USERS (Web Browser)                      │
│                      climate-survival.netlify.app                │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                    ┌───────▼───────┐
                    │   Netlify     │  ← Static hosting + CDN
                    │  (Frontend)   │
                    │  Next.js 16   │
                    │  Static Expo  │
                    └───────┬───────┘
                            │  /api/* proxy (netlify.toml)
                    ┌───────▼──────────────┐
                    │ Hugging Face Spaces  │  ← Docker container
                    │   .NET 9 Web API     │
                    │   Backend Services   │
                    └───────┬──────────────┘
                            │
          ┌─────────────────┼─────────────────────┐
          │                 │                     │
          ▼                 ▼                     ▼
   ┌────────────┐   ┌──────────────┐   ┌──────────────────┐
   │  Open-Meteo │   │   Gemini AI  │   │  External APIs   │
   │  Seasonal   │   │  (LLM)       │   │ ──────────────── │
   │  ECMWF      │   │  Crop Re-    │   │ BigDataCloud     │
   │  WMO/ERA5   │   │  commend     │   │ ip-api.com       │
   └────────────┘   │  Re-ranking   │   │ Wikipedia API    │
                     └──────────────┘   │ CBSL Price PDF   │
                                        │ World Bank       │
                                        └──────────────────┘
```

### Key Architectural Decisions

| Decision | Rationale |
|---|---|
| **Static frontend + API backend** | Decoupled architecture allows independent scaling and deployment |
| **Netlify (free tier)** | Generous free tier (100GB bandwidth, auto HTTPS, CDN, form handling) |
| **Hugging Face Spaces Docker** | Free container hosting with GPU support for future ML models |
| **Next.js static export** | Zero server-side costs; pre-renders all pages at build time |
| **.NET 9 minimal API** | High performance, low memory footprint in Docker container |
| **All free APIs** | Zero operational costs; no API keys for most data sources |
| **Polly retry policies** | Resilient to transient API failures with exponential backoff |

---

## 3. Technology Stack

### 3.1 Frontend

| Technology | Version | Purpose |
|---|---|---|
| **Next.js** | 16.2.9 | React framework with static export, file-based routing |
| **React** | 19.2.4 | UI component library |
| **TypeScript** | 5.x | Type-safe development |
| **Tailwind CSS** | v4 | Utility-first CSS framework, zero-runtime |
| **Recharts** | 3.9.0 | Declarative charting (bar, pie, radial, line charts) |
| **jsPDF + html2canvas** | Latest | Client-side PDF report generation |
| **Google Analytics** | G-ECE9GWGK7E | User analytics and engagement tracking |

### 3.2 Backend

| Technology | Version | Purpose |
|---|---|---|
| **.NET** | 9.0 | Cross-platform web framework |
| **ASP.NET Core** | 9.0 | REST API controllers, routing, middleware |
| **Polly** | Latest | HTTP resilience (retry, timeout, circuit breaker) |
| **MailKit** | 4.12.0 | SMTP email delivery (Gmail App Passwords) |
| **PdfPig** | 1.7.0-custom-5 | PDF parsing for CBSL price reports |
| **MemoryCache** | Built-in | In-memory response caching for external APIs |

### 3.3 Infrastructure & DevOps

| Technology | Purpose |
|---|---|
| **GitHub** | Source control, CI/CD triggers |
| **Netlify** | Static hosting, CDN, automatic HTTPS, API proxying |
| **Hugging Face Spaces** | Docker container hosting (free tier, no credit card) |
| **Docker** | Containerized backend deployment |
| **Git** | Version control with 3-remote workflow |

### 3.4 External API Integrations

| API | Endpoint | Purpose | Auth Required |
|---|---|---|---|
| **Open-Meteo Seasonal** | `seasonal-api.open-meteo.com/v1/seasonal` | 3-month climate anomalies & EFI | ❌ Free |
| **Open-Meteo ECMWF** | `api.open-meteo.com/v1/ecmwf` | 16-day weather forecast ensemble | ❌ Free |
| **Open-Meteo WMO/ERA5** | `api.open-meteo.com/v1/wmo` | Hydrometeorological (soil moisture, flood risk) | ❌ Free |
| **BigDataCloud** | `api.bigdatacloud.net/data/reverse-geocode-client` | Reverse geocoding (city, region, country) | ❌ Free |
| **ip-api.com** | `ip-api.com/json` | IP-based country detection | ❌ Free |
| **World Bank** | `api.worldbank.org/v2/country` | Country names and codes | ❌ Free |
| **Wikipedia REST** | `en.wikipedia.org/w/api.php` | Product/crop thumbnail images | ❌ Free |
| **Google Gemini** | `generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash` | AI crop recommendation re-ranking | API key (Free Tier) |
| **CBSL Sri Lanka** | `cbsl.gov.lk/.../price_report_YYYYMMDD_e.pdf` | Daily retail/wholesale market prices | ❌ Free PDF |

---

## 4. Frontend Architecture

### 4.1 Component Tree

```
RootLayout (globals.css + Google Analytics)
└── LocalizationProvider
    └── HomeContent
        ├── Header
        │   ├── Logo + Title
        │   ├── AI Badge (animated pulse)
        │   ├── Currency Badge
        │   ├── LanguageSwitcher
        │   └── New Location button
        ├── Hero Section (animated)
        │   ├── Globe (floating animation)
        │   ├── Title + Description
        │   └── Feature Tags (AI, Real-time, Crops)
        ├── LocationPrompt
        │   ├── Country dropdown
        │   └── Geolocation button
        ├── AiThinkingIndicator (neural pulse animation)
        ├── SkeletonLoader (shimmer placeholders)
        ├── [Results]
        │   ├── Risk Banner
        │   ├── ClimateOverview
        │   │   ├── Temperature Anomaly
        │   │   ├── Precipitation Anomaly
        │   │   └── Signal Badges
        │   ├── ClimateCharts (Recharts)
        │   │   ├── Anomaly Bar Chart
        │   │   ├── Risk Radial Gauges
        │   │   ├── Soil Moisture Gauge
        │   │   └── Sensor Data Chart
        │   ├── PdfDownloadButton
        │   ├── RecommendationCard (× N)
        │   │   ├── Product Image (Wikipedia)
        │   │   ├── Risk Badge
        │   │   └── Daraz Affiliate Link (LK only)
        │   ├── CropRecommendationCard (× N)
        │   │   ├── Suitability Score
        │   │   ├── Tolerance Badges
        │   │   └── Harvest Timeline
        │   └── SriLankaPrices (CBSL data table)
        ├── Contact Link
        └── Footer
```

### 4.2 State Management

- **React useState** for local component state (location, data, loading, error)
- **LocalizationContext** (React Context) for global locale/language state
- **No external state management library** — avoids unnecessary dependencies for static export

### 4.3 Animation System

All animations are pure CSS — no JavaScript animation libraries:

| Animation | Implementation |
|---|---|
| Scroll-reveal | `IntersectionObserver` + CSS transitions |
| AI neural pulse | CSS keyframes (`orbit`, `pulse-glow`) |
| Skeleton shimmer | CSS linear-gradient + `background-position` animation |
| Card hover lift | CSS `transform` transition |
| Image zoom on hover | CSS `scale` transformation |
| Floating globe | CSS `translateY` keyframe |
| Staggered entry | CSS `animation-delay` (incrementing per card) |
| Gradient background shift | CSS `background-position` animation |

### 4.4 Localization Architecture

```
LocalizationProvider
├── Loads locale from IP detection → country → language
├── Supports: en, es, fr, zh, si (Sinhala)
├── Dynamic translations via JSON files
└── LanguageSwitcher component (manual override)
```

---

## 5. Backend Architecture

### 5.1 Service Layer

```
ClimateService          → Open-Meteo Seasonal API (anomalies + EFI)
HydroMeteoService       → Open-Meteo WMO/ERA5 (soil moisture, flood risk)
CropRecommendationService → Rule engine + GeminiService re-ranking
GeminiService           → Google Gemini LLM API
RecommendationService   → Orchestrates all services, generates recommendations
LocationService         → ip-api.com + BigDataCloud + World Bank
CountryLocaleService    → Language/currency mapping
PriceService            → CBSL PDF download + parsing
SriLankaPriceService    → CBSL daily price PDF parsing (PdfPig)
EmailService            → Contact form → SMTP (Gmail)
```

### 5.2 API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/recommendations?lat=&lng=` | Full recommendation response (climate + items) |
| `GET` | `/api/recommendations/health` | Health check |
| `GET` | `/api/crops?lat=&lng=&countryCode=` | AI-powered crop recommendations |
| `GET` | `/api/locations/countries` | Country list (World Bank) |
| `GET` | `/api/locations/ip-country` | IP-based country detection |
| `GET` | `/api/locations/languages` | Available languages |
| `GET` | `/api/locations/locale/{code}` | Country locale (currency, language) |
| `GET` | `/api/prices/sri-lanka` | CBSL daily market prices |
| `POST` | `/api/contact` | Contact form submission → email |

### 5.3 Recommendation Pipeline

```
User Location (lat/lng)
    │
    ▼
┌─────────────────────────────────────────────┐
│ 1. Reverse Geocode (BigDataCloud)           │
│    → City, Region, Country                  │
└──────────────────────┬──────────────────────┘
                       │
┌──────────────────────▼──────────────────────┐
│ 2. Climate Analysis                         │
│    ├── Seasonal Forecast (Open-Meteo)       │
│    │   → 3-month temp/precip anomalies      │
│    │   → Extreme Forecast Index (EFI)       │
│    └── HydroMeteo (Open-Meteo WMO/ERA5)     │
│        → Soil moisture (4 depths)           │
│        → Flood risk index                   │
│        → Storm severity index               │
│        → Drought severity index             │
│        → River discharge                    │
└──────────────────────┬──────────────────────┘
                       │
┌──────────────────────▼──────────────────────┐
│ 3. Signal Detection (Rule Engine)           │
│    ├── Temperature anomaly thresholds       │
│    ├── Precipitation anomaly thresholds     │
│    ├── EFI thresholds                       │
│    └── Hydrometeorological thresholds       │
│    → El Niño / La Niña / Drought / Flood    │
│    → Heatwave / Cold Spell / Storm Risk     │
└──────────────────────┬──────────────────────┘
                       │
┌──────────────────────▼──────────────────────┐
│ 4. Item Recommendations (Rule Engine)       │
│    → Map signals → stock-up items           │
│    → Priority scoring per item              │
│    → Risk level assignment                  │
└──────────────────────┬──────────────────────┘
                       │
┌──────────────────────▼──────────────────────┐
│ 5. Crop Recommendations                     │
│    ├── Rule-based candidates (by climate)   │
│    └── Gemini LLM re-ranking                │
│        → Suitability scores                 │
│        → Plant-by / harvest dates           │
│        → Growing advice (generalAdvice)     │
└──────────────────────┬──────────────────────┘
                       │
                       ▼
         Unified Response → Frontend
```

### 5.4 Error Resilience

All HTTP clients use **Polly retry policies**:

| Service | Retries | Timeout |
|---|---|---|
| ClimateService | 2 | Default (100s) |
| PriceService | 1 | Default |
| LocationService | 1 | Default |
| HydroMeteoService | 1 | Default |
| CropRecommendationService | 1 | Default |
| GeminiService | 1 | Default |
| SriLankaPriceService | 0 | 15s |

On total API failure, the system gracefully degrades:
- Climate anomalies default to `0` with 50% confidence
- Recommendations fall back to pure rule-based (no AI re-ranking)
- Frontend shows error state with retry button

---

## 6. External API Integrations — Deep Dive

### 6.1 Open-Meteo Seasonal Forecast

**Purpose:** 3-month climate outlook with temperature anomalies, precipitation anomalies, and Extreme Forecast Index (EFI).

**Endpoint:** `https://seasonal-api.open-meteo.com/v1/seasonal`

**Parameters:**
```
?latitude={lat}&longitude={lng}
&daily=temperature_2m_mean,precipitation_sum
&weekly=temperature_2m_anomaly,precipitation_anomaly,temperature_2m_efi,precipitation_efi
&timezone=auto
&forecast_months=3
```

**Response data:** Weekly temperature anomaly (K), precipitation anomaly (mm), EFI for both variables, plus 51 ensemble members.

### 6.2 Google Gemini LLM

**Purpose:** Re-rank crop recommendations beyond simple rule matching. The AI evaluates nuanced suitability based on crop-specific climate tolerances against forecasted conditions.

**Model:** `gemini-2.0-flash` (Free Tier)

**Prompt engineering:** Structured prompt with location context, climate anomalies, signal detections, and crop database → returns JSON array with suitability scores and growing advice.

**Graceful degradation:** If the Gemini API is unavailable (no key configured or quota exhausted), the system falls back to rule-based crop scoring using hardcoded tolerance thresholds.

### 6.3 CBSL Daily Price Report

**Purpose:** Deliver real-time retail and wholesale market prices from 5 Sri Lankan markets.

**Integration:** 
- Downloads the Central Bank of Sri Lanka's daily PDF report
- Parses with **PdfPig** (Apache-2.0 licensed .NET PDF library)
- Regex-based extraction handles the compact text format (no whitespace between tokens)
- Covers 32+ commodities across 5 markets

**Markets tracked:** Dambulla, Nuwara Eliya, Kandy, Pettah, Jaffna

### 6.4 Wikipedia Product Images

**Purpose:** Display actual product photos alongside recommendations for visual appeal and trust.

**Integration:**
- Wikipedia REST API (`prop=pageimages&pithumbsize=400`)
- 40+ item name → Wikipedia page title mappings
- In-memory cache prevents redundant API calls
- Graceful emoji fallback on missing images

### 6.5 Location Services

| API | Role |
|---|---|
| **ip-api.com** | Free IP geolocation (no key required) |
| **BigDataCloud** | Reverse geocoding: lat/lng → city, region, country |
| **World Bank** | Country names, codes, and regional data |

---

## 7. AI/ML Integration

### 7.1 Hybrid Recommendation Architecture

```
┌──────────────────────────────────────────────────┐
│                 Recommendation Engine              │
├──────────────────────┬───────────────────────────┤
│   Deterministic      │      AI (Gemini)          │
│   Rule Engine        │      LLM Re-ranking       │
├──────────────────────┼───────────────────────────┤
│ Signal detection     │ Crop suitability scoring  │
│ Item mapping         │ Growing advice generation │
│ Priority calculation │ Harvest timeline          │
│ Risk level           │ Nuanced climate reasoning │
└──────────────────────┴───────────────────────────┘
```

### 7.2 Signal Detection Rules

```python
# Simplified decision logic
if temp_anomaly > 1.5°C and precip_anomaly < -20mm → El Niño
if temp_anomaly < -1.0°C and precip_anomaly > 30mm → La Niña
if precip_anomaly < -30mm → Drought
if precip_anomaly > 40mm → Heavy Rainfall
if EFI_temperature > 0.7 → Heatwave
if temp_anomaly < -3.0°C → Cold Spell
if flood_risk_index > 50 → Flood Risk
if storm_severity > 50 → Storm Risk
if drought_severity > 60 → Extreme Drought
```

### 7.3 AI Prompt Structure (Gemini)

The LLM prompt includes:
- Location context (country, climate zone)
- Current climate anomalies (temperature, precipitation)
- Detected signals
- Available crops with their tolerance profiles
- Instruction to return `JSON` with scores 0-100, dates, and advice

---

## 8. Data Flow

### 8.1 Complete Request Lifecycle

```
1. USER visits climate-survival.netlify.app
2. JavaScript loads → detects IP country (ip-api.com)
3. Sets locale (language + currency) from country
4. USER selects location (dropdown or geolocation)
5. Frontend calls /api/recommendations?lat=X&lng=Y
   └── Netlify proxies to HF Space backend
6. Backend orchestrates:
   ├── BigDataCloud reverse geocode
   ├── Open-Meteo Seasonal API (anomalies)
   ├── Open-Meteo WMO/ERA5 (hydrometeo)
   ├── Rule engine → signals + items
   └── Gemini AI → crop re-ranking
7. Returns unified JSON response
8. Frontend renders:
   ├── ClimateOverview (anomalies + signals)
   ├── ClimateCharts (Recharts visualizations)
   ├── RecommendationCards (with Wikipedia images)
   └── CropRecommendationCards (with AI scores)
9. USER can:
   ├── Download PDF report (jsPDF + html2canvas)
   ├── Switch language (en/es/fr/zh/si)
   ├── View CBSL prices (Sri Lanka only)
   ├── Click Daraz affiliate links (Sri Lanka only)
   └── Contact via form (→ Gmail SMTP)
```

---

## 9. Deployment Architecture

### 9.1 Infrastructure Diagram

```
                    ┌─────────────────────┐
                    │   GitHub Repository │
                    │  dkcngomes/         │
                    │  climate-survival   │
                    └────────┬────────────┘
                             │
              ┌──────────────┼──────────────────┐
              │              │                   │
              ▼              ▼                   ▼
     ┌────────────────┐ ┌────────────┐ ┌────────────────────┐
     │  origin        │ │netlify-repo│ │  hf                │
     │  GitHub        │ │ GitHub     │ │ Hugging Face       │
     │  (primary)     │ │ (mirror)   │ │ Spaces             │
     └────────────────┘ └──────┬─────┘ └──────────┬─────────┘
                               │                  │
                               ▼                  ▼
                      ┌────────────────┐ ┌────────────────────┐
                      │   Netlify      │ │ Docker Container   │
                      │  (Static)      │ │ .NET 9 API         │
                      │  Frontend      │ │ Port 7860          │
                      └────────────────┘ └────────────────────┘
                               │                  │
                               └──────┬───────────┘
                                      ▼
                            ┌──────────────────┐
                            │  climate-survival│
                            │  .netlify.app    │
                            │  (via proxy)     │
                            └──────────────────┘
```

### 9.2 Deployment Workflow

```
Developer commits → git push (all 3 remotes)
    │
    ├── origin → GitHub (code backup)
    ├── netlify-repo → Netlify auto-build + deploy
    │   ├── npm install
    │   ├── npm run build (static export)
    │   └── Publish out/ to CDN
    └── hf → Hugging Face auto-build + deploy
        ├── Docker build
        └── Start container (port 7860)
```

### 9.3 Cost Analysis

| Service | Monthly Cost | Limits |
|---|---|---|
| Netlify (Free) | $0 | 100GB bandwidth, 300 build minutes |
| HF Spaces (Free) | $0 | 2 vCPU, 16GB RAM, 50GB storage |
| Open-Meteo APIs | $0 | No rate limit documented |
| Google Gemini Free | $0 | 60 requests/minute, 1,500/day |
| ip-api.com (Free) | $0 | 45 requests/minute (non-commercial) |
| BigDataCloud (Free) | $0 | 1,000 requests/day |
| Wikipedia API | $0 | No rate limit |
| CBSL PDF | $0 | Public government data |
| **Total** | **$0** | Fully operational at zero cost |

---

## 10. Monetization Strategy

### 10.1 Daraz Affiliate Program (Sri Lanka)

- **Partner:** Daraz Lanka (Alibaba Group subsidiary)
- **Member ID:** `155412816`
- **Mechanism:** Affiliate links on product recommendations
- **Visibility:** Only shown when `countryCode === "LK"`
- **Compliance:** FTC disclosure "As a Daraz Affiliate we earn from qualifying purchases"

### 10.2 Expansion Opportunities

| Channel | Description | Timeline |
|---|---|---|
| **Amazon Associates** | Expand to US/UK/EU markets via Amazon affiliate links | Near-term |
| **Local affiliate programs** | Partner with regional e-commerce platforms (Flipkart, Shopee, etc.) | Medium-term |
| **Premium tier** | Extended forecasts, SMS alerts, API access for businesses | Long-term |
| **B2B agriculture** | Crop insurance risk assessment, supply chain planning | Long-term |

---

## 11. Localization System

### 11.1 Supported Languages

| Code | Language | Script |
|---|---|---|
| `en` | English | Latin |
| `es` | Spanish | Latin |
| `fr` | French | Latin |
| `zh` | Chinese | Han |
| `si` | Sinhala | Sinhala |

### 11.2 Locale Detection Flow

```
IP Address → ip-api.com → Country Code
    │
    ▼
CountryLocaleService → Default Language + Currency
    │
    ├── en → USD
    ├── LK → Sinhala / LKR
    ├── FR → French / EUR
    └── etc.
    │
    ▼
LocalizationContext (React Context)
    ├── Translatable text
    ├── Currency symbol
    └── Manual override via LanguageSwitcher
```

---

## 12. Security Considerations

| Area | Implementation |
|---|---|
| **HTTPS** | Netlify automatic TLS (Let's Encrypt) |
| **API proxying** | No direct API exposure; all traffic through Netlify → HF Space |
| **CORS** | Wide-open policy (AllowAnyOrigin) — acceptable for public API |
| **Email SMTP** | Gmail App Passwords (16-char) stored as HF Space secrets |
| **Gemini API key** | Stored as HF Space secret environment variable |
| **No user accounts** | No authentication, no PII stored, no database |
| **Contact form** | No persistent storage; forwarded directly to email |

---

## 13. Performance & Monitoring

### 13.1 Performance Characteristics

| Metric | Expected | Worst Case |
|---|---|---|
| Initial page load | < 1.5s (CDN cached) | < 3s |
| API response time | 3-5s (aggregates 3 APIs) | 15s (with Polly retries) |
| PDF generation | < 2s (client-side) | < 5s |
| CSS/JS bundle | ~80KB | ~120KB |
| Lighthouse score | 85+ | 75+ |

### 13.2 Monitoring

- **Google Analytics** (G-ECE9GWGK7E): user engagement, page views, geography
- **HF Space logs**: API errors, response times, build failures
- **Netlify analytics**: bandwidth, deploy status, CDN performance

---

## 14. Scalability Roadmap

### 14.1 Immediate (Next 3 Months)

- [x] Fix Open-Meteo Seasonal API variable names (completed)
- [x] Rich animation system for user engagement (completed)
- [ ] Expand product image coverage (more Wikipedia mappings)
- [ ] Add Amazon Associates integration for non-LK users
- [ ] Performance optimization (image lazy loading, bundle splitting)

### 14.2 Short-Term (3-6 Months)

- [ ] User accounts with saved locations and watchlists
- [ ] Email/SMS alerts for critical climate signals
- [ ] Mobile app (React Native or PWA)
- [ ] Historical climate data comparison
- [ ] Community-contributed crop data

### 14.3 Medium-Term (6-12 Months)

- [ ] Premium subscription tier
- [ ] B2B API for agricultural businesses
- [ ] Machine learning models (vs. rule engine) for item recommendations
- [ ] Integration with weather station networks
- [ ] Expanded market price data (India, Bangladesh, Vietnam)

### 14.4 Scaling Considerations

| Bottleneck | Solution |
|---|---|
| Free tier rate limits | Upgrade to paid API tiers ($5-20/month) |
| Single-region API | Multi-region deployments via Fly.io or Railway |
| No database | Add PostgreSQL for user accounts/history |
| Monolithic backend | Split into microservices (recommendation, pricing, auth) |
| Static export limits | Move to Next.js SSR with serverless functions |

---

## 15. Competitive Advantages

| Advantage | Climate Survival | Competitors |
|---|---|---|
| **Cost to operate** | **$0/month** (all free APIs + hosting) | $50-500/month typical |
| **AI integration** | Gemini LLM-powered crop re-ranking | Rule-based only |
| **Local market data** | CBSL daily prices (Sri Lanka) | None |
| **Localization** | 5 languages + auto locale detection | English-only typically |
| **Monetization** | Affiliate commerce (non-intrusive) | Ads or subscriptions |
| **Deployment speed** | Push-to-deploy (under 5 min) | Complex CI/CD pipelines |
| **Tech stack** | Modern (Next.js 16, .NET 9) | Often legacy |
| **Data sources** | 8+ integrated free APIs | 1-2 paid sources |

---

## 16. Appendix: API Reference

### Full API Endpoint Documentation

| Endpoint | Method | Parameters | Response |
|---|---|---|---|
| `/api/recommendations` | GET | `lat`, `lng` | `RecommendationResponse { forecast, recommendations, generatedAt, overallRiskLevel }` |
| `/api/crops` | GET | `lat`, `lng`, `countryCode` (opt) | `CropRecommendationResponse { crops[], generalAdvice, totalCrops, generatedAt }` |
| `/api/locations/countries` | GET | — | `CountryInfo[]` |
| `/api/locations/ip-country` | GET | — | `IpCountryResult { ip, country, countryCode, locale }` |
| `/api/locations/languages` | GET | — | `{ code, name }[]` |
| `/api/locations/locale/{code}` | GET | `code` (path) | `CountryLocale { language, currencyCode, currencySymbol, locale }` |
| `/api/prices/sri-lanka` | GET | — | `SriLankaPricesResponse { date, prices[] }` |
| `/api/recommendations/health` | GET | — | `{ status, timestamp }` |
| `POST /api/contact` | POST | `{ name, email, subject, message }` | `{ success, messageId }` |

---

## Document Information

**Author:** Nipuna Gomes  
**Contact:** [climate-survival.netlify.app/contact](https://climate-survival.netlify.app/contact)  
**Repository:** [github.com/dkcngomes/climate-survival](https://github.com/dkcngomes/climate-survival)  
**License:** Proprietary

---

*This document reflects the architecture as of June 29, 2026.*
