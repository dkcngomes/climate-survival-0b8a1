"use client";

import { ItemRecommendation } from "@/types";
import { useLocalization } from "@/i18n/LocalizationContext";
import { getAffiliateSearchUrl } from "@/config/affiliate";

interface Props {
  item: ItemRecommendation;
  index: number;
  countryCode?: string;
}

const riskColors: Record<string, { bg: string; text: string; dot: string }> = {
  Critical: { bg: "bg-red-50 border-red-200", text: "text-red-700", dot: "bg-red-500" },
  High: { bg: "bg-orange-50 border-orange-200", text: "text-orange-700", dot: "bg-orange-500" },
  Medium: { bg: "bg-yellow-50 border-yellow-200", text: "text-yellow-700", dot: "bg-yellow-500" },
  Low: { bg: "bg-green-50 border-green-200", text: "text-green-700", dot: "bg-green-500" },
};

const categoryIcons: Record<string, string> = {
  Grains: "🌾",
  "Canned & Preserved": "🥫",
  "Oils & Fats": "🫒",
  Protein: "🥩",
  Food: "🍚",
  Beverages: "🧃",
  Essentials: "🔋",
  Agriculture: "🌱",
};

export default function RecommendationCard({ item, index, countryCode }: Props) {
  const { t, formatCurrency } = useLocalization();
  const colors = riskColors[item.riskLevel] || riskColors.Medium;
  const icon = categoryIcons[item.category] || "📦";
  const isSriLanka = countryCode === "LK";

  return (
    <div className={`rounded-2xl border-2 p-5 ${colors.bg} transition-all hover:shadow-md`}>
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-center gap-3">
          <span className="text-3xl">{icon}</span>
          <div>
            <div className="flex items-center gap-2">
              <span className="text-xs text-gray-600 font-mono">#{index + 1}</span>
              <span className={`px-2 py-0.5 rounded-full text-xs font-semibold ${colors.text} bg-white/80`}>
                {item.riskLevel}
              </span>
            </div>
            <h3 className="text-lg font-bold text-gray-900 mt-1">{item.itemName}</h3>
            <p className="text-xs text-gray-700">{item.category}</p>
            {/* Localized estimated price */}
            {item.estimatedPrice != null && (
              <p className="text-sm font-semibold text-gray-900 mt-1">
                {t("stockUp.estPrice")}: {formatCurrency(item.estimatedPrice)}
              </p>
            )}
          </div>
        </div>
      </div>

      <p className="text-sm text-gray-900 mb-3">{item.reason}</p>

      <div className="bg-white/80 rounded-xl p-3 space-y-2">
        <div className="flex items-start gap-2">
          <span className="text-blue-700 font-bold text-sm">💡</span>
          <p className="text-sm text-gray-900">
            <span className="font-semibold">{t("stockUp.action")}:</span> {item.suggestedAction}
          </p>
        </div>
        <div className="flex items-start gap-2">
          <span className="text-green-700 font-bold text-sm">📦</span>
          <p className="text-sm text-gray-900">
            <span className="font-semibold">{t("stockUp.storage")}:</span> {item.storageTip}
          </p>
        </div>
      </div>

      {/* Affiliate link — only shown for Sri Lanka (Daraz.lk) */}
      {isSriLanka && (
        <a
          href={getAffiliateSearchUrl(item.itemName)}
          target="_blank"
          rel="noopener noreferrer sponsored"
          className="mt-3 inline-flex items-center gap-1.5 text-xs text-orange-600 hover:text-orange-800 transition-colors"
        >
          <span>🛒</span>
          <span>Buy on Daraz</span>
          <span className="text-[10px] opacity-70">(affiliate)</span>
        </a>
      )}
    </div>
  );
}
