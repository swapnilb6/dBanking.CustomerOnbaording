using dBanking.Core;
using dBanking.Core.Mappers;
using dBanking.CustomerOnbaording.API.Middlewares;
using dBanking.Infrastructure;
using dBanking.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer; // Add this using directive
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;

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




// Add controllers
builder.Services.AddControllers();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<CustomerMappingProfile>();
});

builder.Services.AddDbContext<AppDBContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerDB")));
//options.UseNpgsql(builder.Configuration.GetConnectionString("SqlServerDB"));


//builder.Services.AddSwaggerGen();

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
