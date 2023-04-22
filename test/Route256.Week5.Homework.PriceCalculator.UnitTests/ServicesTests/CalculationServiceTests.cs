using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using FluentAssertions;
using Route256.Week5.Homework.PriceCalculator.Bll.Services;
using Route256.Week5.Homework.PriceCalculator.Dal.Models;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Builders;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Extensions;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Fakers;
using Route256.Week5.Homework.TestingInfrastructure.Creators;
using Route256.Week5.Homework.TestingInfrastructure.Fakers;
using Xunit;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.ServicesTests;

public class CalculationServiceTests
{
    [Fact]
    public async Task SaveCalculation_Success()
    {
        // arrange
        const int goodsCount = 5;

        var userId = Create.RandomId();
        var calculationId = Create.RandomId();

        var goodModels = GoodModelFaker.Generate(goodsCount)
            .ToArray();

        var goods = goodModels
            .Select(x => GoodEntityV1Faker.Generate().Single()
                .WithUserId(userId)
                .WithHeight(x.Height)
                .WithWidth(x.Width)
                .WithLength(x.Length)
                .WithWeight(x.Weight))
            .ToArray();
        var goodIds = goods.Select(x => x.Id)
            .ToArray();

        var calculationModel = CalculationModelFaker.Generate()
            .Single()
            .WithUserId(userId)
            .WithGoods(goodModels);

        var calculations = CalculationEntityV1Faker.Generate(1)
            .Select(x => x
                .WithId(calculationId)
                .WithUserId(userId)
                .WithPrice(calculationModel.Price)
                .WithTotalWeight(calculationModel.TotalWeight)
                .WithTotalVolume(calculationModel.TotalVolume))
            .ToArray();

        var builder = new CalculationServiceBuilder();
        builder.CalculationRepository
            .SetupAddCalculations(new[] { calculationId })
            .SetupCreateTransactionScope();
        builder.GoodsRepository
            .SetupAddGoods(goodIds);

        var service = builder.Build();

        // act
        var result = await service.SaveCalculation(calculationModel, default);

        // assert
        result.Should().Be(calculationId);
        service.CalculationRepository
            .VerifyAddWasCalledOnce(calculations)
            .VerifyCreateTransactionScopeWasCalledOnce(IsolationLevel.ReadCommitted);
        service.GoodsRepository
            .VerifyAddWasCalledOnce(goods);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public void CalculatePriceByVolume_Success()
    {
        // arrange
        var goodModels = GoodModelFaker.Generate(5)
            .ToArray();

        var builder = new CalculationServiceBuilder();
        var service = builder.Build();

        //act
        var price = service.CalculatePriceByVolume(goodModels, out var volume);

        //asserts
        volume.Should().BeApproximately(goodModels.Sum(x => x.Height * x.Width * x.Length), 1e-9d);
        price.Should().Be((decimal)volume * CalculationService.VolumeToPriceRatio);
    }

    [Fact]
    public void CalculatePriceByWeight_Success()
    {
        // arrange
        var goodModels = GoodModelFaker.Generate(5)
            .ToArray();

        var builder = new CalculationServiceBuilder();
        var service = builder.Build();

        //act
        var price = service.CalculatePriceByWeight(goodModels, out var weight);

        //asserts
        weight.Should().Be(goodModels.Sum(x => x.Weight));
        price.Should().Be((decimal)weight * CalculationService.WeightToPriceRatio);
    }

    [Fact]
    public async Task QueryCalculations_Success()
    {
        // arrange
        var userId = Create.RandomId();

        var filter = QueryCalculationFilterFaker.Generate()
            .WithUserId(userId)
            .WithCalculationIds(Array.Empty<long>());

        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId))
            .ToArray();

        var queryModel = CalculationHistoryQueryModelFaker.Generate()
            .WithUserId(userId)
            .WithLimit(filter.Limit)
            .WithOffset(filter.Offset)
            .WithCalculationIds(Array.Empty<long>());

        var builder = new CalculationServiceBuilder();
        builder.CalculationRepository
            .SetupQueryCalculation(calculations);
        var service = builder.Build();

        //act
        var result = await service.QueryCalculations(filter, default);

        //asserts
        service.CalculationRepository
            .VerifyQueryWasCalledOnce(queryModel);

        service.VerifyNoOtherCalls();

