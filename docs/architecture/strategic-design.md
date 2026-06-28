# Energy IoT Platform — Thiết kế chiến lược (DDD)

> Thiết kế tầng chiến lược: actor, bounded context, context map, sở hữu dữ liệu,
> ngôn ngữ chung (ubiquitous language), và các domain event chảy giữa các context.
> Schema DB chi tiết thiết kế theo từng context, đúng lúc cần (domain model dẫn dắt schema).

## 1. Actor & mô hình xác thực (Auth)

| Actor | Là ai | Auth |
|---|---|---|
| **Meter (thiết bị)** | Công tơ gửi số đo → Ingestion | **Device auth**: API key / client certificate / device token (máy, KHÔNG phải login người) |
| **Người dùng** | Nhân viên ENGIE / khách hàng (dashboard, alert, hoá đơn) | **OIDC login → JWT**, theo role |
| **Service ↔ Service** | Các service gọi nhau | service token / mTLS |

Xác thực giao cho **Identity Provider (IdP)**: **Keycloak** (local/học) → **Azure Entra ID** (prod ENGIE). Service chỉ **kiểm JWT**, không lưu mật khẩu.

## 2. Bounded Contexts

1. **Ingestion** — cửa nhận số đo từ meter. Xác thực thiết bị, validate, publish `MeterReadingReceived`. **KHÔNG sở hữu DB nghiệp vụ** (cửa nhận mỏng; sau có thể thêm inbox table cho idempotency). Đầu nguồn của mọi thứ.
2. **Metering / Time-Series** — consume reading event; lưu raw + aggregate; tính load curve. Publish `ConsumptionAggregated`. **Sở hữu TimescaleDB**.
3. **Billing** — consume tiêu thụ đã tổng hợp; áp biểu giá (heures pleines/creuses); tạo hoá đơn qua outbox. **Sở hữu billing DB** (Postgres).
4. **Alerting** — consume reading/aggregate + tín hiệu bất thường; phát alert (mất điện, quá tải). **Sở hữu alerts DB**.
5. **Forecasting & Anomaly** (Python/FastAPI) — ML dự báo + phát hiện bất thường/gian lận. Publish `AnomalyDetected`. **Sở hữu model/output store**.
6. **Meter & Customer Registry** — dữ liệu tham chiếu nghiệp vụ: meter, customer, contract, tariff plan. **Sở hữu registry DB**. Các context khác tham chiếu qua ID.
7. **Identity & Access** (supporting) — xác thực & phân quyền, giao cho IdP (Keycloak → Entra ID). Quản lý user, role, device credential. Cross-cutting.
8. **Payments** — thu tiền cho hoá đơn qua PSP (**Stripe**, test mode). Tạo payment intent, nhận **webhook** khi thanh toán thành công, publish `PaymentSucceeded`. **Sở hữu payments DB**.

## 3. Context Map (tích hợp = event qua broker, KHÔNG share DB)

```
[Meter devices] --(device auth)--> Ingestion
                                      | publish: MeterReadingReceived
                                      v
                    ┌──────────── Broker (RabbitMQ) ────────────┐
                    v                          v                v
              Metering/TimeSeries          Alerting        (cùng nghe)
              (lưu raw+aggregate)          (mất điện/quá tải)
                    | publish: ConsumptionAggregated
                    v
              Billing  +  Forecasting --(publish: AnomalyDetected)--> Alerting

Meter&Customer Registry  --(tham chiếu qua meterId/customerId)-->  mọi context
Identity&Access (IdP)    --(JWT được kiểm bởi)-->                  mọi service
```

- **Pattern tích hợp:** Publish/Subscribe qua broker (event-driven, eventual consistency). Không share DB giữa context.
- **Registry**: tham chiếu qua ID (shared ID / conformist); dữ liệu mang theo trong event khi tiện.
- **Identity**: Open Host — mọi service kiểm JWT do IdP cấp.
- **Payments**: Billing publish `InvoiceGenerated` → Payments tạo Stripe PaymentIntent; Stripe gọi **webhook** khi thanh toán xong → Payments publish `PaymentSucceeded` → Billing đánh dấu invoice `Paid`. Stripe = external (test mode).

## 4. Sở hữu dữ liệu — **database-per-service**

Mỗi context sở hữu kho dữ liệu riêng; không có DB chung. Dữ liệu liên-context chảy qua event (event mang theo field consumer cần) hoặc tham chiếu qua ID + query. Ngoại lệ: Ingestion không sở hữu DB nghiệp vụ (nó publish).

## 5. Ngôn ngữ chung (Ubiquitous Language)

Meter · Reading · Index (chỉ số luỹ kế) · LoadCurve (courbe de charge) · Aggregate · Consumption · Tariff (HeuresPleines / HeuresCreuses) · Invoice · Alert · Anomaly · Customer · Contract.

## 6. Domain Events (hợp đồng trên broker)

- `MeterReadingReceived` { readingId, meterId, timestamp, kwh, receivedAt }
- `ConsumptionAggregated` { meterId, period, granularity, kwh }
- `AnomalyDetected` { meterId, type, severity, detectedAt }
- `InvoiceGenerated` { invoiceId, customerId, period, total }
- `PaymentSucceeded` { invoiceId, paymentId, amount, paidAt }
- `AlertRaised` { alertId, meterId, type, severity }

Event là hợp đồng có version — chỉ thêm field (additive); thay đổi phá vỡ = version event mới.

## 7. Trình tự build (giờ vs sau)

- **Giờ (P0 spine):** Ingestion → (RabbitMQ) → Metering. Chứng minh backbone event-driven.
- **MVP (mở rộng):** + Metering, Anomaly (AI), **Billing** (outbox), **Payments** (Stripe test). Registry dùng **seed data**.
- **Sau:** Alerting, Forecasting nâng cao, Registry CRUD, Identity.
- **Identity & Access:** lắp ở phase sau (JWT qua Keycloak → Entra ID). Giờ Ingestion để device-auth stub/mở trong dev.

## 8. Map sang .NET service / repo

Mỗi bounded context = 1 service riêng (solution riêng, layer Clean Architecture riêng, DB riêng). Repo `SmartMetering` hiện tại bắt đầu với context **Ingestion**.

## 9. Hạ tầng cross-cutting: Gateway & Service Discovery

**Service Discovery (east-west, service ↔ service):** dùng **Kubernetes Service DNS** built-in — service gọi nhau qua **tên Service** (vd `http://metering`, `rabbitmq`), K8s tự resolve + load-balance tới các pod. **KHÔNG dùng Zookeeper/Consul/Eureka** (thế giới trước K8s; Zookeeper xưa đi với Kafka — Kafka nay cũng bỏ qua KRaft). Đã áp dụng: app connect RabbitMQ qua tên `rabbitmq`.

**API Gateway (north-south, client → hệ):** cửa vào duy nhất cho client (frontend/meter), lo routing/TLS/auth/rate-limit/đôi khi API composition.
- Local/k3d: **Ingress + Traefik** (k3d có sẵn).
- Học .NET: **YARP** (reverse proxy Microsoft) / Ocelot.
- Prod ENGIE: **Azure API Management (APIM)** / Application Gateway.
- Thêm khi frontend gọi nhiều service; giai đoạn dev hiện dùng `kubectl port-forward`.
