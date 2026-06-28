using SmartMetering.Billing.Application.Abstractions;
using SmartMetering.Billing.Domain.Balances;

namespace SmartMetering.Billing.Infrastructure.Persistence;

public sealed class MeterBalanceRepository(BillingDbContext db) : IMeterBalanceRepository
{
    public async Task AddConsumptionAsync(string meterId, decimal kwh, CancellationToken ct = default)
    {
        var balance = await db.Balances.FindAsync([meterId], ct);
        if (balance is null)
        {
            balance = MeterBalance.Start(meterId);
            db.Balances.Add(balance);
        }

        balance.Add(kwh);
        await db.SaveChangesAsync(ct);
    }
}
