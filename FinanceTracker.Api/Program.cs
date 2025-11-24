using Microsoft.OpenApi.Models;
using System.IO;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using MongoDB.Driver;
using FinanceTracker.Api.Services;
using FinanceTracker.Api.Interfaces;



var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// Redis Cache
// ----------------------------
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "FinanceTracker_";
});

// ----------------------------
// Controllers
// ----------------------------
builder.Services.AddControllers();

// ----------------------------
// Swagger / OpenAPI
// ----------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// ----------------------------
// MongoDB Client
// ----------------------------
var mongoConnection = Environment.GetEnvironmentVariable("MONGOCONNECTION") 
    ?? builder.Configuration.GetConnectionString("MongoConnection");

builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnection));

// ----------------------------
// Register UserService Layer
// ----------------------------
builder.Services.AddScoped<UserService>();

// Subscription Service
builder.Services.AddScoped<SubscriptionService>();


// Transaction Service (interface required by SubscriptionService)
// TODO: Replace `FakeTransactionService` with  real implementation. this is placeholder.
builder.Services.AddScoped<ITransactionService, FakeTransactionService>();

// Balance Service
builder.Services.AddScoped<BalanceService>();


var app = builder.Build();

// ----------------------------
// Swagger UI Setup
// ----------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "api/{documentName}/swagger.json";
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api/v1/swagger.json", "Finance Tracker API V1");
        c.RoutePrefix = "api/docs";
    });
}

// ----------------------------
// Middleware + Routing
// ----------------------------
app.UseAuthorization();

app.Urls.Add("http://0.0.0.0:8080");

app.MapGet("/", () => "Finance Tracker API is running!");

app.MapControllers();

app.Run();


// ------------------------------------------------------------------
// TEMPORARY SERVICE: Replace once real TransactionService is created
// ------------------------------------------------------------------
public class FakeTransactionService : ITransactionService
{
    public Task<bool> RecordTransactionAsync(
        MongoDB.Bson.ObjectId userId,
        decimal amount,
        string currency,
        DateTime whenUtc,
        string description)
    {
        // Log to console so you know this is being triggered
        Console.WriteLine($"[FAKE TXN] User {userId} - {amount} {currency} @ {whenUtc} | {description}");
        return Task.FromResult(true);
    }
}