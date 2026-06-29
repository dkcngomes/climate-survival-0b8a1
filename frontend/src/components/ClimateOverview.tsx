"use client";

import { ClimateForecast, ClimateSignalLabels, ClimateSignalDescriptions } from "@/types";
import { useLocalization } from "@/i18n/LocalizationContext";

interface Props {
  forecast: ClimateForecast;
}

const signalColors: Record<string, string> = {
  ElNino: "bg-orange-100 border-orange-300 text-orange-800",
  LaNina: "bg-blue-100 border-blue-300 text-blue-800",
  Drought: "bg-amber-100 border-amber-300 text-amber-800",
  HeavyRainfall: "bg-indigo-100 border-indigo-300 text-indigo-800",
  Heatwave: "bg-red-100 border-red-300 text-red-800",
  ColdSpell: "bg-cyan-100 border-cyan-300 text-cyan-800",
  FloodRisk: "bg-purple-100 border-purple-300 text-purple-800",
  StormRisk: "bg-pink-100 border-pink-300 text-pink-800",
  ExtremeDrought: "bg-red-100 border-red-300 text-red-800",
  Normal: "bg-green-100 border-green-300 text-green-800",
};

export default function ClimateOverview({ forecast }: Props) {
  const { t } = useLocalization();
  const h = forecast.hydroMeteo;

  return (
    <div className="bg-white rounded-2xl shadow-lg p-6 mb-8">
      <div className="flex items-start justify-between mb-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">🌤 {t("climate.outlook")}</h2>
          <p className="text-gray-800 text-sm mt-1">{forecast.locationName}</p>
          <p className="text-gray-600 text-xs">{forecast.region}</p>
        </div>
        <div className="text-right">
          <p className="text-xs text-gray-700">{t("climate.forecastConfidence")}</p>
          <p className="text-lg font-bold text-gray-900">{forecast.probability}%</p>
          {forecast.forecastPeriodLabel && (
            <p className="text-xs text-gray-700 mt-1 font-medium">{forecast.forecastPeriodLabel}</p>
          )}
        </div>
      </div>

      {/* Seasonal Anomaly metrics */}
      <div className="grid grid-cols-2 gap-4 mb-6">
        <div className="bg-gray-50 rounded-xl p-4">
          <p className="text-xs text-gray-700 uppercase tracking-wide">{t("climate.temperature")}</p>
          <p className={`text-2xl font-bold mt-1 ${(forecast.temperatureAnomaly ?? 0) > 0 ? "text-red-600" : "text-blue-600"}`}>
            {forecast.temperatureAnomaly != null
              ? `${forecast.temperatureAnomaly > 0 ? "+" : ""}${forecast.temperatureAnomaly.toFixed(1)}°C`
              : "N/A"}
          </p>
          <p className="text-xs text-gray-600 mt-1">{t("climate.seasonalAnomaly")}</p>
        </div>
        <div className="bg-gray-50 rounded-xl p-4">
          <p className="text-xs text-gray-700 uppercase tracking-wide">{t("climate.precipitation")}</p>
          <p className={`text-2xl font-bold mt-1 ${(forecast.precipitationAnomaly ?? 0) > 0 ? "text-blue-600" : "text-amber-600"}`}>
            {forecast.precipitationAnomaly != null
              ? `${forecast.precipitationAnomaly > 0 ? "+" : ""}${forecast.precipitationAnomaly.toFixed(1)}mm`
              : "N/A"}
          </p>
          <p className="text-xs text-gray-600 mt-1">{t("climate.seasonalAnomaly")}</p>
        </div>
      </div>

      {/* Hydrometeorological Panel */}
      {h && (
        <div className="mb-6 bg-gradient-to-br from-sky-50 to-blue-50 rounded-xl p-4 border border-sky-200">
          <h3 className="text-sm font-semibold text-sky-800 uppercase tracking-wide mb-3 flex items-center gap-2">
            💧 {t("climate.hydrometeo")}
          </h3>

          {/* Soil Moisture */}
          {h.meanSoilMoisture != null && (
            <div className="mb-3">
              <div className="flex justify-between text-xs text-gray-700 mb-1">
                <span>{t("climate.soilMoisture")}</span>
                <span>{(h.meanSoilMoisture * 100).toFixed(0)}%</span>
              </div>
              <div className="w-full h-2 bg-gray-200 rounded-full overflow-hidden">
                <div
                  className={`h-full rounded-full transition-all ${
                    h.isDroughtCondition ? "bg-red-500" : h.meanSoilMoisture > 0.3 ? "bg-blue-500" : "bg-yellow-500"
                  }`}
                  style={{ width: `${Math.min(100, h.meanSoilMoisture * 100)}%` }}
                />
              </div>
              <div className="flex justify-between text-xs mt-1">
                <span className="text-red-600 font-medium">{t("climate.dry")}</span>
                <span className="text-blue-600 font-medium">{t("climate.saturated")}</span>
              </div>
            </div>
          )}

          {/* Indices grid */}
          <div className="grid grid-cols-3 gap-3 mt-4">
            {h.droughtSeverityIndex != null && (
              <div className={`rounded-lg p-3 text-center ${h.droughtSeverityIndex > 60 ? "bg-red-100" : h.droughtSeverityIndex > 30 ? "bg-yellow-100" : "bg-green-100"}`}>
                <p className="text-xs text-gray-700">{t("climate.drought")}</p>
                <p className="text-xl font-bold text-gray-900">{h.droughtSeverityIndex}</p>
                <p className="text-xs text-gray-600">/ 100</p>
              </div>
            )}
            {h.floodRiskIndex != null && (
              <div className={`rounded-lg p-3 text-center ${h.floodRiskIndex > 50 ? "bg-purple-100" : h.floodRiskIndex > 20 ? "bg-yellow-100" : "bg-green-100"}`}>
                <p className="text-xs text-gray-700">{t("climate.floodRisk")}</p>
                <p className="text-xl font-bold text-gray-900">{h.floodRiskIndex}</p>
                <p className="text-xs text-gray-600">/ 100</p>
              </div>
            )}
            {h.stormSeverityIndex != null && (
              <div className={`rounded-lg p-3 text-center ${h.stormSeverityIndex > 50 ? "bg-pink-100" : h.stormSeverityIndex > 20 ? "bg-yellow-100" : "bg-green-100"}`}>
                <p className="text-xs text-gray-700">{t("climate.stormRisk")}</p>
                <p className="text-xl font-bold text-gray-900">{h.stormSeverityIndex}</p>
                <p className="text-xs text-gray-600">/ 100</p>
              </div>
            )}
          </div>

          {/* Metrics grid */}
          <div className="grid grid-cols-2 gap-2 mt-3 text-xs text-gray-700">
            {h.riverDischargeMax != null && (
              <p>🌊 {t("climate.riverDischarge")}: <strong>{h.riverDischargeMax.toFixed(0)} m³/s</strong></p>
            )}
            {h.maxWindGustKmh != null && (
              <p>💨 {t("climate.maxWindGust")}: <strong>{h.maxWindGustKmh.toFixed(0)} km/h</strong></p>
            )}
            {h.dailyPrecipitationSum != null && (
              <p>🌧️ {t("climate.precip3day")}: <strong>{h.dailyPrecipitationSum.toFixed(1)} mm</strong></p>
            )}
            {h.precipitationIntensityMm != null && (
              <p>⛈️ {t("climate.peakIntensity")}: <strong>{h.precipitationIntensityMm.toFixed(1)} mm/h</strong></p>
            )}
          </div>
        </div>
      )}

      {/* Detected signals */}
{/*       <h3 className="text-sm font-semibold text-gray-800 uppercase tracking-wide mb-3">
        {t("climate.detectedSignals")}
      </h3>
      <div className="space-y-3">
        {forecast.detectedSignals.map((signal) => (
          <div
            key={signal}
            className={`rounded-xl border-2 p-4 ${signalColors[signal] || "bg-gray-100 border-gray-300"}`}
          >
            <p className="font-bold text-gray-900">{ClimateSignalLabels[signal] || signal}</p>
            <p className="text-sm mt-1 text-gray-800">
              {ClimateSignalDescriptions[signal] || ""}
            </p>
          </div>
        ))}
      </div> */}

    </div>
  );
}
