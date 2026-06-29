// ── Matches backend C# models ──

export type ClimateSignal =
  | "ElNino"
  | "LaNina"
  | "Drought"
  | "HeavyRainfall"
  | "Heatwave"
  | "ColdSpell"
  | "Normal";

export const ClimateSignalLabels: Record<ClimateSignal, string> = {
  ElNino: "El Niño",
  LaNina: "La Niña",
  Drought: "Drought",
  HeavyRainfall: "Heavy Rainfall",
  Heatwave: "Heatwave",
  ColdSpell: "Cold Spell",
  Normal: "Normal Conditions",
};

export const ClimateSignalDescriptions: Record<ClimateSignal, string> = {
  ElNino: "Warmer-than-average sea temperatures in Pacific → disrupted weather patterns, crop stress, food price inflation",
  LaNina: "Cooler-than-average sea temperatures in Pacific → increased rainfall, flooding risk, supply chain disruption",
  Drought: "Prolonged dry period → water scarcity, crop failure, livestock losses",
  HeavyRainfall: "Above-average precipitation → flooding, transportation disruption, crop damage",
  Heatwave: "Extreme high temperatures → heat stress on crops, increased cooling demand, energy price spikes",
  ColdSpell: "Abnormally cold weather → heating demand surge, crop frost damage, transportation delays",
  Normal: "Weather patterns within expected seasonal norms",
};

export interface ClimateForecast {
  latitude: number;
  longitude: number;
  locationName: string;
  region: string;
  temperatureAnomaly?: number;
  precipitationAnomaly?: number;
  extremeTemperatureIndex?: number;
  extremePrecipitationIndex?: number;
  probability: number;
  forecastDate: string;
  forecastPeriodLabel?: string;
  detectedSignals: ClimateSignal[];
  hydroMeteo?: HydroMeteoData;
}

export interface HydroMeteoData {
  soilMoisture0To1cm?: number;
  soilMoisture1To3cm?: number;
  soilMoisture3To9cm?: number;
  soilMoisture9To27cm?: number;
  meanSoilMoisture?: number;
  isDroughtCondition: boolean;
  precipitationIntensityMm?: number;
  dailyPrecipitationSum?: number;
  isExtremePrecipitation: boolean;
  maxWindGustKmh?: number;
  isStormWind: boolean;
  evapotranspirationSum?: number;
  riverDischargeMax?: number;
  riverDischargeMean?: number;
  isFloodRisk: boolean;
  droughtSeverityIndex?: number;
  floodRiskIndex?: number;
  stormSeverityIndex?: number;
}

export interface ItemRecommendation {
  itemName: string;
  category: string;
  reason: string;
  priority: number;
  riskLevel: string;
  suggestedAction: string;
  storageTip: string;
  triggerSignal: ClimateSignal;
}

export interface RecommendationResponse {
  forecast: ClimateForecast;
  recommendations: ItemRecommendation[];
  generatedAt: string;
  overallRiskLevel: string;
}

// ===== CROP RECOMMENDATIONS =====
export interface CropRecommendation {
  cropName: string;
  category: string;
  description: string;
  daysToHarvestMin: number;
  daysToHarvestMax: number;
  plantByDate: string;
  harvestStartDate: string;
  harvestEndDate: string;
  heatTolerance: string;
  coldTolerance: string;
  droughtTolerance: string;
  floodTolerance: string;
  suitabilityScore: number;
  recommendationReason: string;
  growingTip: string;
  plantingMethod: string;
  imageUrl?: string;
  wikiUrl?: string;
}

export interface CropRecommendationResponse {
  crops: CropRecommendation[];
  totalCrops: number;
  generalAdvice: string;
}

export interface LocationState {
  lat: number;
  lng: number;
  status: "idle" | "loading" | "success" | "error" | "manual";
  error?: string;
}

export interface CountryInfo {
  code: string;
  name: string;
  latitude: number;
  longitude: number;
  region: string;
}

export interface CountryLocale {
  countryCode: string;
  languageCode: string;
  languageName: string;
  currencyCode: string;
  currencySymbol: string;
  locale: string;
}

export interface IpCountryResult {
  countryCode: string | null;
  country: CountryInfo | null;
  locale: CountryLocale | null;
}
