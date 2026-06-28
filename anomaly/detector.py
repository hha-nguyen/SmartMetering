"""Anomaly detector: nghe luồng số đo (fanout exchange 'readings'),
phát hiện bất thường (z-score theo từng meter + ngưỡng cứng),
publish 'AnomalyDetected' vào fanout exchange 'anomalies'."""
import os
import json
import statistics
from collections import deque, defaultdict

import pika

RABBIT_HOST = os.getenv("RABBIT_HOST", "rabbitmq")
RABBIT_USER = os.getenv("RABBIT_USER", "admin")
RABBIT_PASS = os.getenv("RABBIT_PASS", "admin")

IN_EXCHANGE = "readings"
IN_QUEUE = "anomaly.readings"
OUT_EXCHANGE = "anomalies"

WINDOW = 20          # số mẫu gần nhất giữ cho mỗi meter
Z_THRESHOLD = 3.0    # lệch > 3 độ lệch chuẩn = bất thường
HARD_MAX = 10.0      # kWh: bình thường 0.1-3.0 -> > 10 chắc chắn bất thường

history = defaultdict(lambda: deque(maxlen=WINDOW))


def detect(meter_id: str, kwh: float):
    reasons = []
    if kwh > HARD_MAX:
        reasons.append(f"above {HARD_MAX} kWh")

    h = history[meter_id]
    if len(h) >= 8:
        mean = statistics.fmean(h)
        std = statistics.pstdev(h)
        if std > 0 and (kwh - mean) / std > Z_THRESHOLD:
            reasons.append(f"z-score > {Z_THRESHOLD}")

    h.append(kwh)
    return reasons


def main():
    creds = pika.PlainCredentials(RABBIT_USER, RABBIT_PASS)
    conn = pika.BlockingConnection(pika.ConnectionParameters(host=RABBIT_HOST, credentials=creds))
    ch = conn.channel()

    ch.exchange_declare(exchange=IN_EXCHANGE, exchange_type="fanout", durable=True)
    ch.queue_declare(queue=IN_QUEUE, durable=True)
    ch.queue_bind(queue=IN_QUEUE, exchange=IN_EXCHANGE)
    ch.exchange_declare(exchange=OUT_EXCHANGE, exchange_type="fanout", durable=True)

    def on_msg(c, method, props, body):
        try:
            r = json.loads(body)
            reasons = detect(r["meterId"], float(r["kwh"]))
            if reasons:
                evt = {
                    "meterId": r["meterId"],
                    "timestamp": r["timestamp"],
                    "kwh": r["kwh"],
                    "reason": ", ".join(reasons),
                }
                c.basic_publish(exchange=OUT_EXCHANGE, routing_key="", body=json.dumps(evt))
                print(f"ANOMALY {r['meterId']} kwh={r['kwh']} ({evt['reason']})", flush=True)
            c.basic_ack(method.delivery_tag)
        except Exception as e:  # noqa: BLE001
            print(f"ERR {e}", flush=True)
            c.basic_nack(method.delivery_tag, requeue=False)

    ch.basic_consume(queue=IN_QUEUE, on_message_callback=on_msg)
    print("Anomaly detector listening on 'anomaly.readings'...", flush=True)
    ch.start_consuming()


main()
