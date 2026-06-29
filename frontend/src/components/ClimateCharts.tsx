"use client";

import { ClimateForecast } from "@/types";
import { useLocalization } from "@/i18n/LocalizationContext";
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  Cell,
  PieChart,
  Pie,
} from "recharts";

interface Props {
  forecast: ClimateForecast;
}

export default function ClimateCharts({ forecast }: Props) {
  const { t } = useLocalization();
  const hm = forecast.hydroMeteo;

  // Gauge data for risk indices
  const riskGauges = [
    {
      label: t("climate.drought"),
      value: hm?.droughtSeverityIndex ?? 0,
      color: "#f59e0b",
    },
    {
      label: t("climate.floodRisk"),
      value: hm?.floodRiskIndex ?? 0,
      color: "#3b82f6",
    },
    {
      label: t("climate.storm"),
      value: hm?.stormSeverityIndex ?? 0,
      color: "#8b5cf6",
    },
  ];

  // Soil moisture gauge data
  const soilMoisture = hm?.meanSoilMoisture ?? 0;
  const soilData = [
    { name: "Moisture", value: Math.round(soilMoisture * 100), fill: "#22c55e" },
    { name: "Remaining", value: 100 - Math.round(soilMoisture * 100), fill: "#e5e7eb" },
  ];

  // Sensor data for bar chart
  const sensorData = [
    {
      name: t("climate.windGust"),
      value: hm?.maxWindGustKmh ?? 0,
      unit: "km/h",
      color: "#8b5cf6",
    },
    {
      name: t("climate.riverDischarge"),
      value: hm?.riverDischargeMean ?? 0,
      unit: "m³/s",
      color: "#3b82f6",
    },
    {
      name: t("climate.peakIntensity"),
      value: hm?.precipitationIntensityMm ?? 0,
      unit: "mm/h",
      color: "#06b6d4",
    },
    {
      name: t("climate.threeDayPrecip"),
      value: hm?.dailyPrecipitationSum ?? 0,
      unit: "mm",
      color: "#0ea5e9",
    },
  ];

  // Temperature & precipitation anomaly
  const anomalies = [
    {
      name: t("climate.temperature"),
      value: forecast.temperatureAnomaly ?? 0,
      unit: "°C",
      color: forecast.temperatureAnomaly && forecast.temperatureAnomaly > 0 ? "#ef4444" : "#3b82f6",
    },
    {
      name: t("climate.precipitation"),
      value: forecast.precipitationAnomaly ?? 0,
      unit: "mm",
      color: forecast.precipitationAnomaly && forecast.precipitationAnomaly > 0 ? "#3b82f6" : "#f59e0b",
    },
  ];

  return (
    <div className="bg-white rounded-2xl border border-gray-200 p-5 shadow-sm">
      <h3 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
        📊 {t("climate.charts")}
      </h3>

      {/* Anomaly Bar Chart */}
      <div className="mb-6">
        <h4 className="text-sm font-semibold text-gray-700 mb-2">{t("climate.seasonalAnomaly")}</h4>
        <div className="h-48">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={anomalies}>
              <XAxis dataKey="name" tick={{ fontSize: 12, fill: "#374151" }} />
              <YAxis tick={{ fontSize: 11, fill: "#6b7280" }} />
              <Tooltip
                contentStyle={{ fontSize: 13, borderRadius: 8 }}
                formatter={(value, name) => [`${Number(value).toFixed(1)}`, name]}
              />
              <Bar dataKey="value" radius={[6, 6, 0, 0]}>
                {anomalies.map((entry, i) => (
                  <Cell key={i} fill={entry.color} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Risk Gauges (Pie Charts as radial gauges) */}
      <div className="mb-6">
        <h4 className="text-sm font-semibold text-gray-700 mb-2">{t("climate.riskIndices")}</h4>
        <div className="grid grid-cols-3 gap-3">
          {riskGauges.map((gauge) => {
            const data = [
              { name: gauge.label, value: gauge.value, fill: gauge.color },
              { name: "Remaining", value: 100 - gauge.value, fill: "#e5e7eb" },
            ];
            return (
              <div key={gauge.label} className="flex flex-col items-center">
                <div className="h-20 w-full">
                  <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                      <Pie
                        data={data}
                        cx="50%"
                        cy="100%"
                        startAngle={180}
                        endAngle={0}
                        innerRadius="60%"
                        outerRadius="90%"
                        dataKey="value"
                        stroke="none"
                      >
                        {data.map((entry, i) => (
                          <Cell key={i} fill={entry.fill} />
                        ))}
                      </Pie>
                    </PieChart>
                  </ResponsiveContainer>
                </div>
                <span className="text-lg font-bold text-gray-900 -mt-1">{gauge.value}</span>
                <span className="text-[10px] text-gray-600 text-center leading-tight">{gauge.label}</span>
              </div>
            );
          })}
        </div>
      </div>

      {/* Soil Moisture Gauge */}
      <div className="mb-6">
        <h4 className="text-sm font-semibold text-gray-700 mb-2">{t("climate.soilMoisture")}</h4>
        <div className="h-24">
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie
                data={soilData}
                cx="50%"
                cy="100%"
                startAngle={180}
                endAngle={0}
                innerRadius="55%"
                outerRadius="90%"
                dataKey="value"
                stroke="none"
              />
            </PieChart>
          </ResponsiveContainer>
        </div>
        <p className="text-center text-sm font-semibold text-gray-900 -mt-3">
          {Math.round(soilMoisture * 100)}%
        </p>
      </div>

      {/* Sensor Data Bar Chart */}
      <div>
        <h4 className="text-sm font-semibold text-gray-700 mb-2">{t("climate.sensorData")}</h4>
        <div className="h-48">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={sensorData} layout="vertical">
              <XAxis type="number" tick={{ fontSize: 11, fill: "#6b7280" }} />
              <YAxis type="category" dataKey="name" tick={{ fontSize: 11, fill: "#374151" }} width={80} />
              <Tooltip
                contentStyle={{ fontSize: 13, borderRadius: 8 }}
                formatter={(value, name) => [`${Number(value).toFixed(1)}`, name]}
              />
              <Bar dataKey="value" radius={[0, 6, 6, 0]}>
                {sensorData.map((entry, i) => (
                  <Cell key={i} fill={entry.color} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>
    </div>
  );
}
