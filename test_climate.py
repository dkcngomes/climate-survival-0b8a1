import urllib.request, json

locations = [
    (51.5074, -0.1278, 'London'),
    (40.7128, -74.0060, 'NYC'),
    (35.6762, 139.6503, 'Tokyo'),
    (19.0760, 72.8777, 'Mumbai'),
    (-33.8688, 151.2093, 'Sydney'),
    (55.7558, 37.6173, 'Moscow'),
    (14.5995, 120.9842, 'Manila'),
]

for lat, lng, name in locations:
    url = f'https://nipunadkcn-climate-survival-api.hf.space/api/recommendations?lat={lat}&lng={lng}'
    req = urllib.request.Request(url)
    try:
        with urllib.request.urlopen(req, timeout=20) as resp:
            d = json.loads(resp.read())
            f = d['forecast']
            signals = f['detectedSignals']
            print(f'{name:10s} | signals={signals} | risk={d["overallRiskLevel"]:8s} | loc={f["locationName"]:20s} | temp_anom={f.get("temperatureAnomaly","?")} | precip_anom={f.get("precipitationAnomaly","?")} | prob={f["probability"]}')
    except Exception as e:
        print(f'{name:10s} | ERROR: {e}')
