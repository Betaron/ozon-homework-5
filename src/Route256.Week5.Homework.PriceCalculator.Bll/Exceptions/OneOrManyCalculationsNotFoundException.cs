namespace Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;

public class OneOrManyCalculationsNotFoundException : Exception
{
    public object? Body { get; }

    public OneOrManyCalculationsNotFoundException(object? body = null)
        : base("Один или несколько расчетов не найдены")
    {
        Body = body;
    }
}
