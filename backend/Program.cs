using ClimateAdvisor.Api.Services;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddMemoryCache();

// HTTP clients with retry policy (Polly)
builder.Services.AddHttpClient<IClimateService, ClimateService>()
    .AddTransientHttpErrorPolicy(p => p.RetryAsync(2));

builder.Services.AddHttpClient<IPriceService, PriceService>()
    .AddTransientHttpErrorPolicy(p => p.RetryAsync(1));

builder.Services.AddHttpClient<ILocationService, LocationService>()
    .AddTransientHttpErrorPolicy(p => p.RetryAsync(1));

builder.Services.AddSingleton<ICountryLocaleService, CountryLocaleService>();

builder.Services.AddHttpClient<IHydroMeteoService, HydroMeteoService>()
    .AddTransientHttpErrorPolicy(p => p.RetryAsync(1));

builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddHttpClient<ICropRecommendationService, CropRecommendationService>()
    .AddTransientHttpErrorPolicy(p => p.RetryAsync(1));

// Email
builder.Services.AddSingleton<IEmailService, EmailService>();

// Gemini LLM
builder.Services.AddHttpClient<IGeminiService, GeminiService>()
    .AddTransientHttpErrorPolicy(p => p.RetryAsync(1));

// Sri Lanka price data (CBSL Daily Price Report)
builder.Services.AddHttpClient<SriLankaPriceService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});

// Exchange rates (Frankfurter API — free, no key needed)
builder.Services.AddHttpClient<ExchangeRateService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

// CORS — allow frontend (Next.js dev server & production)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ── Middleware ──
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.MapControllers();

// ── Health check quick route ──
app.MapGet("/", () => Results.Redirect("/api/recommendations/health"));

app.Run();
