using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace TeacherFind.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly ICorsService _corsService;
    private readonly ICorsPolicyProvider _corsPolicyProvider;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        ICorsService corsService,
        ICorsPolicyProvider corsPolicyProvider)
    {
        _next = next;
        _logger = logger;
        _corsService = corsService;
        _corsPolicyProvider = corsPolicyProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Hata yakalandı fakat response zaten başlamış. Hata cevabı yazılamadı.");
                throw;
            }

            context.Response.Clear();

            await ApplyCorsPolicyAsync(context);
            await WriteExceptionResponseAsync(context, ex);
        }
    }

    private async Task ApplyCorsPolicyAsync(HttpContext context)
    {
        var origin = context.Request.Headers["Origin"].ToString();

        if (string.IsNullOrWhiteSpace(origin))
            return;

        var policy = await _corsPolicyProvider.GetPolicyAsync(context, "Frontend");

        if (policy is null)
            return;

        var corsResult = _corsService.EvaluatePolicy(context, policy);
        _corsService.ApplyResult(corsResult, context.Response);
    }

    private static async Task WriteExceptionResponseAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = ex switch
        {
            InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Yetkisiz erişim."),
            KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "Sunucuda beklenmeyen bir hata oluştu.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            message,
            statusCode = (int)statusCode
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
    }
}