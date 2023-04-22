using System.Collections.Generic;
using System.Linq;
using AutoBogus;
using Bogus;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.Fakers;

public static class QueryCalculationIdsModelFaker
{
    private static readonly object Lock = new();

    private static readonly Faker<QueryCalculationIdsModel> Faker = new AutoFaker<QueryCalculationIdsModel>()
        .RuleFor(x => x.Id, f => f.Random.Long(0L))
        .RuleFor(x => x.UserId, f => f.Random.Long(0L));

    public static IEnumerable<QueryCalculationIdsModel> Generate(int count = 1)
    {
        lock (Lock)
        {
            return Enumerable.Repeat(Faker.Generate(), count);
        }
    }

    public static QueryCalculationIdsModel WithId(
    this QueryCalculationIdsModel src,
    long id)
    {
        return src with { Id = id };
    }

    public static QueryCalculationIdsModel WithUserId(
        this QueryCalculationIdsModel src,
        long userId)
    {
        return src with { UserId = userId };
    }

    public static QueryCalculationIdsModel WithGoods(
        this QueryCalculationIdsModel src,
        long[] goodsIds)
    {
        return src with { GoodIds = goodsIds };
    }
}
