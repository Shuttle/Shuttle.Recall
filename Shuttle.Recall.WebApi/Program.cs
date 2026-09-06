using System.Data.Common;
using Asp.Versioning;
using Microsoft.Data.SqlClient;
using Scalar.AspNetCore;
using Serilog;
using Shuttle.Access.AspNetCore;
using Shuttle.Recall.SqlServer.EventProcessing;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Recall.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);

        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        var configurationFolder = Environment.GetEnvironmentVariable("CONFIGURATION_FOLDER");

        if (string.IsNullOrEmpty(configurationFolder))
        {
            throw new ApplicationException("Environment variable `CONFIGURATION_FOLDER` has not been set.");
        }

        var appsettingsPath = Path.Combine(configurationFolder, "appsettings.json");

        if (!File.Exists(appsettingsPath))
        {
            throw new ApplicationException($"File '{appsettingsPath}' cannot be accessed/found.");
        }

        var webApplicationBuilder = WebApplication.CreateBuilder(args);
        var configuration = webApplicationBuilder.Configuration;

        configuration
            .AddUserSecrets<Program>(true)
            .AddJsonFile(appsettingsPath);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        var apiVersion1 = new ApiVersion(1, 0);

        webApplicationBuilder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = apiVersion1;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        webApplicationBuilder.Services
            .AddHttpContextAccessor()
            .Configure<ApiOptions>(configuration.GetSection(ApiOptions.SectionName))
            .AddScoped<IEventStoreContext, EventStoreContext>()
            .AddScoped<IPrimitiveEventQuery, PrimitiveEventQuery>()
            .AddLogging(builder =>
            {
                builder.AddSerilog();
            })
            .AddEndpointsApiExplorer()
            .AddOpenApi(options =>
            {
                options.AddSchemaTransformer((schema, _, _) =>
                {
                    schema.Title = schema.Title?.Replace("+", "_");
                    return Task.CompletedTask;
                });
            })
            .AddAccessAuthorization(options =>
            {
                configuration.GetSection(AccessAuthorizationOptions.SectionName).Bind(options);
            })
            .Services
            .AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

        webApplicationBuilder.Services
            .AddRecall(options =>
            {
                configuration.GetSection(RecallOptions.SectionName).Bind(options);
            })
            .UseSqlServerEventStorage(options =>
            {
                configuration.GetSection(SqlServerStorageOptions.SectionName).Bind(options);

                if (string.IsNullOrWhiteSpace(options.ConnectionString))
                {
                    // No connection string is configured statically: the event store is selected per
                    // request from the `Shuttle-Recall-Event-Store` header (see EventStoreContext) and
                    // applied via SqlServerStorageDbContext.SetConnectionString(). This placeholder only
                    // satisfies SqlServerStorageOptionsValidator at startup and is never actually used.
                    options.ConnectionString = "Server=.";
                    options.ConfigureDatabase = false;
                }
            })
            .UseSqlServerEventProcessing();

        var app = webApplicationBuilder.Build();

        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(apiVersion1)
            .ReportApiVersions()
            .Build();

        app.UseCors("AllowAll");

        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("Shuttle Recall API")
                .WithTheme(ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        app.UseAccessAuthorization();

        app
            .MapEventEndpoints(versionSet)
            .MapEventTypeEndpoints(versionSet)
            .MapServerEndpoints(versionSet);

        app.Run();
    }
}