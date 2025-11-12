using Microsoft.OpenApi.Models;
using System.IO;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "FinanceTracker_";
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var mongoConnection = Environment.GetEnvironmentVariable("MONGOCONNECTION") ?? builder.Configuration.GetConnectionString("MongoConnection");

builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnection));

var app = builder.Build();


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

//app.UseHttpsRedirection();
app.UseAuthorization();
app.Urls.Add("http://0.0.0.0:8080");


app.MapGet("/", () => "Finance Tracker API is running!");

app.MapControllers(); 

app.Run();