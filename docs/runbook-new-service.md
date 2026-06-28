# Runbook: Thêm một service (bounded context) mới

> Mỗi bounded context = 1 service riêng (4 layer Clean Architecture + DB riêng).
> Dùng cho mọi service mới: Billing, Alerting, Payments, Registry...

## A. Scaffold (1 lệnh — đã tự động hoá)

```bash
bash scripts/new-service.sh <Name>
```
Ví dụ: `bash scripts/new-service.sh Billing`

→ Tạo `src/<Name>/SmartMetering.<Name>.{Domain,Application,Infrastructure,Api}`, nối reference (chỉ trỏ vào trong), thêm vào solution, build.

## B. Viết code theo Clean Architecture (thứ tự trong → ngoài)

1. **Domain**: Entity / Value Object / Aggregate (tên thuần nghiệp vụ, không "Aggregate" trong tên). Factory `Create(...)` + guard clauses.
2. **Application**: use case (handler) + **port** (interface trong `Abstractions/`). DTO/event là `record`.
3. **Infrastructure**: hiện thực port (EF Core/Npgsql, RabbitMQ publisher/consumer...). Thêm package cần (`dotnet add <proj> package <X>`).
4. **Api**: endpoint (Minimal API) hoặc `BackgroundService` (consumer) + **đăng ký DI**:
   - `AddScoped<IPort, Impl>()` (map interface → impl)
   - `AddScoped<Handler>()` (đăng ký class cụ thể để DI tạo được)
   - validation: `AddProblemDetails()` + FluentValidation nếu có input.

## C. Đóng gói (Docker)

- Tạo `src/<Name>/SmartMetering.<Name>.Api/Dockerfile` (multi-stage SDK→chiseled — copy từ service có sẵn, đổi tên đường dẫn).
- `.dockerignore` đã có ở gốc solution (dùng chung).

## D. Triển khai K8s

- Tạo manifest `k8s/<name>.yaml` (Deployment + Service + probes + resources + non-root).
- Nếu service cần DB/broker riêng → manifest hạ tầng tương ứng (`k8s/<dep>.yaml`).
- Deploy:
```bash
docker build -f src/<Name>/SmartMetering.<Name>.Api/Dockerfile -t smartmetering-<name>:dev .
k3d image import smartmetering-<name>:dev -c smartmetering
kubectl apply -f k8s/<name>.yaml
kubectl get pods -w
```

## E. Test

- Health: `/health/live`, `/health/ready`.
- Endpoint/luồng: qua `kubectl port-forward` + Scalar/Bruno/curl.
- Event-driven: kiểm RabbitMQ UI (queue) / DB.

## Chu trình redeploy mỗi khi đổi code (1 service)

```bash
dotnet build
docker build -f src/<Name>/SmartMetering.<Name>.Api/Dockerfile -t smartmetering-<name>:dev .
k3d image import smartmetering-<name>:dev -c smartmetering
kubectl rollout restart deployment <name>
```

## Lưu ý (tránh lỗi đã gặp)

- **Paste lệnh dài bị wrap → vỡ lệnh.** Dùng script, hoặc gõ mỗi lệnh 1 dòng ngắn; không paste comment `#` vào shell.
- **YAML paste hay bị thụt dư + mất `---`** → để assistant viết file YAML.
- **Sau khi máy/Docker restart**: nếu kubectl không nối được cluster → `k3d cluster stop smartmetering && k3d cluster start smartmetering`.
- Solution dùng `.sln` (không `.slnx`) cho extension C# load đúng.
