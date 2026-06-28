namespace SmartMetering.Billing.Application.Abstractions;

public interface IInvoiceGenerator
{
    // sinh hoá đơn cho phần tiêu thụ tích luỹ của meter; null nếu chưa có gì để tính
    Task<Guid?> GenerateAsync(string meterId, CancellationToken ct = default);
}
