using MediatR;
using Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;
using Route256.Week5.Homework.PriceCalculator.Bll.Services.Interfaces;

namespace Route256.Week5.Homework.PriceCalculator.Bll.Queries;

public record GetCalculationHistoryQuery(
    long UserId,
    int Take,
    int Skip,
    long[] CalculationIds)
    : IRequest<GetHistoryQueryResult>;

public class GetCalculationHistoryQueryHandler
    : IRequestHandler<GetCalculationHistoryQuery, GetHistoryQueryResult>
{
    private readonly ICalculationService _calculationService;

    public GetCalculationHistoryQueryHandler(
        ICalculationService calculationService)
    {
        _calculationService = calculationService;
    }

    public async Task<GetHistoryQueryResult> Handle(
        GetCalculationHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CalculationIds.Any())
        {
            var nonExistenceIds = await _calculationService.CheckCalculationsNonExistence(request.CalculationIds, cancellationToken);
            if (nonExistenceIds.Any())
            {
                throw new OneOrManyCalculationsNotFoundException(Array.Empty<long>());
            }

            var pendingIds = await _calculationService.QueryCalculationsIds(request.CalculationIds, cancellationToken);
            var notBelongingIds = pendingIds
                .Where(x => x.UserId != request.UserId)
                .Select(x => x.Id).ToArray();
            if (notBelongingIds.Any())
            {
                throw new OneOrManyCalculationsBelongsToAnotherUserException(Array.Empty<long>());
            }
        }

        var query = new QueryCalculationFilter(
            request.UserId,
            request.Take,
            request.Skip,
            request.CalculationIds);

        var log = await _calculationService.QueryCalculations(query, cancellationToken);

        return new GetHistoryQueryResult(
        log.Select(x => new GetHistoryQueryResult.HistoryItem(
                x.TotalVolume,
                x.TotalWeight,
                x.Price,
                x.GoodIds))
            .ToArray());
    }
}