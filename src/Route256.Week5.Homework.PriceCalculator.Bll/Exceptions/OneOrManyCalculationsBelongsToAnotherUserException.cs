namespace Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;

public class OneOrManyCalculationsBelongsToAnotherUserException : Exception
{
    public long[] WrongCalculationIds { get; }

    public OneOrManyCalculationsBelongsToAnotherUserException(long[] wrongCalculationIds)
        : base("Один или несколько расчетов принадлежат другому пользователю")
    {
        WrongCalculationIds = wrongCalculationIds;
    }
}
