using MediatR;
using Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;
using Route256.Week5.Homework.PriceCalculator.Bll.Services.Interfaces;

namespace Route256.Week5.Homework.PriceCalculator.Bll.Commands;
public record ClearCalculationHistoryCommand(
    long UserId,
    long[] CalculationIds)
    : IRequest;

public class ClearCalculationHistoryCommandHandler
    : IRequestHandler<ClearCalculationHistoryCommand>
{
    private readonly ICalculationService _calculationService;

    public ClearCalculationHistoryCommandHandler(
        ICalculationService calculationService)
    {
        _calculationService = calculationService;
    }

    public async Task Handle(
    ClearCalculationHistoryCommand request,
    CancellationToken ct)
    {
        var pendingIds = request.CalculationIds.Any() ?
            await _calculationService.QueryCalculationsIds(request.CalculationIds, ct) :
            await _calculationService.QueryCalculationsIds(request.UserId, ct);

        if (request.CalculationIds.Any())
        {
            var nonExistenceIds = request.CalculationIds.Except(pendingIds.Select(x => x.Id)).ToArray();
            if (nonExistenceIds.Any())
            {
                throw new OneOrManyCalculationsNotFoundException();
            }

            var notBelongingIds = pendingIds
                .Where(x => x.UserId != request.UserId)
                .Select(x => x.Id).ToArray();
            if (notBelongingIds.Any())
            {
                throw new OneOrManyCalculationsBelongsToAnotherUserException(
                    new { wrong_calculation_ids = notBelongingIds });
            }
        }

        await _calculationService.DeleteCalculations(pendingIds, ct);
    }
}