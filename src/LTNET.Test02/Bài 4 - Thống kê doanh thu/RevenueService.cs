namespace LTNET.Test02.Analytics;

public sealed record RevenueRequest(DateTime From, DateTime To, string Branch);
public sealed record RevenueTransaction(string TransactionId, DateTime OccurredAt, string Branch, decimal Amount);
public sealed record RevenueProgress(int Percent, string Message);

public static class RevenueService
{
    public static readonly string[] Branches = ["Tất cả", "Hà Nội", "Đà Nẵng", "TP. Hồ Chí Minh"];

    public static void Validate(RevenueRequest request)
    {
        if (request.From.Date > request.To.Date) throw new ArgumentException("Từ ngày không được sau Đến ngày.");
        if (!Branches.Contains(request.Branch)) throw new ArgumentException("Chi nhánh không hợp lệ.");
    }

    public static async Task<List<RevenueTransaction>> GenerateAsync(RevenueRequest request, IProgress<RevenueProgress>? progress, CancellationToken token)
    {
        Validate(request);
        var result = new List<RevenueTransaction>();
        int days = (request.To.Date - request.From.Date).Days + 1;
        // Simulate a 5-second database query and aggregation; no UI controls are touched here.
        for (int step = 1; step <= 25; step++)
        {
            token.ThrowIfCancellationRequested();
            await Task.Delay(200, token).ConfigureAwait(false);
            for (int row = 0; row < 12; row++)
            {
                token.ThrowIfCancellationRequested();
                int index = (step - 1) * 12 + row;
                string branch = Branches[1 + index % 3];
                if (request.Branch != "Tất cả" && request.Branch != branch) continue;
                result.Add(new($"GD{index + 1:D5}", request.From.Date.AddDays(index % days).AddHours(8 + index % 12), branch, 100_000m + index % 20 * 50_000m));
            }
            progress?.Report(new(step * 4, $"Đang tổng hợp: {step * 4}% ({result.Count:N0} giao dịch)"));
        }
        token.ThrowIfCancellationRequested();
        return result.OrderBy(r => r.OccurredAt).ThenBy(r => r.TransactionId).ToList();
    }
}
