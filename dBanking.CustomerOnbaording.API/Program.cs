using dBanking.Core;
using dBanking.Core.DTOS.Validators;
using dBanking.Core.Mappers;
using dBanking.Core.Messages;
using dBanking.CustomerOnbaording.API.Middlewares;
using dBanking.Infrastructure;
using dBanking.Infrastructure.DbContext;
using FluentValidation.AspNetCore; // Add this using directive
using MassTransit;
using MassTransit.RabbitMqTransport.Topology;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructureServices();
dBanking.Core.dependancyInjection.AddCoreServices(builder.Services);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));


// Register authorization policies that map to scope claims (scp / scope)
builder.Services.AddAuthorization(options =>
{
    // App.read -> requires token to contain "customer:read"
    options.AddPolicy("App.read", policy =>
        policy.RequireAssertion(context =>
            context.User.Claims.Any(c =>
                (c.Type == "scp" || c.Type == "scope" || c.Type == "http://schemas.microsoft.com/identity/claims/scope") &&
                c.Value.Split(' ').Contains("customer:read")
            )
        )
    );

    // App.write -> requires token to contain "customer:write"
    options.AddPolicy("App.write", policy =>
        policy.RequireAssertion(context =>
            context.User.Claims.Any(c =>
                (c.Type == "scp" || c.Type == "scope" || c.Type == "http://schemas.microsoft.com/identity/claims/scope") &&
                c.Value.Split(' ').Contains("customer:write")
            )
        )
    );
});


// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // No EF outbox configured here to avoid adding EF integration package.
    // If you want the EF outbox, add MassTransit.EntityFrameworkCore with matching EF Core versions.

    // No consumers in this service (producer-only); can add in other services
    // x.AddConsumer<SomeConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var mq = builder.Configuration.GetSection("Messaging:RabbitMq");

        cfg.Host(
            mq["Host"],
            mq["VirtualHost"],
            h =>
            {
                h.Username(mq["Username"]);
                h.Password(mq["Password"]);
                if (bool.TryParse(mq["UseTls"], out var useTls) && useTls)
                    h.UseSsl(s => { s.Protocol = System.Security.Authentication.SslProtocols.Tls12; });
            });
        cfg.Message<dBanking.Core.Messages.CustomerCreated>(mt =>
        {
            mt.SetEntityName("event.customer.created");
        });
        cfg.Message<dBanking.Core.Messages.KycStatusChanged>(mt =>
        {
            mt.SetEntityName("event.kyc.status");
        });
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
        // Recommended topology for events (publish/subscribe)


        // Explicit queue bound to topic exchanges with routing key patterns
        cfg.ReceiveEndpoint("customer-events-queue", e =>
        {
            e.PrefetchCount = 16;
            e.ConfigureConsumeTopology = false; // we will bind exchanges manually

            // Bind to CustomerCreated topic exchange
            e.Bind("event.customer.created", x =>
            {
                x.ExchangeType = "topic";
                // Match routing keys like:
                //   customer.created.pune
                //   customer.created.india.mumbai
                //   customer.created.<any>
                x.RoutingKey = "customer.created.#";
                x.Durable = true;
            });

            // Bind to KycStatusChanged topic exchange
            e.Bind("event.kyc.status", x =>
            {
                x.ExchangeType = "topic";
                x.RoutingKey = "kyc.status.#";
                x.Durable = true;
            });

            // Wire consumers to this endpoint
           // e.ConfigureConsumer<CustomerCreatedConsumer>(context);
           // e.ConfigureConsumer<KycStatusChangedConsumer>(context);
        });




        cfg.ConfigureEndpoints(context);


        // NOTE: removed explicit cfg.Publish<T> calls for message types that are not defined in the solution
        // to avoid compilation errors. Add per-message publish configuration when message contracts exist.
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
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresAzureDb")));



builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CMS API", Version = "v1" });
    var tokenUrlString = builder.Configuration.GetValue<string>("AzureAd:TokenUrl");
    if (string.IsNullOrWhiteSpace(tokenUrlString))
    {
        throw new InvalidOperationException("AzureAd.TokenUrl configuration value is missing or empty.");
    }
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Flows = new OpenApiOAuthFlows
        {
            ClientCredentials = new OpenApiOAuthFlow
            {
                // TokenUrl = new Uri(tokenUrlString),
                // Scopes = new Dictionary<string, string>
                //{
                //    {"api://38be1d86-8bdc-4bad-ad6c-c20ca69474f0/.default",".default" }
                //},

                TokenUrl = new Uri("https://localhost:44369/api/auth/token"),
            }
        },

        In = ParameterLocation.Header,
        Name = HeaderNames.Authorization,
        Type = SecuritySchemeType.OAuth2
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {

            {   new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "oauth2"
                    }
                },
                new[] { "App.read", "App.write" }
            }
    });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandellingMW();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
