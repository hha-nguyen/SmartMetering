FROM python:3.13-slim
WORKDIR /app
COPY sim/simulator.py .
# -u: output không buffer -> log hiện ngay
ENTRYPOINT ["python", "-u", "simulator.py"]
