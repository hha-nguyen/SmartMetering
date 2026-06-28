using SmartMetering.Billing.Application.Abstractions;
using SmartMetering.Billing.Application.Readings;

namespace SmartMetering.Billing.Application.Consumption;

// use case: nhận số đo -> cộng dồn vào balance (chờ xuất hoá đơn)
public sealed class AccumulateConsumptionHandler(IMeterBalanceRepository balances)
{
    public Task HandleAsync(MeterReadingReceived evt, CancellationToken ct = default)
        => balances.AddConsumptionAsync(evt.MeterId, evt.Kwh, ct);
}
