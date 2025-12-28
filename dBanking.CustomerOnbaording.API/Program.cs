using dBanking.Infrastructure;
using dBanking.Core;
using dBanking.CustomerOnbaording.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructureServices();
builder.Services.AddCoreServices();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

app.UseExceptionHandellingMW();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
