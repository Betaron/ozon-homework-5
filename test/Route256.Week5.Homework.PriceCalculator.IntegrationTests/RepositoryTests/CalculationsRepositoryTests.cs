using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Route256.Week5.Homework.PriceCalculator.Dal.Models;
using Route256.Week5.Homework.PriceCalculator.Dal.Repositories.Interfaces;
using Route256.Week5.Homework.PriceCalculator.IntegrationTests.Fixtures;
using Route256.Week5.Homework.TestingInfrastructure.Creators;
using Route256.Week5.Homework.TestingInfrastructure.Fakers;
using Xunit;

namespace Route256.Week5.Homework.PriceCalculator.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class CalculationsRepositoryTests
{
    private readonly double _requiredDoublePrecision = 0.00001d;
    private readonly decimal _requiredDecimalPrecision = 0.00001m;
    private readonly TimeSpan _requiredDateTimePrecision = TimeSpan.FromMilliseconds(100);

    private readonly ICalculationRepository _calculationRepository;
    private readonly IGoodsRepository _goodsRepository;

    public CalculationsRepositoryTests(TestFixture fixture)
    {
        _calculationRepository = fixture.CalculationRepository;
        _goodsRepository = fixture.GoodsRepository;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task Add_Calculations_Success(int count)
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate(count)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();

        // Act
        var calculationIds = await _calculationRepository.Add(calculations, default);

        // Asserts
        calculationIds.Should().HaveCount(count);
        calculationIds.Should().OnlyContain(x => x > 0);
    }

    [Fact]
    public async Task Query_SingleCalculation_Success()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate()
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();
        var expected = calculations.Single();

        var calculationId = (await _calculationRepository.Add(calculations, default))
            .Single();

