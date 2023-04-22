using Moq;
using Route256.Week5.Homework.PriceCalculator.Bll.Commands;
using Route256.Week5.Homework.PriceCalculator.Bll.Services.Interfaces;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.Stubs;

public class ClearCalculationHistoryHandlerStub : ClearCalculationHistoryCommandHandler
{
    public Mock<ICalculationService> CalculationService { get; }

    public ClearCalculationHistoryHandlerStub(
        Mock<ICalculationService> calculationService)
        : base(calculationService.Object)
    {
        CalculationService = calculationService;
    }

    public void VerifyNoOtherCalls()
    {
        CalculationService.VerifyNoOtherCalls();
    }
}
