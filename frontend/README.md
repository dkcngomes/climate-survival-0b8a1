# Climate Survival — Frontend

Next.js 16 app with Tailwind CSS, static-exported to Netlify.

## Features

- Climate risk overview with interactive Recharts charts
- Stock-up recommendations based on seasonal climate forecasts
- Crop recommendations with AI re-ranking (optional Gemini)
- Sri Lanka CBSL market prices (LK users only)
- Daraz affiliate links (LK users only)
- Multi-language support (en, si, es, fr, zh)
- PDF survival report download
- Contact form
- Google Analytics

## Local Development

```bash
npm install
npm run dev
```

Set `NEXT_PUBLIC_API_URL=http://localhost:8080` in `.env.local` to use local backend.

## Build

```bash
npm run build
# Output in out/ — ready for static hosting
```

## Deployment

Auto-deploys on Netlify when pushing to `dkcngomes/climate-survival-0b8a1`.
