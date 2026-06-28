namespace SmartMetering.Billing.Infrastructure.Outbox;

// Cơ chế hạ tầng (không phải domain): 1 dòng = 1 event chờ publish.
public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = default!; // vd "InvoiceGenerated"
    public string Content { get; set; } = default!; // JSON payload
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; } // null = chưa publish
}
