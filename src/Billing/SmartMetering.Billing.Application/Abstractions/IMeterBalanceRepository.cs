namespace SmartMetering.Billing.Application.Abstractions;

public interface IMeterBalanceRepository
{
    // cộng dồn kWh vào balance của meter (tạo mới nếu chưa có)
    Task AddConsumptionAsync(string meterId, decimal kwh, CancellationToken ct = default);
}
