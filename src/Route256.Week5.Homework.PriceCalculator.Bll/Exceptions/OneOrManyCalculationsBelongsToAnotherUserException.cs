namespace Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;

public class OneOrManyCalculationsBelongsToAnotherUserException : Exception
{
    public object? Body { get; }

    public OneOrManyCalculationsBelongsToAnotherUserException(object? body = null)
        : base("Один или несколько расчетов принадлежат другому пользователю")
    {
        Body = body;
    }
}
