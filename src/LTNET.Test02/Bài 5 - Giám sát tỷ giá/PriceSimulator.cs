namespace LTNET.Test02.Ticker;

public sealed record PriceTick(DateTime Timestamp, decimal PreviousPrice, decimal Price)
{
    public decimal Change => Price - PreviousPrice;
    public string Direction => Change > 0 ? "Tăng" : Change < 0 ? "Giảm" : "Không đổi";
}

public static class PriceSimulator
{
    public static PriceTick Next(decimal previous, DateTime timestamp, Random random)
    {
        if (previous <= 0) throw new ArgumentOutOfRangeException(nameof(previous));
        decimal factor = 1 + (decimal)(random.NextDouble() - 0.5) * 0.02m;
        decimal price = Math.Max(0.01m, Math.Round(previous * factor, 2, MidpointRounding.AwayFromZero));
        return new(timestamp, previous, price);
    }
}
