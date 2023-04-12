using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Builders;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Extensions;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Fakers;
using Route256.Week5.Homework.TestingInfrastructure.Creators;
using Xunit;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.HandlersTests;

public class GetCalculationHistoryQueryHandlerTests
{
    [Fact]
    public async Task Handle_MakeAllCalls()
    {
        //arrange
        var userId = Create.RandomId();

        var command = GetCalculationHistoryQueryFaker.Generate()
            .WithUserId(userId);

        var queryModels = QueryCalculationModelFaker.Generate(5)
            .Select(x => x.WithUserId(userId))
            .ToArray();

        var filter = QueryCalculationFilterFaker.Generate()
            .WithUserId(userId)
            .WithLimit(command.Take)
            .WithOffset(command.Skip);

        var builder = new GetCalculationHistoryHandlerBuilder();
        builder.CalculationService
            .SetupQueryCalculations(queryModels);

        var handler = builder.Build();

        //act
        var result = await handler.Handle(command, default);

        //asserts
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(queryModels.Length);
        result.Items.Select(x => x.Price)
            .Should().IntersectWith(queryModels.Select(x => x.Price));
        result.Items.Select(x => x.Volume)
            .Should().IntersectWith(queryModels.Select(x => x.TotalVolume));
        result.Items.Select(x => x.Weight)
            .Should().IntersectWith(queryModels.Select(x => x.TotalWeight));
    }

    [Fact]
    public async Task Handle_NotEmptyIds_BelongsAnotherUser_Throws()
    {
        //arrange
        var userId = Create.RandomId();
        var anotherUserId = Create.RandomId();
        var goodsIds = Create.RandomId(3);
        var queryModels = QueryCalculationModelFaker.Generate()
            .Select(x => x
                .WithUserId(anotherUserId)
                .WithGoodsIds(goodsIds))
            .ToArray();
        var queryIdsModels = queryModels
            .Select(x => new QueryCalculationIdsModel(
                x.Id,
                x.UserId,
                x.GoodIds))
            .ToArray();

        var query = GetCalculationHistoryQueryFaker.Generate()
            .WithUserId(userId);

        var builder = new GetCalculationHistoryHandlerBuilder();
        builder.CalculationService
            .SetupQueryCalculationsIdsByCalculationsIds(queryIdsModels)
            .SetupCheckCalculationsNonExistence(Array.Empty<long>())
            .SetupQueryCalculations(queryModels);
        var handler = builder.Build();

        //act, assert
        var exeption = await Assert.ThrowsAsync<OneOrManyCalculationsBelongsToAnotherUserException>
            (() => handler.Handle(query, default));
        exeption.Body.Should().Be(Array.Empty<long>());
    }

    [Fact]
    public async Task Handle_NotEmptyIds_GoodsNotFound_Throws()
    {
        //arrange
        var userId = Create.RandomId();
        var goodsIds = Create.RandomId(3);
        var queryModels = QueryCalculationModelFaker.Generate()
            .Select(x => x
                .WithUserId(userId))
            .ToArray();
        var queryIdsModels = queryModels
            .Select(x => new QueryCalculationIdsModel(
                x.Id,
                x.UserId,
                x.GoodIds))
            .ToArray();

        var query = GetCalculationHistoryQueryFaker.Generate()
            .WithUserId(userId);

        var builder = new GetCalculationHistoryHandlerBuilder();
        builder.CalculationService
            .SetupQueryCalculationsIdsByCalculationsIds(queryIdsModels)
            .SetupCheckCalculationsNonExistence(goodsIds)
            .SetupQueryCalculations(queryModels);
        var handler = builder.Build();

        //act, assert
        var exeption = await Assert.ThrowsAsync<OneOrManyCalculationsNotFoundException>
            (() => handler.Handle(query, default));
        exeption.Body.Should().Be(Array.Empty<long>());
    }
}