# 🚀 Deploy Backend to Hugging Face Spaces (Free, No Credit Card)

## Overview

```
Netlify (frontend, free)          Hugging Face Space (backend, free)
  climate-survival.netlify.app ──── {username}-climate-survival-api.hf.space
       │                                    │
       └── proxy /api/* ────────────────────┘
```

---

## Step 1: Create the Space on Hugging Face

1. Go to **[huggingface.co](https://huggingface.co)** and sign up (GitHub or email — **no credit card needed**)

2. Click your profile picture → **New Space**

3. Fill in:

   | Field | Value |
   |-------|-------|
   | **Space Name** | `climate-survival-api` |
   | **License** | `mit` |
   | **Space SDK** | **Docker** |
   | **Hardware** | **CPU basic** (free) |
   | **Space Type** | Public |

4. Click **Create Space**

5. After creation, you'll see a page with a Git URL. Copy it — it looks like:
   ```
   https://huggingface.co/spaces/YOUR_USERNAME/climate-survival-api
   ```

---

## Step 2: Push Backend Code to the Space

Run this command in your terminal **from the project root**:

```bash
# Clone the Hugging Face Space repo (you'll be prompted for your HF token)
git clone https://huggingface.co/spaces/YOUR_USERNAME/climate-survival-api
cd climate-survival-api

# Copy all backend files into the Space repo
xcopy /E /I ..\climate-advisor\backend\* .
# (or on Linux/macOS: cp -r ../climate-advisor/backend/* .)

# Hugging Face reads this file for metadata
echo. > README.md

# Add everything and push
git add .
git commit -m "Initial deploy: Climate Survival API"
git push
```

> **Or use the automated script below.**

---

## Step 3: Update Netlify Proxy

After the Space deploys (takes ~3-5 min for first build), your API will be at:

```
https://YOUR_USERNAME-climate-survival-api.hf.space
```

Open `netlify.toml` in this repo and replace `YOUR_HF_USERNAME` with your actual Hugging Face username.

Then commit and push — Netlify auto-deploys.

---

## Step 4: Set Environment Variables

Hugging Face Spaces use `appsettings.json` by default. For secrets, add **Repository Secrets** in your HF Space:

1. Go to your Space → **Settings** → **Repository Secrets**
2. Add these:

   | Key | Value | Required? |
   |-----|-------|-----------|
   | `GEMINI__APIKEY` | *(your Gemini key)* | Optional (rules work without it) |
   | `EMAIL__SMTPPASS` | *bmqb pict nkjg dwom* | Optional (for contact form email) |

3. Then click **New variable** for each key.

> Note: HF Spaces inject secrets as environment variables. .NET parses `GEMINI__APIKEY` as `Gemini:ApiKey` automatically.
