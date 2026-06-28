using Microsoft.Extensions.Configuration;
using Stripe;
using SmartMetering.Payments.Application.Abstractions;

namespace SmartMetering.Payments.Infrastructure.Stripe;

public sealed class StripePaymentGateway : IPaymentGateway
{
    public StripePaymentGateway(IConfiguration config)
    {
        // key đọc từ k8s Secret -> env Stripe__SecretKey
        StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
    }

    public async Task<string> CreateAndConfirmAsync(Guid invoiceId, decimal amount, CancellationToken ct = default)
    {
        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = (long)Math.Round(amount * 100), // cents
            Currency = "eur",
            PaymentMethod = "pm_card_visa", // test payment method (test mode)
            PaymentMethodTypes = ["card"],
            Confirm = true, // tạo + thanh toán luôn -> succeeds (test) -> webhook
            Metadata = new Dictionary<string, string> { ["invoiceId"] = invoiceId.ToString() },
        }, cancellationToken: ct);

        return intent.Id;
    }
}
