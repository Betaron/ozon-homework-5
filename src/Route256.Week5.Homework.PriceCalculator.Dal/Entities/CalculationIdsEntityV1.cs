namespace Route256.Week5.Homework.PriceCalculator.Dal.Entities;

public record CalculationIdsEntityV1
{
    public long Id { get; init; }

    public long UserId { get; init; }

    public long[] GoodIds { get; init; } = Array.Empty<long>();
}
