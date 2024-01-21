using RetailRhythmRadar.BackgroundServices;
using RetailRhythmRadar.Configuration;
using RetailRhythmRadar.Hubs;

namespace RetailRhythmRadar;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var services = builder.Services;

        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen();

        services.AddHttpClient();

        services.RegisterServices(builder.Configuration);
        
        services.AddHostedService<ConsumerHostedService>();

        services.AddSignalR();

        services.AddCors(options => options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyMethod()
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

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app
            .MapCommandEndPoints()
            .MapEventEndPoints()
            .MapQueryEndPoints();

        app.UseCors("AllowAll");

        // Skip this for now. Should be determined by config and/or some RUNNING IN CONTAINER env var
        //app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseWebSockets();

        app.UseStaticFiles();

        //app.MapControllers();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHub<MessageExchangeHub>("/hubs/messageExchange");
        });

        app.Run();
    }
}