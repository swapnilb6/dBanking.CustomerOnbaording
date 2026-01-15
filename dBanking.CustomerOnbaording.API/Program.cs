using dBanking.Core.Mappers;
using dBanking.Core.Repository_Contracts;
using dBanking.Core.ServiceContracts;
using dBanking.Core.Services;
using dBanking.CustomerOnbaording.API.Consumers;
using dBanking.CustomerOnbaording.API.Middlewares;
using dBanking.Infrastructure;
using dBanking.Infrastructure.DbContext;
using dBanking.Infrastructure.Repositories;
using FluentValidation.AspNetCore; // Add this using directive
using MassTransit;
using MassTransit.RabbitMqTransport.Topology;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.
builder.Services.AddInfrastructureServices();
dBanking.Core.dependancyInjection.AddCoreServices(builder.Services);
builder.Services.AddScoped<IKycCaseService, KycCaseService>();

//Auditing services registration
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICorrelationAccessor, HttpCorrelationAccessor>();

builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();



// AuthN: JWT Bearer from Entra ID for a protected web API
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));


// AuthZ: policies based on delegated scopes
builder.Services.AddAuthorization(options =>
{
    // The 'scp' claim contains short scope names like "App.read", "App.write"
    options.AddPolicy("App.Read", policy => policy.RequireScope("App.read"));
    options.AddPolicy("App.Write", policy => policy.RequireScope("App.write"));
});


// Swagger/OpenAPI + OAuth2 (Authorization Code + PKCE)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "dBanking.CMS API", Version = "v1" });

    var tenantId = builder.Configuration["AzureAd:TenantId"];
    var instance = builder.Configuration["AzureAd:Instance"]; // https://login.microsoftonline.com/
    var authUrl = $"{instance}{tenantId}/oauth2/v2.0/authorize";
    var tokenUrl = $"{instance}{tenantId}/oauth2/v2.0/token";


    var apiClientId = builder.Configuration["AzureAd:ClientId"];
    var scopes = new Dictionary<string, string>
    {
        [$"api://{apiClientId}/App.read"] = "Read access to dBanking.CMS",
        [$"api://{apiClientId}/App.write"] = "Write access to dBanking.CMS"
    };
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(authUrl),
                TokenUrl = new Uri(tokenUrl),
                Scopes = new Dictionary<string, string>
                {
                    [$"api://{builder.Configuration["AzureAd:ClientId"]}/App.read"] = "Read access",
                    [$"api://{builder.Configuration["AzureAd:ClientId"]}/App.write"] = "Write access"
                }
            }
        },
        Description = "OAuth2 Authorization Code Flow with PKCE (Azure AD / Entra ID)"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
        },
        new List<string>
        {
            $"api://{builder.Configuration["AzureAd:ClientId"]}/App.read",
            $"api://{builder.Configuration["AzureAd:ClientId"]}/App.write"
        }
    }
});
});

// The following flag can be used to get more descriptive errors in development environments
IdentityModelEventSource.ShowPII = false;


// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<KycStatusChangedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var mq = builder.Configuration.GetSection("Messaging:RabbitMq");

        cfg.Host(mq["Host"], mq["VirtualHost"], h =>
        {
            h.Username(mq["Username"]);
            h.Password(mq["Password"]);

            if (bool.TryParse(mq["UseTls"], out var useTls) && useTls)
            {
                h.UseSsl(s => { s.Protocol = System.Security.Authentication.SslProtocols.Tls12; });
            }
        });

        // Set the exchange names (entity names)
        cfg.Message<dBanking.Core.Messages.CustomerCreated>(mt =>
        {
            mt.SetEntityName("event.customer.created");   // topic exchange
        });

        cfg.Message<dBanking.Core.Messages.KycStatusChanged>(mt =>
        {
            mt.SetEntityName("event.kyc.status");         // topic exchange
        });

        // Publish topology
        cfg.Publish<dBanking.Core.Messages.CustomerCreated>(p =>
        {
            p.ExchangeType = "topic";
            p.Durable = true;
        });

        cfg.Publish<dBanking.Core.Messages.KycStatusChanged>(p =>
        {
            p.ExchangeType = "topic";
            p.Durable = true;
        });

        // Explicit consumer endpoint (distinct queue name)
        cfg.ReceiveEndpoint("kyc-status-changed-processor", e =>
        {
            // Let MassTransit bind the consumer to the message exchange using consume topology
            // (default is true; no need to set e.ConfigureConsumeTopology)
            e.PrefetchCount = 16;

            e.ConfigureConsumer<KycStatusChangedConsumer>(context);

            e.UseMessageRetry(r => r.Exponential(
                retryLimit: 5,
                minInterval: TimeSpan.FromSeconds(1),
                maxInterval: TimeSpan.FromSeconds(30),
                intervalDelta: TimeSpan.FromSeconds(5)));

            // Optional throughput cap:
            // e.UseConcurrencyLimit(8);
        });

        // REMOVE this 'customer-events-queue' until you attach consumers to it.
        // Otherwise it's a dead queue accumulating messages.
        // If/when you need a fan-in queue for multiple topics with manual routing keys:
        //
        // cfg.ReceiveEndpoint("customer-events-queue", e =>
        // {
        //     e.PrefetchCount = 16;
        //     e.ConfigureConsumeTopology = false; // manual binding
        //
        //     e.Bind("event.customer.created", x =>
        //     {
        //         x.ExchangeType = "topic";
        //         x.RoutingKey = "customer.created.#";
        //         x.Durable = true;
        //     });
        //
        //     e.Bind("event.kyc.status", x =>
        //     {
        //         x.ExchangeType = "topic";
        //         x.RoutingKey = "kyc.status.#";
        //         x.Durable = true;
        //     });
        //
        //     // Then wire consumers:
        //     // e.ConfigureConsumer<CustomerCreatedConsumer>(context);
        //     // e.ConfigureConsumer<KycStatusChangedConsumer>(context);
        // });

        // IMPORTANT: Do NOT call ConfigureEndpoints when using explicit endpoint declarations
        // cfg.ConfigureEndpoints(context);
    });
});


builder.Services.AddOptions<MassTransitHostOptions>().Configure(options =>
{
    options.WaitUntilStarted = true;
    options.StartTimeout = TimeSpan.FromSeconds(30);
});


// Add controllers
builder.Services.AddControllers();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<CustomerMappingProfile>();
});

builder.Services.AddFluentValidationAutoValidation();

// SQL Server registration (kept as commented reference)
// builder.Services.AddDbContext<AppDBContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerDB")));

// PostgreSQL registration using AppPostgresDbContext
// Make sure you have a connection string named "PostgresDB" in configuration
builder.Services.AddDbContext<AppPostgresDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("LocalPostgresDB")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{

    // Swagger UI + OAuth2 client settings
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "dBanking.CMS API v1");
        c.OAuthClientId(builder.Configuration["Swagger:ClientId"]);        // Swagger app's client ID
        c.OAuthScopes(builder.Configuration.GetSection("Swagger:Scopes").Get<string[]>() ?? Array.Empty<string>());
        c.OAuthUsePkce();                                                  // <-- REQUIRED
        c.OAuth2RedirectUrl(builder.Configuration["Swagger:RedirectUri"]); // e.g. https://localhost:5001/swagger/oauth2-redirect.html
    });
}

app.UseExceptionHandellingMW();
app.UseMiddleware<CorrelationIdMiddleware>();


app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();


public partial class Program { }