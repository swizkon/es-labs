using ES.Labs.Api;
using ES.Labs.Api.BackgroundServices;
using ES.Labs.Api.Security;
using ES.Labs.Domain;
using ES.Labs.Domain.Projections;
using EventStore.Client;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;

services.AddEventStoreClient(settings => EventStoreClientSettings
    .Create("esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false"));

services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddSignalR();

services.AddCustomAuthorization();

services.AddCors(options => options.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .WithOrigins(
            "http://localhost:3000",
            "http://localhost:5000",
            "http://localhost:5173",
            "http://localhost:4173",

            "http://localhost:6000",
            "https://localhost:6001",
            
            "http://localhost:7000",
            "https://localhost:7001");
}));

services.AddSingleton<AppVersionInfo>();
services.AddSingleton<ProjectionState>(new ProjectionState
{
    Date = DateTime.UtcNow,
    EqualizerState = new EqualizerState
    {
        DeviceName = EventStoreConfiguration.DeviceStreamName,
        Volume = 33,
        CurrentVersion = null,
        Channels = new List<EqualizerState.EqualizerBandState>
        {
            new()
            {
                Channel = "0",
                Level = "10"
            }
        }
    }
    
});

services.AddHttpClient();

services.AddSingleton<IEventMetadataInfo, AppVersionInfo>();
services.AddScoped<EventDataBuilder>();

services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("REDIS_CONNECTION_STRING");
    options.InstanceName = "ESDemo-";
});

services.AddHostedService<ConsumerHostedService>();

var app = builder.Build();

app.MapGet("/appInfo", ([FromServices] AppVersionInfo appInfo) => Results.Ok(appInfo))
    .RequireAuthorization(policyBuilder =>
    {
        policyBuilder.AddRequirements(new HasPermissionRequirement(Permissions.PlaceOrder));
    })
    .RequireAuthorization(Policies.OnlyEvenSeconds);

app.MapGet("/me", () => Results.Ok(DateTime.Now)).RequireAuthorization(Policies.MustHaveSession);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();

app.UseStaticFiles();

//app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<TestHub>("/hubs/testHub");
});

app.Run();