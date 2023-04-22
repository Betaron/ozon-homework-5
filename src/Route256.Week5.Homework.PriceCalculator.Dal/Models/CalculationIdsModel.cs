namespace Route256.Week5.Homework.PriceCalculator.Dal.Models;

public record CalculationIdsModel
{
    public long Id { get; init; }

    public long UserId { get; init; }

    public long[] GoodIds { get; init; } = Array.Empty<long>();
}
