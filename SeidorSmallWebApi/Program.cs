using Microsoft.Extensions.Caching.Distributed;
using SEIDOR_DecrypterLibrary;
using SEIDOR_SLayer;
using SeidorSmallWebApi.Controllers;
using SeidorSmallWebApi.Extensions;
using SeidorSmallWebApi.Filters;
using SeidorSmallWebApi.Initialize;
using System.Text.Json;

// Create standard instance web application. Setup configuration automatically from appsettings
var builder = WebApplication.CreateBuilder(args);
var decoder = new SeiDecoder(builder.Configuration.GetSection("PrivateKey"));

// Add dependencies
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITokenManager>(serviceProvider => new JwtManager(
    serviceProvider.GetRequiredService<IDistributedCache>(),
    serviceProvider.GetRequiredService<IHttpContextAccessor>(),
    builder.Configuration.GetSection("Jwt").GetValue<int>("AuthentificationTokenDuration")));

builder.Services.AddSingleton(decoder);
builder.Services.AddSingleton(sp => new SLConnection(
                   builder.Configuration.GetSection("ServiceLayer").GetValue<string>("serviceLayerRoot"),
                   builder.Configuration.GetSection("ServiceLayer").GetValue<string>("companyDB"),
                   builder.Configuration.GetSection("ServiceLayer").GetValue<string>("userName"),
                   builder.Configuration.GetSection("ServiceLayer").GetValue<string>("password"),
                   builder.Configuration.GetSection("ServiceLayer").GetValue<int>("language")));

builder.Services.AddScoped<SEI_CreateTablesSL>();
builder.Services.AddControllers();
builder.Services.AddScoped<SetupController>();

builder.Services.AddTransient<ExceptionMiddleware>();

// Configure Controllers
builder.Services.ConfigureMvcBuilder();

builder.Services.ConfigureIISIntegration();

// Configure authentication for the controller methods.
var jwt = decoder.DecryptString(builder.Configuration.GetSection("Jwt").GetValue<string>("EncryptedUser"));
builder.Services.ConfigureAuthentication(JsonDocument.Parse(jwt).RootElement.GetProperty("AuthenticationKey").GetString());

builder.Services.ConfigureSwagger();

// Configure Serilog logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFile(builder.Configuration.GetSection("Serilog"));

// Build application
var app = builder.Build();

app.ConfigureEnvironment();
app.ConfigureCustomExceptionMiddleware();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();