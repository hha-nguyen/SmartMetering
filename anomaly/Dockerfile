FROM python:3.13-slim
WORKDIR /app
RUN pip install --no-cache-dir pika
COPY anomaly/detector.py .
ENTRYPOINT ["python", "-u", "detector.py"]
