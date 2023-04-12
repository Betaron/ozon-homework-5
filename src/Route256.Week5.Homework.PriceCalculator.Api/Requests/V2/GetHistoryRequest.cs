namespace Route256.Week5.Homework.PriceCalculator.Api.Requests.V2;

public record GetHistoryRequest(
    long UserId,
    int Take,
    int Skip,
    long[] CalculationIds);