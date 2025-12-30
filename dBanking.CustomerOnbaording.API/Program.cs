using dBanking.Core;
using dBanking.Core.Mappers;
using dBanking.CustomerOnbaording.API.Middlewares;
using dBanking.Infrastructure;
using dBanking.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructureServices();
builder.Services.AddCoreServices();

// Add controllers
builder.Services.AddControllers();
    
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<CustomerMappingProfile>();
});


builder.Services.AddDbContext<AppDBContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PrimaryDb"));
});

builder.Services.AddSwaggerGen();

var app = builder.Build();

if(app.Environment.IsDevelopment())
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
