<# .SYNOPSIS
    Deploy the backend to a Hugging Face Space.
.DESCRIPTION
    This script clones a HF Space repo, copies backend files,
    and pushes. Run this AFTER creating the Space on huggingface.co.
.EXAMPLE
    .\scripts\deploy-hf-space.ps1 -HfSpaceUrl "https://huggingface.co/spaces/yourname/climate-survival-api"
.NOTES
    You need a Hugging Face User Access Token for Git authentication.
    Generate one at: https://huggingface.co/settings/tokens
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$HfSpaceUrl,

    [Parameter(Mandatory = $false)]
    [string]$TempDir = "$env:TEMP\hf-space-deploy"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$BackendDir = Join-Path $ProjectRoot "backend"

Write-Host "🚀 Deploying Climate Survival API to Hugging Face Space" -ForegroundColor Cyan
Write-Host ""

# Clean temp dir if exists
if (Test-Path $TempDir) {
    Remove-Item -Recurse -Force $TempDir
}

Write-Host "📦 Cloning HF Space repo..." -ForegroundColor Yellow
git clone $HfSpaceUrl $TempDir

if (-not (Test-Path $TempDir)) {
    Write-Error "Failed to clone. Check your HF token and URL."
    exit 1
}

Write-Host "📋 Copying backend files..." -ForegroundColor Yellow
# Copy everything except bin/obj
Get-ChildItem -Path $BackendDir -Exclude "bin", "obj" | Copy-Item -Destination $TempDir -Recurse -Force

# Copy the HF-specific .gitignore
Copy-Item (Join-Path $ProjectRoot "scripts\hf-space.gitignore") (Join-Path $TempDir ".gitignore")

Write-Host "📝 Creating README.md for HF Space..." -ForegroundColor Yellow
@"
---
title: Climate Survival API
emoji: 🌍
colorFrom: green
colorTo: emerald
sdk: docker
app_port: 7860
license: mit
---

# Climate Survival API

Backend API for Climate Survival — climate-adaptive crop & stock-up recommendations.

- .NET 9
- Open-Meteo, World Bank, Wikipedia APIs
- Gemini LLM reranking (optional)

## Environment Variables (Repository Secrets)

| Key | Description |
|-----|-------------|
| `GEMINI__APIKEY` | Google Gemini API key (optional) |
| `EMAIL__SMTPPASS` | Gmail app password for contact form (optional) |
"@ | Set-Content -Path (Join-Path $TempDir "README.md") -Encoding UTF8

Set-Location $TempDir

Write-Host "📤 Pushing to Hugging Face Space..." -ForegroundColor Yellow
git add -A
git commit -m "Initial deploy: Climate Survival API"
git push

Set-Location $ProjectRoot

# Cleanup
Remove-Item -Recurse -Force $TempDir

Write-Host ""
Write-Host "✅ Deployed successfully!" -ForegroundColor Green
Write-Host "Your API is at: $($HfSpaceUrl -replace 'huggingface.co/spaces', '$&.hf.space')" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next step: Update netlify.toml with the actual URL above." -ForegroundColor Yellow
