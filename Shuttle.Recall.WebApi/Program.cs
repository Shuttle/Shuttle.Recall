using Asp.Versioning;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Scalar.AspNetCore;
using Serilog;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.RestClient;
using Shuttle.Recall.SqlServer.Storage;
using System.Data.Common;

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

        webApplicationBuilder.Configuration
            .AddUserSecrets<Program>(true)
            .AddJsonFile(appsettingsPath);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(webApplicationBuilder.Configuration)
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

        var accessConnectionString = webApplicationBuilder.Configuration.GetConnectionString("Recall") ?? throw new ApplicationException("Missing connection string 'Recall'.");

        webApplicationBuilder.Services
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
            .AddAccessAuthorization(authorizationBuilder =>
            {
                webApplicationBuilder.Configuration.GetSection(AccessAuthorizationOptions.SectionName).Bind(authorizationBuilder.Options);
            })
            .AddAccessClient(clientBuilder =>
            {
                webApplicationBuilder.Configuration.GetSection(AccessClientOptions.SectionName).Bind(clientBuilder.Options);

                clientBuilder.UseBearerAuthenticationProvider(providerBuilder =>
                {
                    providerBuilder.Options.GetBearerAuthenticationContextAsync = async (_, _) =>
                    {
                        var token = (await new DefaultAzureCredential().GetTokenAsync(new(["https://management.azure.com/.default"]), CancellationToken.None)).Token;

                        return new(token);
                    };
                });
            })
            .AddRecall(recallBuilder =>
            {
                webApplicationBuilder.Configuration.GetSection(RecallOptions.SectionName).Bind(recallBuilder.Options);

                recallBuilder
                    .UseSqlServerEventStorage(builder =>
                    {
                        webApplicationBuilder.Configuration.GetSection(SqlServerStorageOptions.SectionName).Bind(builder.Options);

                        builder.Options.ConnectionString = accessConnectionString;
                        builder.Options.Schema = "access";
                        builder.Options.DbConnectionServiceKey = "AccessDbConnection";
                    });

                recallBuilder.SuppressEventProcessorHostedService();
                recallBuilder.SuppressPrimitiveEventSequencerHostedService();
            })
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