        result.Should().NotBeEmpty();
        result.Should().OnlyContain(x => x.UserId == userId);
        result.Should().OnlyContain(x => x.Id > 0);
        result.Select(x => x.TotalWeight)
            .Should().IntersectWith(calculations.Select(x => x.TotalWeight));
        result.Select(x => x.TotalVolume)
            .Should().IntersectWith(calculations.Select(x => x.TotalVolume));
        result.Select(x => x.Price)
            .Should().IntersectWith(calculations.Select(x => x.Price));
    }

    [Theory]
    [InlineData(5, 0, 3, 3)]
    [InlineData(4, 2, 3, 1)]
    [InlineData(0, 5, 3, 0)]
    [InlineData(5, 4, 3, 0)]
    public async Task QueryCalculations_NotEmptyIds_Success(int take, int skip, int idsCount, int expectedCount)
    {
        // arrange
        var userId = Create.RandomId();

        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x
            .WithUserId(userId)
            .WithId(Create.RandomId()))
            .ToArray();

        var expectedCalculations = calculations.Take(idsCount).Skip(skip);

        var queryIds = calculations
            .Take(idsCount)
            .Select(x => x.Id)
            .ToArray();

        var filter = QueryCalculationFilterFaker.Generate()
            .WithUserId(userId)
            .WithLimit(take)
            .WithOffset(skip)
            .WithCalculationIds(queryIds);

        var queryModel = CalculationHistoryQueryModelFaker.Generate()
            .WithUserId(userId)
            .WithLimit(filter.Limit)
            .WithOffset(filter.Offset)
            .WithCalculationIds(queryIds);

        var builder = new CalculationServiceBuilder();
        builder.CalculationRepository
            .SetupQueryCalculation(calculations.Take(take).Skip(skip).ToArray());
        var service = builder.Build();

        //act
        var result = await service.QueryCalculations(filter, default);

        //asserts
        service.CalculationRepository
            .VerifyQueryWasCalledOnce(queryModel);

        service.VerifyNoOtherCalls();

        result.Should().HaveCount(expectedCount);
    }

    [Fact]
    public async Task Query_CalculationsIds_SelectByUserId_Success()
    {
        // arrange
        var userId = Create.RandomId();
        var calculationsIds = CalculationIdsModelFaker.Generate(3)
            .Select(x => x
                .WithUserId(userId))
            .ToArray();

        var builder = new CalculationServiceBuilder();
        builder.CalculationRepository
            .SetupQueryCalculationByUserId(calculationsIds);
        var service = builder.Build();

        //act
        var result = await service.QueryCalculationsIds(userId, default);

        //asserts
        service.CalculationRepository
            .VerifyQueryIdsWasCalledOnce(userId);

        service.VerifyNoOtherCalls();

        result.Should().NotBeEmpty();
        result.Should().OnlyContain(x => x.UserId == userId);
        result.Should().OnlyContain(x => x.Id > 0);
        result.Select(x => x.Id)
            .Should().IntersectWith(calculationsIds.Select(x => x.Id));
        result.Select(x => x.UserId)
            .Should().IntersectWith(calculationsIds.Select(x => x.UserId));
        result.Select(x => x.GoodIds)
            .Should().IntersectWith(calculationsIds.Select(x => x.GoodIds));
    }

    [Fact]
    public async Task Query_CalculationsIds_SelectByCalculationIds_Success()
    {
        // arrange
        var userId = Create.RandomId();
        var calculationsIds = CalculationIdsModelFaker.Generate(3)
            .Select(x => x
                .WithUserId(userId)
                .WithId(Create.RandomId()))
            .ToArray();
        var ids = calculationsIds.Select(x => x.Id).ToArray();

        var builder = new CalculationServiceBuilder();
        builder.CalculationRepository
            .SetupQueryCalculationByCalculationIds(calculationsIds);
        var service = builder.Build();

        //act
        var result = await service.QueryCalculationsIds(ids, default);

        //asserts
        service.CalculationRepository
            .VerifyQueryIdsWasCalledOnce(ids);

        service.VerifyNoOtherCalls();

        result.Should().NotBeEmpty();
        result.Should().OnlyContain(x => x.UserId == userId);
        result.Should().OnlyContain(x => x.Id > 0);
        result.Select(x => x.Id)
            .Should().IntersectWith(calculationsIds.Select(x => x.Id));
        result.Select(x => x.UserId)
            .Should().IntersectWith(calculationsIds.Select(x => x.UserId));
        result.Select(x => x.GoodIds)
            .Should().IntersectWith(calculationsIds.Select(x => x.GoodIds));
    }

    [Fact]
    public async Task Delete_Calculations_Success()
    {
        // arrange
        var userId = Create.RandomId();
        var calculationsIds = CalculationIdsModelFaker.Generate(3)
            .Select(x => new Bll.Models.QueryCalculationIdsModel(Create.RandomId(), userId, x.GoodIds))
            .ToArray();

        var builder = new CalculationServiceBuilder();
        var service = builder.Build();

        //act
        await service.DeleteCalculations(calculationsIds, default);

        //assert
        service.CalculationRepository.VerifyDeleteCascadeWasCalledOnce(
            calculationsIds.Select(x => new CalculationIdsModel
            {
                Id = x.Id,
                UserId = x.UserId,
                GoodIds = x.GoodIds,
            }).ToArray());
    }

    [Fact]
    public async Task Query_Calculations_NonExistance_Success()
    {
        // arrange
        var userId = Create.RandomId();
        var calculationsIds = CalculationIdsModelFaker.Generate(3)
            .Select(x => x
                .WithUserId(userId)
                .WithId(Create.RandomId()))
            .ToArray();
        var ids = calculationsIds.Select(x => x.Id).ToArray();
        var nonExistingIds = Create.RandomId(3);

        var builder = new CalculationServiceBuilder();
        builder.CalculationRepository
            .SetupQueryCalculationByCalculationIds(calculationsIds);
        var service = builder.Build();

        var ExistingAndNonExistingIds = nonExistingIds.Union(ids).ToArray();

        //act
        var result = await service.CheckCalculationsNonExistence(ExistingAndNonExistingIds, default);

        //assert
        result.Should().IntersectWith(nonExistingIds);
    }
}
