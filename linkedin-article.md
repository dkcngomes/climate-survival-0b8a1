# I Built a Full-Stack AI App for $0/Month — Here's the Architecture That Makes It Possible

**By Nipuna C. Gomes**

---

A few weeks ago, I couldn't sleep.

I kept thinking about something: we have all this climate data floating around — global models, weather forecasts, even daily market prices published by governments. But nobody connects the dots for the average person.

Like, what should I buy at the grocery store today because next month's weather will make it twice as expensive? Should I plant rice this season or switch to something more drought-resistant?

The data is out there. It's just scattered across a dozen different websites, formats, and APIs.

So I built something to fix that.

**[climate-survival.netlify.app](https://climate-survival.netlify.app)** | **[github.com/dkcngomes/climate-survival](https://github.com/dkcngomes/climate-survival)**

Let me walk you through how it works — and why it costs me absolutely nothing to run.

---

## The problem I was trying to solve

Back home in Sri Lanka, I've seen how unpredictable weather hits people.

Farmers wake up not knowing if the monsoon will flood their fields or skip them entirely. Families see rice prices double overnight after a bad harvest and wonder if they should have stocked up earlier. Shop owners watch supply chains break and can't do anything about it.

There's no shortage of data. The European weather models are free. The Sri Lanka Central Bank publishes price reports every day. Wikipedia has images of almost every product you can name.

The problem is **connecting them** in a way that actually helps someone decide what to do.

I wanted to build a tool that does exactly that — and I wanted to prove you don't need a big budget to do it.

---

## The stack (spoiler: it costs zero dollars)

Here's what powers the whole thing:

**Frontend:** Next.js + React + TypeScript, deployed as a static site on Netlify's free tier. Tailwind for styling. Recharts for the climate graphs. jsPDF for generating survival reports you can download. Zero server costs.

**Backend:** .NET 9 Web API, running inside a Docker container on Hugging Face Spaces. Free tier gives me 2 CPU cores, 16GB RAM, and 50GB of storage. No credit card needed.

**AI layer:** Google Gemini 2.0 Flash (free tier) for re-ranking crop recommendations. If it's down or unavailable, the system falls back to the rule engine. No single point of failure.

**The data sources (all free):**

- Open-Meteo — climate forecasts, weather anomalies, soil moisture, flood risk
- ip-api.com — detects your country from your IP
- BigDataCloud — figures out what city you're in
- World Bank — country codes and names
- Wikipedia — product images
- Sri Lanka Central Bank — daily market prices for 32 different commodities across 5 markets
- Google Gemini — AI recommendations

**Total monthly bill: $0.00.**

---

## How the recommendation engine actually works

I didn't want to go pure AI — too expensive and unreliable. And I didn't want pure rules — too rigid. So I built a hybrid.

**Step one:** The system pulls 3-month climate anomaly data from Open-Meteo. Then a rule engine checks for patterns:

- Temperature spike + low rainfall? That's El Niño territory → stock up on dry goods, drought-resistant grains
- Extreme precipitation forecast? → flood risk → stock waterproofing supplies, plant short-duration rice
- High Extreme Forecast Index? → heatwave incoming

**Step two:** Each detected signal maps to specific products and crops. This is based on agricultural research and common sense — nothing fancy.

**Step three:** The rule engine's output gets sent to Gemini for a second pass. The AI considers local conditions, growing seasons, and compounding factors, then produces a ranked list with suitability scores, planting dates, and practical advice.

The whole chain works even if the AI is unavailable. The rules hold the fort.

---

## The hardest part was parsing a government PDF

This is going to sound ridiculous after all that AI talk, but the single hardest technical challenge was parsing the daily price report from the Sri Lanka Central Bank.

Every day, they publish a PDF with prices for 32 items across 5 markets. Sounds straightforward.

PdfPig (the .NET library) extracts text in "compact mode" — meaning no spaces between words. I spent **two days** fine-tuning regex patterns to correctly identify "Coconut (Rs.Unit)85.0075.00" as "Coconut: Rs. 85.00 (retail), Rs. 75.00 (wholesale)."

It works now. It runs silently in production. But I have a new appreciation for anyone who's ever had to parse a government PDF in production.

---

## Localization was non-negotiable

The app auto-detects your language based on your country. You can switch anytime between English, Spanish, French, Chinese, and **Sinhala** — because this was built from Sri Lanka, and it had to work for Sri Lankans first.

---

## The monetization (yes, it makes money)

Running on $0 doesn't mean it can't earn.

For users in Sri Lanka, product recommendations include links to Daraz (it's like Amazon for South Asia). When someone clicks and buys, I earn a commission. There's a clear disclosure — nothing shady.

The affiliate links only show up for Sri Lankan users. Everyone else gets a clean, ad-free experience.

Next up: Amazon Associates for global users, and eventually a premium tier with SMS alerts and extended forecasts.

---

## What 15+ years of building software taught me on this project

**Free tiers are genuinely good now.** Hugging Face Spaces gives you Docker with 16GB RAM for nothing. Netlify gives you 100GB bandwidth and auto HTTPS for nothing. Open-Meteo has no rate limits. The idea that you need a paid cloud account to run a production app is just not true anymore.

**Hybrid AI beats pure AI.** A pure LLM pipeline is expensive and slow. A pure rules engine is brittle. Together they cover each other's weaknesses.

**Government PDFs will humble you.** I've built microservices handling a million requests a day. I've migrated monoliths to event-driven architectures. None of that prepared me for parsing a Central Bank price report.

**Build for where you are.** I'm in Colombo. My audience is Sri Lankan. The Sinhala translations, the local market data, the Daraz integration — none of it would exist if I'd built a generic global app. The specificity is the whole point.

---

## What's next

- Better product images for all 40+ commodities
- Amazon Associates for non-Sri Lankan users
- User accounts with saved locations and email alerts
- A mobile-friendly PWA version
- Eventually a B2B API for agricultural businesses

---

## The point

I built Climate Survival because I wanted to show that useful, production-grade software doesn't require a cloud budget, a DevOps team, or venture capital.

It just needs solid engineering, free public data, and a clear problem to solve.

**Give it a try: [climate-survival.netlify.app](https://climate-survival.netlify.app)**

**Check the code: [github.com/dkcngomes/climate-survival](https://github.com/dkcngomes/climate-survival)**

Would love to hear what you think — especially if you've built something similar, or if you see where this could go next.

Drop a comment or just say hi. Let's build cool stuff.

---

*Nipuna C. Gomes | Tech Lead & Software Engineer | Colombo, Sri Lanka*

🔗 [linkedin.com/in/nipuna-gomes](https://www.linkedin.com/in/nipuna-gomes)
🌐 [nipunadk.netlify.app](https://nipunadk.netlify.app)

*#ClimateTech #OpenSource #DotNet #NextJS #AWS #ZeroCost #SriLanka*
