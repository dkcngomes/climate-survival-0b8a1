"use client";

import { useState, useEffect } from "react";
import { LocationState, RecommendationResponse, CropRecommendationResponse } from "@/types";
import { fetchRecommendations, fetchCropRecommendations } from "@/services/api";
import { LocalizationProvider, useLocalization } from "@/i18n/LocalizationContext";
import LanguageSwitcher from "@/i18n/LanguageSwitcher";
import LocationPrompt from "@/components/LocationPrompt";
import ClimateOverview from "@/components/ClimateOverview";
import ClimateCharts from "@/components/ClimateCharts";
import PdfDownloadButton from "@/components/PdfDownloadButton";
import RecommendationCard from "@/components/RecommendationCard";
import CropRecommendationCard from "@/components/CropRecommendationCard";
import SriLankaPrices from "@/components/SriLankaPrices";
import AlertSubscription from "@/components/AlertSubscription";
import AnimatedSection from "@/components/AnimatedSection";
import AiThinkingIndicator from "@/components/AiThinkingIndicator";
import {
  RecommendationSkeleton,
  CropSkeleton,
  OverviewSkeleton,
  ChartsSkeleton,
} from "@/components/SkeletonLoader";
import Link from "next/link";

function HomeContent() {
  const { t, setLocaleFromCountry, locale } = useLocalization();

  const [location, setLocation] = useState<LocationState>({
    lat: 0,
    lng: 0,
    status: "idle",
  });

  const [data, setData] = useState<RecommendationResponse | null>(null);
  const [cropData, setCropData] = useState<CropRecommendationResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedCountry, setSelectedCountry] = useState<string | undefined>();

  // On mount, set currency from IP-detected country
  useEffect(() => {
    (async () => {
      try {
        const { detectIpCountry } = await import("@/services/api");
        const result = await detectIpCountry();
        if (result?.locale?.countryCode) {
          setLocaleFromCountry(result.locale.countryCode);
        }
      } catch {
        // silently fail
      }
    })();
  }, [setLocaleFromCountry]);

  const handleLocationReady = async (lat: number, lng: number, countryCode?: string) => {
    // Scroll to top so user sees loading state and results
    window.scrollTo({ top: 0, behavior: "smooth" });

    setLocation({ lat, lng, status: "success" });
    setSelectedCountry(countryCode);
    if (countryCode) {
      setLocaleFromCountry(countryCode);
    }
    await fetchData(lat, lng, countryCode);
  };

  const fetchData = async (lat: number, lng: number, countryCode?: string) => {
    setLoading(true);
    setError(null);
    setData(null);
    setCropData(null);

    try {
      const [result, crops] = await Promise.all([
        fetchRecommendations(lat, lng),
        fetchCropRecommendations(lat, lng, countryCode),
      ]);
      setData(result);
      setCropData(crops);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch data");
    } finally {
      setLoading(false);
    }
  };

  const handleReset = () => {
    setLocation({ lat: 0, lng: 0, status: "idle" });
    setData(null);
    setCropData(null);
    setError(null);
  };

  const riskBadge = (level: string) => {
    const colors: Record<string, string> = {
      Critical: "bg-red-100 text-red-800 border-red-300",
      High: "bg-orange-100 text-orange-800 border-orange-300",
      Medium: "bg-yellow-100 text-yellow-800 border-yellow-300",
      Low: "bg-green-100 text-green-800 border-green-300",
    };
    return colors[level] || colors.Low;
  };

  // Currency display in header
  const currencyLabel = `${locale.currencyCode} (${locale.currencySymbol})`;

  return (
    <div className="min-h-screen animate-gradient bg-gradient-to-br from-emerald-50 via-white to-sky-50">
      {/* Header */}
      <header className="border-b border-gray-200 bg-white/80 backdrop-blur-sm sticky top-0 z-10">
        <div className="max-w-5xl mx-auto px-4 py-3 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <span className="text-3xl animate-float">🌍</span>
            <div>
              <h1 className="text-xl font-bold text-gray-900">{t("app.title")}</h1>
              <p className="text-xs text-gray-700">{t("app.subtitle")}</p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            {/* AI badge */}
            <span className="text-xs px-2 py-1 rounded-full bg-gradient-to-r from-emerald-500 to-emerald-600 text-white font-semibold shadow-sm animate-pulse-soft flex items-center gap-1">
              <span className="text-[10px]">✨</span>
              AI
            </span>
            {/* Currency badge */}
            <span className="text-xs px-2 py-1 rounded-full bg-gray-100 text-gray-700 font-medium border border-gray-200">
              {currencyLabel}
            </span>
            {/* Language switcher */}
            <LanguageSwitcher />
            {data && (
              <button
                onClick={handleReset}
                className="px-4 py-1.5 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg text-gray-800 transition-all border border-gray-200 btn-glow"
              >
                🔄 {t("app.newLocation")}
              </button>
            )}
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-5xl mx-auto px-4 py-8">
        {location.status === "idle" && !data && (
          <AnimatedSection>
            <div className="py-12">
              <div className="text-center mb-8">
                <span className="text-6xl block mb-6 animate-float">🌍</span>
                <h2 className="text-4xl font-extrabold text-gray-900 mb-3">
                  🌱 {t("hero.title")}
                </h2>
                <p
                  className="text-lg text-gray-800 max-w-2xl mx-auto"
                  dangerouslySetInnerHTML={{ __html: t("hero.description") }}
                />
                <div className="flex items-center justify-center gap-4 mt-4 text-sm text-gray-500">
                  <span className="flex items-center gap-1">🧠 <span>AI-Powered</span></span>
                  <span>•</span>
                  <span className="flex items-center gap-1">📊 <span>Real-time Data</span></span>
                  <span>•</span>
                  <span className="flex items-center gap-1">🌾 <span>Crop Planning</span></span>
                </div>
              </div>
              <LocationPrompt
                onLocationReady={handleLocationReady}
                location={location}
              />
            </div>
          </AnimatedSection>
        )}

        {loading && (
          <div>
            {/* AI Thinking Header */}
            <AiThinkingIndicator />

            {/* Skeleton preview of content while loading */}
            <div className="space-y-6 animate-fade-in">
              <OverviewSkeleton />
              <ChartsSkeleton />

              {/* Stock-up recommendations skeleton */}
              <div>
                <div className="skeleton h-8 w-64 rounded-lg mb-4" />
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {[0, 1, 2, 3].map((i) => (
                    <RecommendationSkeleton key={i} />
                  ))}
                </div>
              </div>

              {/* Crop recommendations skeleton */}
              <div>
                <div className="skeleton h-8 w-56 rounded-lg mb-4" />
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {[0, 1, 2].map((i) => (
                    <CropSkeleton key={i} />
                  ))}
                </div>
              </div>
            </div>
          </div>
        )}

        {error && (
          <AnimatedSection animation="scale">
            <div className="text-center py-12">
              <div className="bg-red-50 border-2 border-red-200 rounded-2xl p-8 max-w-lg mx-auto">
                <p className="text-4xl mb-4 animate-float">⚠️</p>
                <h3 className="text-xl font-bold text-red-800 mb-2">{t("error.title")}</h3>
                <p className="text-red-600 mb-4">{error}</p>
                <button
                  onClick={handleReset}
                  className="px-6 py-3 bg-red-600 hover:bg-red-700 text-white font-semibold rounded-xl transition-all btn-glow"
                >
                  {t("error.tryAgain")}
                </button>
              </div>
            </div>
          </AnimatedSection>
        )}

        {data && !loading && (
          <div>
            {/* Overall risk banner */}
            <AnimatedSection animation="scale">
              <div className={`mb-6 rounded-2xl border-2 p-4 text-center ${riskBadge(data.overallRiskLevel)}`}>
                <p className="text-sm uppercase tracking-wide font-semibold">{t("risk.overall")}</p>
                <p className="text-3xl font-extrabold mt-1">{data.overallRiskLevel}</p>
                <p className="text-sm mt-1">
                  {t("risk.itemsAndCrops", {
                    items: `${data.recommendations.length} ${t("stockUp.title").toLowerCase()}`,
                    crops: `${cropData?.totalCrops ?? 0} ${t("grow.title").toLowerCase()}`,
                  })}
                </p>
              </div>
            </AnimatedSection>

            <AnimatedSection>
              <ClimateOverview forecast={data.forecast} />
            </AnimatedSection>

            {/* Climate Charts — wrapped in an id for PDF capture */}
            <AnimatedSection>
              <div id="climate-charts-section" className="mb-8">
                <ClimateCharts forecast={data.forecast} />
              </div>
            </AnimatedSection>

            {/* Forecast period info */}
            {data.forecast.forecastPeriodLabel && (
              <AnimatedSection animation="fade">
                <div className="mb-6 bg-blue-50 border border-blue-200 rounded-xl px-5 py-3 flex items-center gap-3 text-sm">
                  <span className="text-blue-500 text-lg animate-float">📊</span>
                  <div>
                    <p className="font-semibold text-gray-900">{t("forecast.horizon")}</p>
                    <p className="text-gray-800">
                      {t("forecast.horizonDesc", { period: data.forecast.forecastPeriodLabel })}
                    </p>
                  </div>
                </div>
              </AnimatedSection>
            )}

            {/* Food Storage Section */}
            {data.recommendations.length > 0 && (
              <AnimatedSection>
                <div className="mb-10">
                  <div className="flex items-start justify-between gap-4 mb-2">
                    <h2 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                      🛒 {t("stockUp.title")}
                    </h2>
                    <PdfDownloadButton data={data} cropData={cropData} countryCode={selectedCountry} />
                  </div>
                  <p className="text-gray-700 text-sm mb-1">{t("stockUp.description")}</p>
                  {selectedCountry === "LK" && (
                    <p className="text-[11px] text-gray-500 mb-6 italic">
                      🛍️ As a Daraz Affiliate we earn from qualifying purchases — helps keep this site free.
                    </p>
                  )}
                  <div className="space-y-4">
                    {data.recommendations.map((item, i) => (
                      <RecommendationCard key={`${item.itemName}-${i}`} item={item} index={i} countryCode={selectedCountry} />
                    ))}
                  </div>
                </div>
              </AnimatedSection>
            )}

            {/* No Storage message — always before Grow section */}
            {data.recommendations.length === 0 && (
              <AnimatedSection animation="scale">
                <div className="mb-10 bg-green-50 border-2 border-green-200 rounded-2xl p-8 text-center">
                  <p className="text-4xl mb-4 animate-float">✅</p>
                  <h3 className="text-xl font-bold text-green-800 mb-2">{t("noStorage.title")}</h3>
                  <p className="text-gray-800 max-w-lg mx-auto">{t("noStorage.description")}</p>
                </div>
              </AnimatedSection>
            )}

            {/* Crop / Growing Section */}
            {cropData && cropData.crops.length > 0 && (
              <div className="mb-10">
                <h2 className="text-2xl font-bold text-gray-900 mb-2 flex items-center gap-2">
                  🌱 {t("grow.title")}
                </h2>
                <p className="text-gray-700 text-sm mb-6">{t("grow.description")}</p>

                {cropData.generalAdvice && (
                  <div className="mb-6 bg-amber-50 border border-amber-200 rounded-xl px-5 py-4 text-sm">
                    <div className="flex items-start gap-3">
                      <span className="text-amber-600 text-lg shrink-0 mt-0.5">🌿</span>
                      <div>
                        <p className="font-semibold text-gray-900 mb-1">{t("grow.strategy")}</p>
                        <p className="text-gray-800">{cropData.generalAdvice}</p>
                      </div>
                    </div>
                  </div>
                )}

                <div className="space-y-4">
                  {cropData.crops.map((crop, i) => (
                    <CropRecommendationCard key={`${crop.cropName}-${i}`} crop={crop} index={i} />
                  ))}
                </div>
              </div>
            )}

            {/* Sri Lanka Market Prices (CBSL) — only for LK users */}
            <AnimatedSection>
              <div className="mt-10 mb-4">
                <SriLankaPrices countryCode={selectedCountry} />
              </div>
            </AnimatedSection>

            {/* SMS Alerts */}
            <AnimatedSection>
              <div className="mt-10 mb-4">
                <AlertSubscription
                  latitude={location.lat}
                  longitude={location.lng}
                  locationName={data.forecast.locationName}
                  countryCode={selectedCountry}
                />
              </div>
            </AnimatedSection>

            <AnimatedSection animation="fade">
              <div className="mt-8 text-center text-xs text-gray-600 border-t border-gray-200 pt-6">
                {t("footer.dataSources")}
                <br />
                {t("footer.generated", { date: new Date(data.generatedAt).toLocaleString() })}
              </div>
            </AnimatedSection>
          </div>
        )}

        {/* App footer */}
        <footer className="mt-12 py-6 border-t border-gray-200">
          <div className="max-w-4xl mx-auto px-4 text-center text-sm text-gray-600">
            <p>
              {t("footer.developedBy")}{" "}
              <a
                href="https://nipuna.netlify.app/"
                target="_blank"
                rel="noopener noreferrer"
                className="font-semibold text-emerald-700 hover:text-emerald-500 underline decoration-emerald-300 hover:decoration-emerald-500 transition-colors"
              >
                Nipuna Gomes
              </a>
              <span className="mx-3 text-gray-300">|</span>
              <Link
                href="/contact"
                className="font-semibold text-blue-600 hover:text-blue-500 underline decoration-blue-300 hover:decoration-blue-500 transition-colors"
              >
                📬 {t("contact.title")}
              </Link>
            </p>
          </div>
        </footer>
      </main>
    </div>
  );
}

export default function Home() {
  return (
    <LocalizationProvider>
      <HomeContent />
    </LocalizationProvider>
  );
}
