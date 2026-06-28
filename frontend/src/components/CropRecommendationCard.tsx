"use client";

import { CropRecommendation } from "@/types";
import { useLocalization } from "@/i18n/LocalizationContext";

interface Props {
  crop: CropRecommendation;
  index: number;
}

const scoreColors = (score: number) => {
  if (score >= 80) return { bg: "bg-green-50 border-green-300", badge: "bg-green-500", text: "text-green-700" };
  if (score >= 60) return { bg: "bg-lime-50 border-lime-300", badge: "bg-lime-500", text: "text-lime-700" };
  if (score >= 40) return { bg: "bg-yellow-50 border-yellow-300", badge: "bg-yellow-500", text: "text-yellow-700" };
  return { bg: "bg-gray-50 border-gray-300", badge: "bg-gray-400", text: "text-gray-600" };
};

const categoryIcons: Record<string, string> = {
  "Leafy Greens": "🥬",
  "Root Vegetables": "🥕",
  Vegetables: "🥒",
  Fruits: "🍓",
  Grains: "🌾",
  Legumes: "🫘",
  Herbs: "🌿",
};

const toleranceIcon = (level: string) => {
  switch (level) {
    case "High": return "✅";
    case "Medium": return "⚠️";
    case "Low": return "❌";
    default: return "❓";
  }
};

export default function CropRecommendationCard({ crop, index }: Props) {
  const { t } = useLocalization();
  const colors = scoreColors(crop.suitabilityScore);
  const icon = categoryIcons[crop.category] || "🌱";

  // Calculate days from today to harvest
  const now = new Date();
  const harvestStart = new Date(crop.harvestStartDate);
  const harvestEnd = new Date(crop.harvestEndDate);
  const daysUntilHarvestStart = Math.ceil((harvestStart.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
  const daysUntilHarvestEnd = Math.ceil((harvestEnd.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));

  // Fallback image if Wikipedia thumbnail not available
  const imgSrc = crop.imageUrl || "";

  return (
    <div className={`rounded-2xl border-2 p-4 ${colors.bg} transition-all hover:shadow-md`}>
      {/* Header */}
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-center gap-3">
          {/* Crop image */}
          {imgSrc ? (
            <div className="w-16 h-16 shrink-0 rounded-xl overflow-hidden border border-gray-200 bg-white">
              <img
                src={imgSrc}
                alt={crop.cropName}
                className="w-full h-full object-cover"
                loading="lazy"
                onError={(e) => {
                  (e.target as HTMLImageElement).style.display = "none";
                  (e.target as HTMLImageElement).parentElement!.innerHTML = `<span class="text-3xl flex items-center justify-center h-full">${icon}</span>`;
                }}
              />
            </div>
          ) : (
            <span className="text-4xl shrink-0">{icon}</span>
          )}
          <div>
            <div className="flex items-center gap-2">
              <span className="text-xs text-gray-600 font-mono">#{index + 1}</span>
              {/* Suitability score */}
              <span className={`px-2 py-0.5 rounded-full text-xs font-semibold text-white ${colors.badge}`}>
                {crop.suitabilityScore}% {t("grow.match")}
              </span>
            </div>
            <h3 className="text-lg font-bold text-black mt-1">{crop.cropName}</h3>
            <p className="text-xs text-gray-700">{crop.category}</p>
          </div>
        </div>
        {/* Harvest countdown */}
        <div className="text-right shrink-0">
          <p className="text-xs text-gray-700">{t("grow.harvestIn")}</p>
          <p className="text-2xl font-extrabold text-black">
            {daysUntilHarvestStart}-{daysUntilHarvestEnd}
          </p>
          <p className="text-xs text-gray-600">{t("grow.days")}</p>
        </div>
      </div>

      {/* Description & reason */}
      <p className="text-sm text-black mb-3">{crop.description}</p>
      <p className="text-sm text-blue-800 bg-blue-50 rounded-lg p-3 mb-3">
        💡 <strong>{t("grow.whyGrow")}:</strong> {crop.recommendationReason}
      </p>

      {/* Tolerance badges */}
      <div className="flex flex-wrap gap-2 mb-3">
        <span className="text-xs px-2 py-1 rounded-full bg-white border border-gray-200 text-black">
          🌡️ {t("grow.heat")}: {toleranceIcon(crop.heatTolerance)} {crop.heatTolerance}
        </span>
        <span className="text-xs px-2 py-1 rounded-full bg-white border border-gray-200 text-black">
          ❄️ {t("grow.cold")}: {toleranceIcon(crop.coldTolerance)} {crop.coldTolerance}
        </span>
        <span className="text-xs px-2 py-1 rounded-full bg-white border border-gray-200 text-black">
          🏜️ {t("grow.drought")}: {toleranceIcon(crop.droughtTolerance)} {crop.droughtTolerance}
        </span>
        <span className="text-xs px-2 py-1 rounded-full bg-white border border-gray-200 text-black">
          🌊 {t("grow.flood")}: {toleranceIcon(crop.floodTolerance)} {crop.floodTolerance}
        </span>
      </div>

      {/* Planting details */}
      <div className="bg-white/80 rounded-xl p-3 space-y-2">
        <div className="flex items-start gap-2">
          <span className="text-green-700 font-bold text-sm">🌱</span>
          <p className="text-sm text-black">
            <span className="font-semibold">{t("grow.planting")}:</span> {crop.plantingMethod}
          </p>
        </div>
        <div className="flex items-start gap-2">
          <span className="text-amber-700 font-bold text-sm">📋</span>
          <p className="text-sm text-black">
            <span className="font-semibold">{t("grow.tip")}:</span> {crop.growingTip}
          </p>
        </div>
        <div className="flex items-start gap-2">
          <span className="text-purple-700 font-bold text-sm">📅</span>
          <p className="text-sm text-black">
            <span className="font-semibold">{t("grow.plantBy")}:</span> {crop.plantByDate} →{" "}
            <span className="font-semibold">{t("grow.harvest")}:</span>{" "}
            {new Date(crop.harvestStartDate).toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" })}
            {" – "}
            {new Date(crop.harvestEndDate).toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" })}
          </p>
        </div>
      </div>
    </div>
  );
}
