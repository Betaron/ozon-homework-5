using AutoBogus;
using Bogus;
using Route256.Week5.Homework.PriceCalculator.Bll.Commands;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.Fakers;

public static class ClearCalculationHistoryCommandFaker
{
    private static readonly object Lock = new();

    private static readonly Faker<ClearCalculationHistoryCommand> Faker = new AutoFaker<ClearCalculationHistoryCommand>()
    .RuleFor(x => x.UserId, f => f.Random.Long(0L));

    public static ClearCalculationHistoryCommand Generate()
    {
        lock (Lock)
        {
            return Faker.Generate();
        }
    }

    public static ClearCalculationHistoryCommand WithUserId(
        this ClearCalculationHistoryCommand src,
        long userId)
    {
        return src with { UserId = userId };
    }

    public static ClearCalculationHistoryCommand WithCalculationIds(
        this ClearCalculationHistoryCommand src,
        long[] calculationIds)
    {
        return src with { CalculationIds = calculationIds };
    }
}
