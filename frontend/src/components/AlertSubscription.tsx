"use client";

import { useState, useEffect } from "react";
import { CarrierEntry } from "@/types";
import { fetchCarriers, subscribeToAlerts, sendTestSms } from "@/services/api";
import { useLocalization } from "@/i18n/LocalizationContext";

interface Props {
  latitude: number;
  longitude: number;
  locationName?: string;
  countryCode?: string;
}

type Step = "form" | "sending" | "done" | "error";

export default function AlertSubscription({ latitude, longitude, locationName, countryCode }: Props) {
  const { t } = useLocalization();

  const [carriers, setCarriers] = useState<CarrierEntry[]>([]);
  const [phone, setPhone] = useState("");
  const [carrier, setCarrier] = useState("");
  const [step, setStep] = useState<Step>("form");
  const [errorMsg, setErrorMsg] = useState("");
  const [subscriptionId, setSubscriptionId] = useState("");
  const [testSent, setTestSent] = useState(false);

  useEffect(() => {
    fetchCarriers()
      .then(setCarriers)
      .catch(() => {}); // silently fail
  }, []);

  // Group carrier list by region for better UX
  const grouped = carriers.reduce<Record<string, CarrierEntry[]>>((acc, c) => {
    const region = c.code.endsWith("-lk") || c.code === "dialog" || c.code === "mobitel"
      ? "Sri Lanka" : c.code.endsWith("-in") || c.code === "jio" || c.code === "vi"
      ? "India" : c.code.endsWith("-uk")
      ? "UK" : c.code.endsWith("-au") || c.code.endsWith("-nz")
      ? "Australia / NZ"
      : c.code === "att" || c.code === "verizon" || c.code === "tmobile"
      ? "US & Canada" : "Other";
    (acc[region] ??= []).push(c);
    return acc;
  }, {});

  const handleSubscribe = async () => {
    if (!phone.trim() || !carrier) return;
    setStep("sending");
    setErrorMsg("");
    try {
      const sub = await subscribeToAlerts({
        phoneNumber: phone.trim(),
        carrierCode: carrier,
        latitude,
        longitude,
        locationName,
        countryCode,
      });
      setSubscriptionId(sub.id);
      setStep("done");
    } catch (err) {
      setErrorMsg(err instanceof Error ? err.message : "Something went wrong");
      setStep("error");
    }
  };

  const handleTestSms = async () => {
    if (!phone.trim() || !carrier) return;
    setTestSent(false);
    try {
      await sendTestSms(phone.trim(), carrier);
      setTestSent(true);
      setTimeout(() => setTestSent(false), 5000);
    } catch {
      // ignore
    }
  };

  return (
    <div className="rounded-2xl border border-emerald-200 bg-emerald-50/50 p-5 animate-fade-in">
      <div className="flex items-center gap-2 mb-3">
        <span className="text-xl">📱</span>
        <h3 className="font-semibold text-gray-800 text-lg">Climate Alerts via SMS</h3>
        <span className="text-[10px] px-2 py-0.5 bg-emerald-200 text-emerald-800 rounded-full font-medium">FREE</span>
      </div>
      <p className="text-sm text-gray-600 mb-4">
        Get SMS alerts when climate conditions change — drought warnings, heatwaves, storm risks, or perfect planting windows.
      </p>

      {step === "done" ? (
        <div className="bg-white rounded-xl p-4 border border-emerald-200 text-center">
          <div className="text-3xl mb-2">✅</div>
          <p className="font-semibold text-emerald-700 mb-1">You're subscribed!</p>
          <p className="text-sm text-gray-500 mb-3">
            A welcome SMS is on its way. Check your phone in a minute.
          </p>
          <button
            onClick={() => { setStep("form"); setSubscriptionId(""); }}
            className="text-sm text-emerald-600 hover:text-emerald-800 underline"
          >
            Subscribe another number
          </button>
        </div>
      ) : (
        <div className="space-y-3">
          {/* Phone number */}
          <div>
            <label className="block text-xs font-medium text-gray-700 mb-1">Phone Number</label>
            <input
              type="tel"
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              placeholder="e.g. 0771234567"
              className="w-full px-3 py-2.5 border border-gray-300 rounded-xl bg-white text-gray-900 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 outline-none text-sm"
              disabled={step === "sending"}
            />
          </div>

          {/* Carrier */}
          <div>
            <label className="block text-xs font-medium text-gray-700 mb-1">Mobile Carrier</label>
            <select
              value={carrier}
              onChange={(e) => setCarrier(e.target.value)}
              className="w-full px-3 py-2.5 border border-gray-300 rounded-xl bg-white text-gray-900 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 outline-none text-sm appearance-none cursor-pointer"
              disabled={step === "sending"}
            >
              <option value="">— Select your carrier —</option>
              {Object.entries(grouped).map(([region, carriers]) => (
                <optgroup key={region} label={region}>
                  {carriers.map((c) => (
                    <option key={c.code} value={c.code}>{c.name}</option>
                  ))}
                </optgroup>
              ))}
            </select>
          </div>

          {/* Error */}
          {step === "error" && (
            <div className="text-sm text-red-600 bg-red-50 rounded-xl px-3 py-2 border border-red-200">
              {errorMsg}
            </div>
          )}

          {/* Actions */}
          <div className="flex gap-2">
            <button
              onClick={handleSubscribe}
              disabled={step === "sending" || !phone.trim() || !carrier}
              className="flex-1 px-4 py-2.5 bg-emerald-600 hover:bg-emerald-700 disabled:bg-gray-300 text-white rounded-xl font-medium transition-colors text-sm"
            >
              {step === "sending" ? "Subscribing..." : "🔔 Subscribe to Alerts"}
            </button>
            <button
              onClick={handleTestSms}
              disabled={step === "sending" || !phone.trim() || !carrier}
              className="px-3 py-2.5 border border-gray-300 hover:bg-gray-50 disabled:text-gray-300 text-gray-600 rounded-xl text-sm transition-colors"
              title="Send a test SMS first"
            >
              {testSent ? "✅ Sent!" : "📨 Test"}
            </button>
          </div>

          <p className="text-[11px] text-gray-400 text-center">
            Powered by email-to-SMS gateways. Standard carrier rates may apply. 
            <a href="/contact" className="text-emerald-600 hover:underline ml-1">Contact us</a> to unsubscribe.
          </p>
        </div>
      )}
    </div>
  );
}
