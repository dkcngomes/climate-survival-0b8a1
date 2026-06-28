// In production (Netlify), NEXT_PUBLIC_API_URL is not set → use relative URLs
// Netlify proxies /api/* → Render backend automatically.
// In local dev, set NEXT_PUBLIC_API_URL=http://localhost:8080 in .env.local
const API_BASE = process.env.NEXT_PUBLIC_API_URL || "";

import type {
  RecommendationResponse,
  CropRecommendationResponse,
  CountryInfo,
  CountryLocale,
  IpCountryResult,
} from "@/types";

export async function fetchRecommendations(lat: number, lng: number) {
  const res = await fetch(`${API_BASE}/api/recommendations?lat=${lat}&lng=${lng}`);
  if (!res.ok) throw new Error(`Recommendations failed: ${res.statusText}`);
  return res.json() as Promise<RecommendationResponse>;
}

export async function fetchCropRecommendations(lat: number, lng: number, countryCode?: string) {
  let url = `${API_BASE}/api/crops?lat=${lat}&lng=${lng}`;
  if (countryCode) url += `&countryCode=${countryCode}`;
  const res = await fetch(url);
  if (!res.ok) {
    const err = await res.json().catch(() => ({ error: "Crop request failed" }));
    throw new Error(err.error || `HTTP ${res.status}`);
  }
  return res.json();
}

export async function fetchCountries() {
  const res = await fetch(`${API_BASE}/api/locations/countries`);
  if (!res.ok) throw new Error(`Countries failed: ${res.statusText}`);
  return res.json() as Promise<CountryInfo[]>;
}

export async function detectIpCountry() {
  try {
    const res = await fetch(`${API_BASE}/api/locations/ip-country`);
    if (!res.ok) return null;
    return res.json() as Promise<IpCountryResult>;
  } catch {
    return null;
  }
}

export async function fetchLanguages() {
  const res = await fetch(`${API_BASE}/api/locations/languages`);
  if (!res.ok) throw new Error(`Languages failed: ${res.statusText}`);
  return res.json() as Promise<{ code: string; name: string }[]>;
}

export async function fetchCountryLocale(countryCode: string) {
  const res = await fetch(`${API_BASE}/api/locations/locale/${countryCode}`);
  if (!res.ok) throw new Error(`Locale failed: ${res.statusText}`);
  return res.json() as Promise<CountryLocale>;
}

export interface ContactPayload {
  name: string;
  email: string;
  subject: string;
  message: string;
}

export async function submitContact(payload: ContactPayload) {
  const res = await fetch(`${API_BASE}/api/contact`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ error: "Submission failed" }));
    throw new Error(err.error || `HTTP ${res.status}`);
  }
  return res.json() as Promise<{ success: boolean; messageId: string }>;
}
