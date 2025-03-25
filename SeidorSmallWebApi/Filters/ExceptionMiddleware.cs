using SeidorSmallWebApi.Exceptions;
using SEIDOR_SLayer;
using SeidorSmallWebApi.Extensions;
using SeidorSmallWebApi.Routes;
using System.Text.Json;

namespace SeidorSmallWebApi.Filters;

public class ExceptionMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly ITokenManager _tokenManager;

    public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger, ITokenManager tokenManager)
    {
        _logger = logger;
        _tokenManager = tokenManager;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            if (context.Request.Path.Value.Equals($"/{ApiRoutes.BaseRoute}/Login"))
            {
                await next(context);
            }
            else
            {
                if (await _tokenManager.IsCurrentActiveToken())
                {
                    await next(context);
                }
                else
                {
                    throw new WebApiException("Invalid Token.");
                }
            }
        }
        catch (SLException ex)
        {
            _logger.LogError("SLException: {Exception}", ex.Message);
            await HandleExceptionAsync(context, ex, 0);
        }
        catch (GWException ex)
        {
            _logger.LogError("GWException: {Exception}", ex.Message);
            await HandleExceptionAsync(context, ex, ex.Code);
        }
        catch (WebApiException ex)
        {
            _logger.LogError("WebApiException: {Exception}", ex.Message);
            await HandleExceptionAsync(context, ex, 0);
        }
        catch (NotFoundException ex)
        {
            _logger.LogError("NotFoundException: {Exception}", ex.Message);
            await HandleExceptionAsync(context, ex, 0);
        }
        catch (EntityAlreadyExistsException ex)
        {
            _logger.LogError("EntityAlreadyExistsException: {Exception}", ex.Message);
            await HandleExceptionAsync(context, ex, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError("General exception: {Exception}", ex.Message);
            await HandleExceptionAsync(context, ex, 0);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, int code)
    {
        context.Response.ContentType = "application/json";

        // Gateway exceptions are handled different, since ApiGateway returns 200 always, and the error in the response
        // Maybe that will be fixed in the future, since now it seems wrong
        context.Response.StatusCode = exception switch
        {
            SLException => StatusCodes.Status400BadRequest,
            GWException => code,
            WebApiException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            EntityAlreadyExistsException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(exception.Message));
    }
}
