namespace Route256.Week5.Homework.PriceCalculator.Bll.Models;

public record QueryCalculationIdsModel(
        long Id,
        long UserId,
        long[] GoodIds);
