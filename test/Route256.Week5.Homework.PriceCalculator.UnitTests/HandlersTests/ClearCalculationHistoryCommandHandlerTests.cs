using System;
using System.Linq;
using System.Threading.Tasks;
using Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Builders;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Extensions;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Fakers;
using Route256.Week5.Homework.TestingInfrastructure.Creators;
using Xunit;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.HandlersTests;

public class ClearCalculationHistoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_EmptyIds_Success()
    {
        //arrange
        var userId = Create.RandomId();
        var calculationIds = QueryCalculationIdsModelFaker.Generate(3)
            .Select(x => x.WithUserId(userId)).ToArray();

        var command = ClearCalculationHistoryCommandFaker.Generate()
            .WithUserId(userId)
            .WithGoods(Array.Empty<long>());

        var builder = new ClearCalculationHistoryHandlerBuilder();
        builder.CalculationService
            .SetupQueryCalculationsIdsByUserId(calculationIds);
        var handler = builder.Build();

        //act
        await handler.Handle(command, default);

        //assert
        handler.CalculationService.VerifyDeleteCalculationsWasCalledOnce(calculationIds);
    }

    [Fact]
    public async Task Handle_NotEmptyIds_Success()
    {
        //arrange
        var userId = Create.RandomId();
        var goodsIds = Create.RandomId(3);
        var calculationIds = QueryCalculationIdsModelFaker.Generate(3)
            .Select(x => x
                .WithUserId(userId)
                .WithGoods(goodsIds))
            .ToArray();
        var ids = calculationIds.Select(x => x.Id).ToArray();

        var command = ClearCalculationHistoryCommandFaker.Generate()
            .WithUserId(userId)
            .WithGoods(goodsIds);

        var builder = new ClearCalculationHistoryHandlerBuilder();
        builder.CalculationService
            .SetupQueryCalculationsIdsByCalculationsIds(calculationIds)
            .SetupCheckCalculationsNonExistence(Array.Empty<long>());
        var handler = builder.Build();

        //act
        await handler.Handle(command, default);

        //assert
        handler.CalculationService.VerifyDeleteCalculationsWasCalledOnce(calculationIds);
    }

    [Fact]
    public async Task Handle_NotEmptyIds_BelongsAnotherUser_Throws()
    {
        //arrange
        var userId = Create.RandomId();
        var anotherUserId = Create.RandomId();
        var goodsIds = Create.RandomId(3);
        var calculationIds = QueryCalculationIdsModelFaker.Generate()
            .Select(x => x
                .WithUserId(anotherUserId)
                .WithGoods(goodsIds))
            .ToArray();
        var ids = calculationIds.Select(x => x.Id).ToArray();

        var command = ClearCalculationHistoryCommandFaker.Generate()
            .WithUserId(userId)
            .WithGoods(goodsIds);

        var builder = new ClearCalculationHistoryHandlerBuilder();
        builder.CalculationService
            .SetupQueryCalculationsIdsByCalculationsIds(calculationIds)
            .SetupCheckCalculationsNonExistence(Array.Empty<long>());
        var handler = builder.Build();

        //act, assert
        await Assert.ThrowsAsync<OneOrManyCalculationsBelongsToAnotherUserException>
            (() => handler.Handle(command, default));
    }

    [Fact]
    public async Task Handle_NotEmptyIds_GoodsNotFound_Throws()
    {
        //arrange
        var userId = Create.RandomId();
        var goodsIds = Create.RandomId(3);
        var calculationIds = QueryCalculationIdsModelFaker.Generate(3)
            .Select(x => x
                .WithUserId(userId))
            .ToArray();
        var ids = calculationIds.Select(x => x.Id).ToArray();

        var command = ClearCalculationHistoryCommandFaker.Generate()
            .WithUserId(userId)
            .WithGoods(goodsIds);

        var builder = new ClearCalculationHistoryHandlerBuilder();
        builder.CalculationService
            .SetupQueryCalculationsIdsByCalculationsIds(calculationIds)
            .SetupCheckCalculationsNonExistence(goodsIds);
        var handler = builder.Build();

        //act, assert
        await Assert.ThrowsAsync<OneOrManyCalculationsNotFoundException>
            (() => handler.Handle(command, default));
    }
}
