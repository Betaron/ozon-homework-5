using Moq;
using Route256.Week5.Homework.PriceCalculator.Bll.Services.Interfaces;
using Route256.Week5.Homework.PriceCalculator.UnitTests.Stubs;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.Builders;

public class ClearCalculationHistoryHandlerBuilder
{
    public Mock<ICalculationService> CalculationService;

    public ClearCalculationHistoryHandlerBuilder()
    {
        CalculationService = new Mock<ICalculationService>();
    }

    public ClearCalculationHistoryHandlerStub Build()
    {
        return new ClearCalculationHistoryHandlerStub(
            CalculationService);
    }
}
