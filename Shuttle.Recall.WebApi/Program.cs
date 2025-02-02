using Asp.Versioning;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Data.Common;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.RestClient;
using Shuttle.Core.Data;
using Shuttle.Recall.Sql.EventProcessing;
using Shuttle.Recall.Sql.Storage;

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

        webApplicationBuilder.Services
            .AddLogging(builder =>
            {
                builder.AddSerilog();
            })
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type => type.FullName);

                options.AddSecurityDefinition("Shuttle.Access", new()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Shuttle.Access",
                    In = ParameterLocation.Header,
                    Description = "Custom authorization header using the Shuttle.Access scheme. Example: 'Shuttle.Access token=GUID'."
                });

                options.AddSecurityRequirement(new()
                {
                    {
                        new()
                        {
                            Reference = new()
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Shuttle.Access"
                            }
                        },
                        []
                    }
                });
            })
            .AddDataAccess(builder =>
            {
                builder.AddConnectionString("Recall", "Microsoft.Data.SqlClient");
                builder.Options.DatabaseContextFactory.DefaultConnectionStringName = "Recall";
            })
            .AddEventStore()
            .AddSqlEventStorage(builder =>
            {
                builder.Options.ConnectionStringName = "Recall";

                builder.UseSqlServer();
            })
            .AddSqlEventProcessing(builder =>
            {
                builder.Options.ConnectionStringName = "Recall";

                builder.UseSqlServer();
            })
            .AddAccessClient(builder =>
            {
                webApplicationBuilder.Configuration.GetSection(AccessClientOptions.SectionName).Bind(builder.Options);
            })
            .AddAccessAuthorization()
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
        app.UseAccessAuthorization();

        app
            .MapEventEndpoints(versionSet)
            .MapEventTypeEndpoints(versionSet)
            .MapServerEndpoints(versionSet);

        app
            .UseSwagger()
            .UseSwaggerUI();

        app.Run();
    }
}
