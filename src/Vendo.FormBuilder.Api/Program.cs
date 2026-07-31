using Vendo.FormBuilder.Api.Extensions;
using Vendo.FormBuilder.Api.Middleware;
using Vendo.FormBuilder.Application;
using Vendo.FormBuilder.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Vendo.FormBuilder.Api")
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/vendo-formbuilder-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14));

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddControllers();
    builder.Services.AddProblemDetails();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "Vendo-FormBuilder API",
            Version = "v1",
            Description = "Vendo-FormBuilder microservice for creating, versioning, publishing, and collecting responses for dynamic forms."
        });

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DefaultCors", policy =>
            policy.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin());
    });

    var app = builder.Build();
    app.UseHttpsRedirection();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();

   //if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vendo-FormBuilder API v1");
            options.RoutePrefix = string.Empty;
        });
    }

    app.UseCors("DefaultCors");
    app.UseHttpsRedirection();
    app.MapControllers();

    var migrateOnStartup = app.Configuration.GetValue("Database:MigrateOnStartup", false);
    if (migrateOnStartup)
    {
        await app.ApplyMigrationsAsync();
    }

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;