        // Act
        var foundCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(userId, 1, 0),
            default);

        // Asserts
        foundCalculations.Should().HaveCount(1);
        var calculation = foundCalculations.Single();

        calculation.Id.Should().Be(calculationId);
        calculation.UserId.Should().Be(expected.UserId);
        calculation.At.Should().BeCloseTo(expected.At, _requiredDateTimePrecision);
        calculation.Price.Should().BeApproximately(expected.Price, _requiredDecimalPrecision);
        calculation.GoodIds.Should().BeEquivalentTo(expected.GoodIds);
        calculation.TotalVolume.Should().BeApproximately(expected.TotalVolume, _requiredDoublePrecision);
        calculation.TotalWeight.Should().BeApproximately(expected.TotalWeight, _requiredDoublePrecision);
    }

    [Theory]
    [InlineData(3, 2, 3)]
    [InlineData(1, 6, 1)]
    [InlineData(2, 8, 2)]
    [InlineData(3, 10, 0)]
    public async Task Query_CalculationsInRange_Success(int take, int skip, int expectedCount)
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate(10)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();

        await _calculationRepository.Add(calculations, default);

        var allCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(userId, 100, 0),
            default);

        var expected = allCalculations
            .OrderByDescending(x => x.At)
            .Skip(skip)
            .Take(take);

        // Act
        var foundCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(userId, take, skip),
            default);

        // Asserts
        foundCalculations.Should().HaveCount(expectedCount);

        if (expectedCount > 0)
        {
            foundCalculations.Should().BeEquivalentTo(expected);
        }
    }

    [Fact]
    public async Task Query_AllCalculations_Success()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();

        var calculationIds = (await _calculationRepository.Add(calculations, default))
            .ToHashSet();

        // Act
        var foundCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(userId, 100, 0),
            default);

        // Assert
        foundCalculations.Should().NotBeEmpty();
        foundCalculations.Should().OnlyContain(x => x.UserId == userId);
        foundCalculations.Should().OnlyContain(x => calculationIds.Contains(x.Id));
        foundCalculations.Should().BeInDescendingOrder(x => x.At);
    }

    [Fact]
    public async Task Query_Calculations_ReturnsEmpty_WhenForWrongUser()
    {
        // Arrange
        var userId = Create.RandomId();
        var anotherUserId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();

        var calculationIds = (await _calculationRepository.Add(calculations, default))
            .ToHashSet();

        // Act
        var foundCalculations = await _calculationRepository.Query(
            new CalculationHistoryQueryModel(anotherUserId, 100, 0),
            default);

        // Assert
        foundCalculations.Should().BeEmpty();
    }

    [Fact]
    public async Task Query_CalculationsIdsModels_SelectByUserId_Success()
    {
        // Arrange
        var userId = Create.RandomId();
        var anotherUserId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();

        var anotherCalculations = CalculationEntityV1Faker.Generate(3)
            .Select(x => x.WithAt(now))
            .ToArray();

        var calculationIds = (await _calculationRepository.Add(calculations, default))
            .ToHashSet();

        await _calculationRepository.Add(anotherCalculations, default);

        // Act
        var foundCalculations = await _calculationRepository.QueryIds(
            userId,
            default);

        // Assert
        foundCalculations.Should().NotBeEmpty();
        foundCalculations.Should().OnlyContain(x => x.UserId == userId);
        foundCalculations.Should().OnlyContain(x => calculationIds.Contains(x.Id));
    }

    [Fact]
    public async Task Query_CalculationsIdsModels_SelectByCalculationIds_Success()
    {
        // Arrange
        var userId = Create.RandomId();
        var anotherUserId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate(5)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();

        var anotherCalculations = CalculationEntityV1Faker.Generate(2)
            .Select(x => x.WithAt(now))
            .ToArray();

        var calculationIds = (await _calculationRepository.Add(calculations, default))
            .ToHashSet().Take(3).ToArray();

        await _calculationRepository.Add(anotherCalculations, default);

        // Act
        var foundCalculations = await _calculationRepository.QueryIds(
            calculationIds,
            default);

        // Assert
        foundCalculations.Should().NotBeEmpty();
        foundCalculations.Should().OnlyContain(x => x.UserId == userId);
        foundCalculations.Should().OnlyContain(x => calculationIds.Contains(x.Id));
    }

    [Fact]
    public async Task Delete_Calculations_Success()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var calculations = CalculationEntityV1Faker.Generate(3)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();

        var calculationIds = (await _calculationRepository.Add(calculations, default))
            .ToHashSet().Take(2).ToArray();

        // Act
        await _calculationRepository.Delete(
            calculationIds,
            default);

        // Assert
        var foundCalculations = await _calculationRepository.QueryIds(
        userId,
        default);

        foundCalculations.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteCascade_Calculations_Success()
    {
        // Arrange
        var userId = Create.RandomId();
        var now = DateTimeOffset.UtcNow;

        var goods = GoodEntityV1Faker.Generate(3)
            .Select(x => x
                .WithUserId(userId))
            .ToArray();

        var calculations = CalculationEntityV1Faker.Generate(3)
            .Select(x => x.WithUserId(userId)
                .WithAt(now))
            .ToArray();

        var calculationIds = (await _calculationRepository.Add(calculations, default))
            .ToHashSet().Take(2).ToArray();
        var goodsIds = (await _goodsRepository.Add(goods, default))
            .ToHashSet().Take(2).ToArray();

        var length = calculationIds.Length;
        var calculationIdsModels = new CalculationIdsModel[length];
        for (int i = 0; i < length; i++)
        {
            calculationIdsModels[i] = new CalculationIdsModel()
            {
                Id = calculationIds[i],
                UserId = userId,
                GoodIds = new long[] { goodsIds[i] }
            };
        }

        // Act
        await _calculationRepository.DeleteCascade(
            calculationIdsModels,
            default);

        // Assert
        var foundCalculations = await _calculationRepository.QueryIds(
        userId,
        default);

        var foundGoods = await _goodsRepository.Query(
        userId,
        default);

        foundCalculations.Should().HaveCount(1);
        foundGoods.Should().HaveCount(1);
    }
}
