"""Meter simulator: bơm số đo giả vào Ingestion API liên tục.
Cấu hình qua env: INGESTION_URL, METER_COUNT, INTERVAL_SECONDS.
Dùng urllib (stdlib) nên không cần cài package."""
import os
import json
import time
import random
import datetime
import urllib.request

INGESTION_URL = os.getenv("INGESTION_URL", "http://ingestion:8080/readings")
METER_COUNT = int(os.getenv("METER_COUNT", "5"))
INTERVAL = float(os.getenv("INTERVAL_SECONDS", "5"))

meters = [f"meter-{i:03d}" for i in range(1, METER_COUNT + 1)]


def send(meter_id: str, kwh: float):
    payload = json.dumps({
        "meterId": meter_id,
        "timestamp": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "kwh": round(kwh, 3),
    }).encode()
    req = urllib.request.Request(
        INGESTION_URL, data=payload,
        headers={"Content-Type": "application/json"}, method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=5) as resp:
            return resp.status
    except Exception as e:  # noqa: BLE001
        return f"ERR {e}"


print(f"Simulator -> {INGESTION_URL} | meters={METER_COUNT} | every {INTERVAL}s", flush=True)
while True:
    for m in meters:
        if random.random() < 0.05:
            kwh = random.uniform(20, 60)    # ~5% bất thường (spike)
        else:
            kwh = random.uniform(0.1, 3.0)  # tiêu thụ ~15 phút bình thường (kWh)
        status = send(m, kwh)
        print(f"{m} kwh={kwh:.3f} -> {status}", flush=True)
    time.sleep(INTERVAL)
