using System.Collections.Generic;
using System.Linq;
using AutoBogus;
using Bogus;
using Route256.Week5.Homework.PriceCalculator.Dal.Models;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.Fakers;

public static class CalculationIdsModelFaker
{
    private static readonly object Lock = new();

    private static readonly Faker<CalculationIdsModel> Faker = new AutoFaker<CalculationIdsModel>()
        .RuleFor(x => x.Id, f => f.Random.Long(0L))
        .RuleFor(x => x.UserId, f => f.Random.Long(0L));

    public static IEnumerable<CalculationIdsModel> Generate(int count = 1)
    {
        lock (Lock)
        {
            return Enumerable.Repeat(Faker.Generate(), count);
        }
    }

    public static CalculationIdsModel WithId(
    this CalculationIdsModel src,
    long id)
    {
        return src with { Id = id };
    }

    public static CalculationIdsModel WithUserId(
        this CalculationIdsModel src,
        long userId)
    {
        return src with { UserId = userId };
    }

    public static CalculationIdsModel WithGoods(
        this CalculationIdsModel src,
        long[] goodsIds)
    {
        return src with { GoodIds = goodsIds };
    }
}
