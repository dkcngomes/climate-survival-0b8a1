"use client";

import { useEffect, useState } from "react";
import { fetchSriLankaPrices, SriLankaPricesResponse } from "@/services/api";
import { useLocalization } from "@/i18n/LocalizationContext";

interface Props {
  countryCode?: string;
}

export default function SriLankaPrices({ countryCode }: Props) {
  const { t } = useLocalization();
  const [data, setData] = useState<SriLankaPricesResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedCategory, setSelectedCategory] = useState<string>("All");
  const [selectedMarket, setSelectedMarket] = useState<string>("All");

  useEffect(() => {
    if (countryCode !== "LK") return;
    loadPrices();
  }, [countryCode]);

  const loadPrices = async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await fetchSriLankaPrices();
      setData(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load prices");
    } finally {
      setLoading(false);
    }
  };

  if (countryCode !== "LK") return null;

  // Get unique categories from the data
  const categories = data
    ? ["All", ...new Set(data.commodities.map((c) => c.category))]
    : ["All"];

  const markets = ["All", "Dambulla", "Nuwara Eliya", "Kandy", "Pettah", "Jaffna"];

  // Filter commodities
  const filteredCommodities = data
    ? data.commodities.filter((c) => {
        if (selectedCategory !== "All" && c.category !== selectedCategory) return false;
        return true;
      })
    : [];

  // Price change helper
  const priceChange = (yesterday: number | null, today: number | null) => {
    if (yesterday === null || today === null) return { text: "n.a.", color: "text-gray-400" };
    const diff = today - yesterday;
    if (diff > 0) return { text: `▲ +${diff.toFixed(0)}`, color: "text-red-600" };
    if (diff < 0) return { text: `▼ ${diff.toFixed(0)}`, color: "text-green-600" };
    return { text: "—", color: "text-gray-400" };
  };

  // Loading state
  if (loading) {
    return (
      <div className="bg-white rounded-2xl border border-gray-200 p-5 shadow-sm">
        <div className="animate-pulse">
          <div className="h-5 bg-gray-200 rounded w-48 mb-4" />
          <div className="h-3 bg-gray-200 rounded w-64 mb-6" />
          <div className="space-y-2">
            {[1, 2, 3].map((i) => (
              <div key={i} className="h-4 bg-gray-200 rounded w-full" />
            ))}
          </div>
        </div>
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="bg-white rounded-2xl border border-gray-200 p-5 shadow-sm">
        <div className="text-center py-4 text-sm text-red-600">
          📊 {error}
          <button onClick={loadPrices} className="ml-2 underline hover:text-red-800">
            {t("error.tryAgain")}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-2xl border border-gray-200 p-5 shadow-sm">
      <div className="flex items-start justify-between gap-4 mb-4">
        <div>
          <h3 className="text-lg font-bold text-gray-900 flex items-center gap-2">
            🇱🇰 {t("prices.sriLankaTitle")}
          </h3>
          <p className="text-xs text-gray-600 mt-1">
            {t("prices.source")}: {data?.source || "CBSL"}
            {data?.reportDate && ` · ${data.reportDate}`}
          </p>
        </div>
        <button
          onClick={loadPrices}
          disabled={loading}
          className="text-xs px-3 py-1.5 bg-gray-100 hover:bg-gray-200 rounded-lg text-gray-700 transition-colors shrink-0"
        >
          🔄 {loading ? t("loading") : t("prices.refresh")}
        </button>
      </div>

      {/* Category & Market filters */}
      <div className="flex flex-wrap gap-2 mb-4">
        <select
          value={selectedCategory}
          onChange={(e) => setSelectedCategory(e.target.value)}
          className="text-xs px-2 py-1.5 rounded-lg border border-gray-200 bg-white text-gray-700"
        >
          {categories.map((cat) => (
            <option key={cat} value={cat}>
              {cat === "All" ? t("prices.allCategories") : cat}
            </option>
          ))}
        </select>
        <select
          value={selectedMarket}
          onChange={(e) => setSelectedMarket(e.target.value)}
          className="text-xs px-2 py-1.5 rounded-lg border border-gray-200 bg-white text-gray-700"
        >
          {markets.map((m) => (
            <option key={m} value={m}>
              {m === "All" ? t("prices.allMarkets") : m}
            </option>
          ))}
        </select>
      </div>

      {/* Price Table */}
      {data && !loading && (
        <>
          <div className="overflow-x-auto -mx-2">
            <table className="w-full text-xs">
              <thead>
                <tr className="border-b border-gray-200">
                  <th className="text-left py-2 px-2 font-semibold text-gray-700 whitespace-nowrap">
                    {t("prices.commodity")}
                  </th>
                  {markets.filter((m) => selectedMarket === "All" || m === selectedMarket).map((market) => (
                    <th key={market} className="text-right py-2 px-2 font-semibold text-gray-700 whitespace-nowrap" colSpan={market === "All" ? 0 : selectedMarket === "All" ? 3 : 2}>
                      {market === "All" ? "" : market}
                    </th>
                  ))}
                </tr>
                <tr className="border-b border-gray-100">
                  <th></th>
                  {markets
                    .filter((m) => selectedMarket === "All" || m === selectedMarket)
                    .filter((m) => m !== "All")
                    .map((market) => (
                      <>
                        <th key={`${market}-yest`} className="text-right py-1 px-1 text-[10px] text-gray-500 font-normal">{t("prices.yesterday")}</th>
                        <th key={`${market}-today`} className="text-right py-1 px-1 text-[10px] text-gray-500 font-normal">{t("prices.today")}</th>
                        {selectedMarket === "All" && (
                          <th key={`${market}-change`} className="text-right py-1 px-1 text-[10px] text-gray-500 font-normal w-14">{t("prices.change")}</th>
                        )}
                      </>
                    ))}
                </tr>
              </thead>
              <tbody>
                {filteredCommodities.map((item) => (
                  <tr key={item.commodity} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                    <td className="py-2 px-2">
                      <span className="font-medium text-gray-900">{item.commodity}</span>
                      <span className="text-[10px] text-gray-400 ml-1">({item.category})</span>
                    </td>
                    {markets
                      .filter((m) => selectedMarket === "All" || m === selectedMarket)
                      .filter((m) => m !== "All")
                      .map((market) => {
                        const price = item.prices.find((m) => m.market === market);
                        if (!price) return (
                          <>
                            <td key={`${market}-empty1`} className="text-right py-2 px-2 text-gray-400">—</td>
                            <td key={`${market}-empty2`} className="text-right py-2 px-2 text-gray-400">—</td>
                            {selectedMarket === "All" && (
                              <td key={`${market}-empty3`} className="text-right py-2 px-2 text-gray-400">—</td>
                            )}
                          </>
                        );

                        const change = priceChange(price.yesterday, price.today);

                        return (
                          <>
                            <td key={`${market}-yest`} className="text-right py-2 px-2 text-gray-600">
                              {price.yesterday?.toFixed(0) ?? "—"}
                            </td>
                            <td key={`${market}-today`} className="text-right py-2 px-2 font-medium text-gray-900">
                              {price.today?.toFixed(0) ?? "—"}
                            </td>
                            {selectedMarket === "All" && (
                              <td key={`${market}-change`} className={`text-right py-2 px-2 text-[11px] ${change.color}`}>
                                {change.text}
                              </td>
                            )}
                          </>
                        );
                      })}
                  </tr>
                ))}
                {filteredCommodities.length === 0 && (
                  <tr>
                    <td colSpan={100} className="text-center py-8 text-gray-500 text-sm">
                      {t("prices.noData")}
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {/* Footer note */}
          <p className="text-[10px] text-gray-400 mt-3">
            {t("prices.footer")}
          </p>
        </>
      )}
    </div>
  );
}
