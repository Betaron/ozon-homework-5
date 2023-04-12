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
            if (ex.Body is not null)
            {
                await httpContext.Response.WriteAsJsonAsync(ex.Body);
            }
        }
        catch (OneOrManyCalculationsNotFoundException ex)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            if (ex.Body is not null)
            {
                await httpContext.Response.WriteAsJsonAsync(ex.Body);
            }
        }
        catch (Exception)
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(
                new { Message = "Возникла внутренняя ошибка" });
        }
    }

}