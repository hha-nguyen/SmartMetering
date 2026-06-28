# Energy IoT Platform — Features & User Stories

> Góc nhìn sản phẩm (hệ thống làm gì, cho ai). Đi kèm góc nhìn kỹ thuật ở
> `../architecture/strategic-design.md`. Ưu tiên: **[MVP]** = phạm vi pilot, **[Sau]** = phase tương lai.

## Personas (actor)

- **Smart Meter (thiết bị)** — tự động gửi số đo tiêu thụ.
- **Operator (nhân viên ENGIE)** — giám sát tiêu thụ, mất điện, bất thường.
- **Customer (khách B2C/B2B)** — xem tiêu thụ, hoá đơn, thanh toán, cảnh báo của chính mình.
- **Billing Analyst** — tạo/duyệt hoá đơn, quản lý biểu giá.
- **Data Analyst** — dự báo, điều tra gian lận.
- **Admin** — quản lý meter, customer, user, quyền truy cập.

## Epic & User Story

### E1. Reading Ingestion  *(context: Ingestion)*
- **[MVP]** Là **smart meter**, tôi gửi số đo định kỳ (meterId, timestamp, kWh) để usage được ghi nhận.
- **[MVP]** Là **hệ thống**, tôi validate số đo (đúng định dạng, không âm) và từ chối dữ liệu xấu.
- **[Sau]** Là **hệ thống**, tôi xác thực thiết bị, để chỉ meter hợp lệ được gửi (chống gian lận).
- **[MVP]** Là **hệ thống**, tôi publish mỗi số đo hợp lệ thành event, để nhiều consumer xử lý được.

### E2. Lưu trữ & Lịch sử tiêu thụ  *(context: Metering/Time-Series)*
- **[MVP]** Là **hệ thống**, tôi lưu số đo raw bền vững (time-series).
- **[MVP]** Là **hệ thống**, tôi tổng hợp số đo (15 phút → giờ → ngày) để query nhanh.
- **[MVP]** Là **customer**, tôi xem t
iêu thụ dạng load curve theo thời gian.

### E3. Giám sát real-time & Cảnh báo  *(context: Metering, Alerting)*
- **[MVP]** Là **operator**, tôi xem tiêu thụ cập nhật real-time.
- **[Sau]** Là **operator**, tôi được cảnh báo khi meter mất tín hiệu/quá tải.

### E4. Phát hiện Bất thường & Gian lận  *(context: Forecasting/Anomaly — AI)*  ⭐ điểm nhấn pilot
- **[MVP]** Là **hệ thống**, tôi phát hiện số đo bất thường (tụt/vọt đột ngột) báo hiệu meter hỏng/gian lận.
- **[MVP]** Là **data analyst**, tôi xem các bất thường bị gắn cờ để điều tra.
- **[Sau]** Là **operator**, tôi xem dự báo tiêu thụ (predicted demand).

### E5. Billing  *(context: Billing)*  ⭐ trong MVP
- **[MVP]** Là **hệ thống**, tôi tạo hoá đơn từ tiêu thụ + biểu giá (heures pleines/creuses), **chính xác và idempotent** (không tính trùng) — dùng outbox.
- **[MVP]** Là **customer**, tôi xem hoá đơn của mình + trạng thái (Unpaid/Paid).
- **[Sau]** Là **billing analyst**, tôi quản lý biểu giá (CRUD).

### E8. Online Payment  *(context: Payments — Stripe test mode)*  ⭐ trong MVP
- **[MVP]** Là **customer**, tôi thanh toán hoá đơn online (Stripe **test mode**, tiền giả) để trả nợ tiền điện.
- **[MVP]** Là **hệ thống**, tôi nhận **webhook** từ Stripe khi thanh toán thành công và cập nhật hoá đơn → `Paid`, **idempotent** (webhook có thể bắn nhiều lần).
- **[Sau]** Là **customer**, tôi xem lịch sử thanh toán + hoàn tiền (refund).

### E6. Quản lý Meter & Customer  *(context: Registry)*
- **[Sau]** Là **admin**, tôi đăng ký/ngừng meter và gắn với customer + contract.
- **[Sau]** Là **admin**, tôi quản lý contract và gán biểu giá.
- *MVP dùng dữ liệu seed (hardcode meter↔customer + 1 tariff) thay cho Registry CRUD đầy đủ.*

### E7. Identity & Access  *(context: Identity)*
- **[Sau]** Là **user**, tôi đăng nhập an toàn (OIDC), chỉ thấy thứ role cho phép.
- **[Sau]** Là **meter**, tôi xác thực như một thiết bị.

## MVP (phạm vi pilot — đã chốt)

Chứng minh **backbone khó** (real-time, scale, distributed) + **AI** + **vòng đời tiền** (billing → payment):

1. **E1 Ingestion** — nhận + validate + publish.
2. **E2 Storage** — consume → lưu raw + aggregate (TimescaleDB).
3. **E3 Real-time** — dashboard live (SignalR).
4. **E4 Anomaly** ⭐ — gắn cờ bất thường (AI).
5. **E5 Billing** ⭐ — tiêu thụ → hoá đơn (outbox, idempotent).
6. **E8 Payments** ⭐ — thanh toán Stripe test + webhook (idempotent).

**Phụ thuộc:** Billing cần dữ liệu meter↔customer + tariff → MVP dùng **seed data** (không cần Registry CRUD đầy đủ). Identity (E7) + Registry CRUD (E6) **hoãn** — stub auth trong dev.

> Ghi chú: MVP này khá lớn (6 epic). Build **tuần tự từng context**, không song song. Stripe ở **test mode** (tiền giả, không PCI).

## Map: feature → context → phase build

| Epic | Context | Phase |
|---|---|---|
| E1 Ingestion | Ingestion | P0 (đang làm) |
| E2 Storage/Aggregate | Metering | P1 |
| E3 Real-time | Metering + Realtime (SignalR) | P2 |
| E4 Anomaly | Forecasting (Python) | kéo sớm cho pilot |
| E5 Billing | Billing | P3 |
| E8 Payments | Payments (Stripe) | sau Billing |
| E6 Registry | Registry | sau (MVP dùng seed) |
| E7 Identity | Identity | sau MVP |
