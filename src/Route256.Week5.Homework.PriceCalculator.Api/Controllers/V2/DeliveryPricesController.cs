using MediatR;
using Microsoft.AspNetCore.Mvc;
using Route256.Week5.Homework.PriceCalculator.Api.Requests.V2;
using Route256.Week5.Homework.PriceCalculator.Api.Responses.V2;
using Route256.Week5.Homework.PriceCalculator.Bll.Queries;

namespace Route256.Week5.Homework.PriceCalculator.Api.Controllers.V2;

[ApiController]
[Route("/v2/delivery-prices")]
public class DeliveryPricesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeliveryPricesController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Метод получения истории вычисления
    /// </summary>
    /// <returns></returns>
    [HttpPost("get-history")]
    public async Task<GetHistoryResponse[]> History(
        GetHistoryRequest request,
        CancellationToken ct)
    {
        var query = new GetCalculationHistoryQuery(
            request.UserId,
            request.Take,
            request.Skip,
            request.CalculationIds);
        var result = await _mediator.Send(query, ct);

        return result.Items
            .Select(x => new GetHistoryResponse(
                new GetHistoryResponse.CargoResponse(
                    x.Volume,
                    x.Weight,
                    x.GoodIds),
                x.Price))
            .ToArray();
    }
}