"use client";

import { jsPDF } from "jspdf";
import html2canvas from "html2canvas";
import { RecommendationResponse, CropRecommendationResponse } from "@/types";
import { useLocalization } from "@/i18n/LocalizationContext";

interface Props {
  data: RecommendationResponse;
  cropData: CropRecommendationResponse | null;
  countryCode?: string;
}

export default function PdfDownloadButton({ data, cropData, countryCode }: Props) {
  const { t, locale } = useLocalization();

  const generatePdf = async () => {
    const pdf = new jsPDF("p", "mm", "a4");
    const pageW = 210;
    const pageH = 297;
    const margin = 15;
    const contentW = pageW - margin * 2;
    let y = margin;

    // ── Helper ──
    const addText = (text: string, size: number, bold = false, color: string = "#1f2937", align: "left" | "center" = "left") => {
      if (y > pageH - margin) {
        pdf.addPage();
        y = margin;
      }
      pdf.setFontSize(size);
      pdf.setFont("helvetica", bold ? "bold" : "normal");
      pdf.setTextColor(color);
      if (align === "center") {
        const textW = pdf.getTextWidth(text);
        pdf.text(text, (pageW - textW) / 2, y);
      } else {
        pdf.text(text, margin, y);
      }
      y += size * 0.5;
    };

    const addLine = () => {
      if (y > pageH - margin) {
        pdf.addPage();
        y = margin;
      }
      pdf.setDrawColor("#d1d5db");
      pdf.line(margin, y, pageW - margin, y);
      y += 6;
    };

    const addBullet = (label: string, value: string) => {
      if (y > pageH - margin) {
        pdf.addPage();
        y = margin;
      }
      pdf.setFontSize(10);
      pdf.setFont("helvetica", "bold");
      pdf.setTextColor("#374151");
      const labelW = pdf.getTextWidth(label + ": ");
      pdf.text("• " + label + ": ", margin, y);
      pdf.setFont("helvetica", "normal");
      pdf.setTextColor("#4b5563");
      // Wrap long values
      const remainingW = contentW - labelW - 6;
      const lines = pdf.splitTextToSize(value, remainingW);
      pdf.text(lines[0], margin + labelW + 4, y);
      if (lines.length > 1) {
        for (let i = 1; i < lines.length; i++) {
          y += 5;
          pdf.text(lines[i], margin + 6, y);
        }
      }
      y += 6;
    };

    // ── Capture charts as image ──
    const chartEl = document.getElementById("climate-charts-section");
    let chartImgData: string | null = null;
    if (chartEl) {
      try {
        const canvas = await html2canvas(chartEl, {
          scale: 2,
          backgroundColor: "#ffffff",
          logging: false,
        });
        chartImgData = canvas.toDataURL("image/png");
      } catch {
        // silently skip
      }
    }

    // ═══════════════ PAGE 1: HEADER ═══════════════
    // Title
    pdf.setFontSize(24);
    pdf.setFont("helvetica", "bold");
    pdf.setTextColor("#059669");
    pdf.text("Climate Survival", margin, y);
    y += 12;

    pdf.setFontSize(12);
    pdf.setFont("helvetica", "normal");
    pdf.setTextColor("#6b7280");
    pdf.text("Personalized Climate Adaptation Report", margin, y);
    y += 8;

    pdf.setFontSize(9);
    pdf.setTextColor("#9ca3af");
    pdf.text(`Generated: ${new Date().toLocaleDateString()} | ${locale.currencyCode} (${locale.currencySymbol})`, margin, y);
    y += 12;
    addLine();

    // ── Location & Risk ──
    addText(t("climate.overallRisk"), 14, true, "#1f2937");
    const riskColor = data.overallRiskLevel === "Critical" ? "#dc2626" :
      data.overallRiskLevel === "High" ? "#ea580c" : "#ca8a04";
    addText(data.overallRiskLevel, 18, true, riskColor, "center");
    y += 4;
    addBullet(t("climate.location"), `${data.forecast.locationName}, ${data.forecast.region}`);
    addBullet(t("climate.forecastConfidence"), `${data.forecast.probability}%`);
    if (data.forecast.temperatureAnomaly !== undefined) {
      addBullet(t("climate.temperatureAnomaly"), `${data.forecast.temperatureAnomaly.toFixed(1)}°C`);
    }
    if (data.forecast.precipitationAnomaly !== undefined) {
      addBullet(t("climate.precipitationAnomaly"), `${data.forecast.precipitationAnomaly.toFixed(1)}mm`);
    }
    addLine();

    // ── Climate Signals ──
    if (data.forecast.detectedSignals.length > 0) {
      addText(t("climate.detectedSignals"), 13, true);
      data.forecast.detectedSignals.forEach((signal) => {
        addBullet(signal, "");
      });
      addLine();
    }

    // ── Charts Image ──
    if (chartImgData) {
      addText(t("climate.charts"), 13, true);
      const imgW = contentW;
      const imgH = (imgW * chartEl!.offsetHeight) / chartEl!.offsetWidth;
      if (y + imgH > pageH - margin) {
        pdf.addPage();
        y = margin;
      }
      pdf.addImage(chartImgData, "PNG", margin, y, imgW, imgH);
      y += imgH + 8;
      addLine();
    }

    // ═══════════════ STOCK-UP RECOMMENDATIONS ═══════════════
    if (data.recommendations.length > 0) {
      if (y > pageH - 40) {
        pdf.addPage();
        y = margin;
      }
      addText(t("stockUp.title"), 14, true, "#1f2937");
      y += 2;

      // Table header
      pdf.setFontSize(9);
      pdf.setFont("helvetica", "bold");
      pdf.setFillColor("#f3f4f6");
      pdf.rect(margin, y, contentW, 6, "F");
      pdf.setTextColor("#374151");
      pdf.text("#", margin + 2, y + 4);
      pdf.text(t("stockUp.item"), margin + 10, y + 4);
      pdf.text(t("stockUp.risk"), margin + 80, y + 4);
      pdf.text(t("stockUp.action"), margin + 110, y + 4);
      y += 9;

      data.recommendations.slice(0, 15).forEach((item, i) => {
        if (y > pageH - 15) {
          pdf.addPage();
          y = margin;
        }
        pdf.setFontSize(8);
        pdf.setFont("helvetica", "normal");
        pdf.setTextColor("#4b5563");
        pdf.text(`${i + 1}.`, margin + 2, y);
        pdf.setFont("helvetica", "bold");
        pdf.text(item.itemName.substring(0, 22), margin + 10, y);
        pdf.setFont("helvetica", "normal");
        const riskShort = item.riskLevel.substring(0, 8);
        pdf.text(riskShort, margin + 80, y);
        const actionShort = item.suggestedAction.substring(0, 35);
        pdf.text(actionShort, margin + 110, y);
        y += 5;
      });
      y += 4;
      addLine();
    }

    // ═══════════════ CROP RECOMMENDATIONS ═══════════════
    if (cropData && cropData.crops.length > 0) {
      if (y > pageH - 40) {
        pdf.addPage();
        y = margin;
      }
      addText(t("grow.title"), 14, true, "#16a34a");
      y += 2;

      cropData.crops.slice(0, 15).forEach((crop, i) => {
        if (y > pageH - 20) {
          pdf.addPage();
          y = margin;
        }
        pdf.setFontSize(10);
        pdf.setFont("helvetica", "bold");
        pdf.setTextColor("#1f2937");
        pdf.text(`${i + 1}. ${crop.cropName}`, margin, y);
        y += 5;
        pdf.setFontSize(8);
        pdf.setFont("helvetica", "normal");
        pdf.setTextColor("#6b7280");
        const desc = crop.description.substring(0, 120);
        const descLines = pdf.splitTextToSize(desc, contentW);
        descLines.forEach((line: string) => {
          pdf.text(line, margin + 4, y);
          y += 4;
        });
        // Score and tolerances
        pdf.setTextColor("#4b5563");
        pdf.text(`${t("grow.suitabilityScore")}: ${crop.suitabilityScore}%  |  ${t("grow.heat")}: ${crop.heatTolerance}  ${t("grow.cold")}: ${crop.coldTolerance}  ${t("grow.drought")}: ${crop.droughtTolerance}  ${t("grow.flood")}: ${crop.floodTolerance}`, margin + 4, y);
        y += 6;
      });
      addLine();
    }

    // ═══════════════ FOOTER ═══════════════
    if (y > pageH - 20) {
      pdf.addPage();
      y = margin;
    }
    pdf.setFontSize(8);
    pdf.setFont("helvetica", "normal");
    pdf.setTextColor("#9ca3af");
    pdf.text("Generated by Climate Survival — climate-survival.netlify.app", margin, y);
    y += 4;
    pdf.text("Data sources: Open-Meteo, World Bank Pink Sheet, Wikipedia", margin, y);

    // ── Save ──
    pdf.save(`Climate-Survival-Report-${countryCode || "global"}.pdf`);
  };

  return (
    <button
      onClick={generatePdf}
      className="inline-flex items-center gap-2 px-5 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl font-medium text-sm transition-colors shadow-sm"
    >
      <span>📄</span>
      <span>{t("report.download")}</span>
    </button>
  );
}
