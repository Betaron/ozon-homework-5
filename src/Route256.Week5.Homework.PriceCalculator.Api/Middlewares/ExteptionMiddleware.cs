using Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;

namespace Route256.Week5.Homework.PriceCalculator.Api.Middlewares;

public class ExceptionMiddleware
{
    public readonly RequestDelegate next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (OneOrManyCalculationsBelongsToAnotherUserException ex)
        {
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(
                new { wrong_calculation_ids = ex.WrongCalculationIds });
        }
        catch (OneOrManyCalculationsNotFoundException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
        catch (Exception)
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(
                new { Message = "Возникла внутренняя ошибка" });
        }
    }

}