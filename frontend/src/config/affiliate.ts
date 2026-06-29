/**
 * Affiliate link configuration for Climate Survival.
 *
 * To use Daraz Affiliate Program:
 * 1. Sign up at https://affiliate.daraz.com/
 * 2. Get your Affiliate ID (looks like a short code)
 * 3. Replace the value below with your affiliate ID
 */

export const AFFILIATE_CONFIG = {
  daraz: {
    affiliateId: "155412816", // ← Your Daraz affiliate member ID
    baseUrl: "https://www.daraz.lk",
  },
} as const;

/**
 * Map item names to Daraz search keywords for relevant product results.
 * Falls back to the item name itself if not in the map.
 */
const darazSearchMap: Record<string, string> = {
  "Bottled Water": "water storage container",
  Rice: "rice 5kg",
  "Flour / Wheat": "wheat flour 1kg",
  "Canned Food (Mixed)": "canned food",
  "Cooking Oil": "cooking oil 1L",
  "Dried Beans": "beans dried",
  Sugar: "sugar 1kg",
  Salt: "salt packet",
  "Pasta / Noodles": "pasta noodles",
  "Oats / Cereal": "oatmeal cereal",
  "Powdered Milk": "milk powder 1kg",
  "Coffee / Tea": "coffee tea",
  "First Aid Kit": "first aid kit",
  "Water Filter": "water purifier filter",
  "Seeds (Non-Hybrid)": "vegetable seeds",
  "Gardening Tools": "garden tools set",
  "Batteries": "batteries pack",
  Flashlights: "torch light emergency",
  "Solar Charger": "solar panel charger",
  "Fuel (Propane)": "gas stove camping",
  Multivitamins: "multivitamin tablets",
  "Freeze-Dried Food": "dry food pack",
  "Prescription Meds": "medicine organizer box",
  "Hygiene Products": "soap hand sanitizer",
  "Bleach / Sanitizer": "disinfectant liquid",
  Fertilizer: "plant fertilizer organic",
  "Soil / Compost": "potting soil bag",
  "Pesticide (Natural)": "natural pesticide plants",
  "Drip Irrigation": "drip irrigation kit",
  "Frost Blanket": "plant cover frost",
  "Shade Cloth": "shade net garden",
  "Greenhouse Supplies": "greenhouse plastic",
  "Rain Barrel": "water collection tank",
  "Seed Trays": "seedling tray",
  "Grow Lights": "grow led light plants",
};

export function getAffiliateSearchUrl(itemName: string): string {
  const searchTerm = darazSearchMap[itemName] || itemName;
  const encoded = encodeURIComponent(searchTerm);
  return `${AFFILIATE_CONFIG.daraz.baseUrl}/catalog/?q=${encoded}&af=${AFFILIATE_CONFIG.daraz.affiliateId}`;
}
