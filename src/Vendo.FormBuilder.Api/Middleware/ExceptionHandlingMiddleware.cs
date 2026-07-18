using System.Text.Json;
using Vendo.FormBuilder.Application.Common.Exceptions;
using Vendo.FormBuilder.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Vendo.FormBuilder.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problem = exception switch
        {
            ApplicationValidationException validationException => CreateProblem(
                context,
                StatusCodes.Status400BadRequest,
                "Validation Failed",
                "One or more validation errors occurred.",
                validationException.Errors),
            NotFoundException notFoundException => CreateProblem(
                context,
                StatusCodes.Status404NotFound,
                "Not Found",
                notFoundException.Message),
            ConcurrencyException concurrencyException => CreateProblem(
                context,
                StatusCodes.Status409Conflict,
                "Concurrency Conflict",
                concurrencyException.Message),
            ConflictException conflictException => CreateProblem(
                context,
                StatusCodes.Status409Conflict,
                "Conflict",
                conflictException.Message),
            DomainException domainException => CreateProblem(
                context,
                StatusCodes.Status400BadRequest,
                "Domain Rule Violation",
                domainException.Message),
            ArgumentException argumentException => CreateProblem(
                context,
                StatusCodes.Status400BadRequest,
                "Bad Request",
                argumentException.Message),
            _ => CreateProblem(
                context,
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                _environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred.")
        };

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        var payload = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(payload);
    }

    private static ProblemDetails CreateProblem(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        IDictionary<string, string[]>? errors = null)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        return problem;
    }
}
