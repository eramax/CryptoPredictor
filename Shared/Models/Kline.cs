namespace Shared.Models
{
    public record Kline(decimal open, decimal low, decimal high, decimal close, decimal volume, long timestamp, decimal turnover);
}
