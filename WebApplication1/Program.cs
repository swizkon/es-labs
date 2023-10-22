using Microsoft.Extensions.Azure;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration
    .AddAzureAppConfiguration(options =>
    {
        options.Connect(Environment.GetEnvironmentVariable("AppConfig"))
            .UseFeatureFlags(c =>
            {
                c.CacheExpirationInterval = TimeSpan.FromMinutes(1);
            });
    });


// builder.Services.AddAzureAppConfiguration();
builder.Services.AddFeatureManagement()
    .AddFeatureFilter<ActivePermissionsFilter>();

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

var fat = app.Services.GetRequiredService<IFeatureManager>();

/*
IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddAzureAppConfiguration(options =>
    {
        options.Connect(Environment.GetEnvironmentVariable("ConnectionString"))
            .UseFeatureFlags();
    }).Build();

IServiceCollection services = new ServiceCollection();

services.AddSingleton<IConfiguration>(configuration).AddFeatureManagement();

await using (var serviceProvider = services.BuildServiceProvider())
{
    var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

    if (await featureManager.IsEnabledAsync("Beta"))
    {
        Console.WriteLine("Welcome to the beta!");
    }
    if (await featureManager.IsEnabledAsync("PermissionEnabled"))
    {
        Console.WriteLine("Welcome to the beta!");
    }
}


*/

//var isAl = fat.IsEnabledAsync("PermissionEnabled", new ParameterMatchContext("CallMe", "AL")).Result;
//var isOther = fat.IsEnabledAsync("PermissionEnabled", new ParameterMatchContext("CallMedfgdgfg", "Other")).Result;

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();