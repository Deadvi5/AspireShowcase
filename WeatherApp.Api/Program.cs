using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddRedisOutputCache("outputCache");
builder.AddMongoDBClient("weather");

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseOutputCache();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapPost("/weatherforecast", (IMongoClient mongoClient) =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        
        var database = mongoClient.GetDatabase("weather");
        database.CreateCollection("forecast");
        var collection = database.GetCollection<WeatherForecastList>("forecast");
        collection.InsertOne(new WeatherForecastList(forecast, DateTimeOffset.UtcNow));
    })
    .WithName("PostWeatherForecast")
    .WithOpenApi();

app.MapGet("/weatherforecast", async (IMongoClient mongoClient) =>
    {
        var database = mongoClient.GetDatabase("weather");
        var collection = database.GetCollection<WeatherForecastList>("forecast");
        var filter = Builders<WeatherForecastList>.Filter.Empty;
        var sort = Builders<WeatherForecastList>.Sort.Descending("LastModifiedDate");
        var lastDocument = await collection.Find(filter).Sort(sort).Limit(1).FirstOrDefaultAsync();   
        
        return lastDocument.Forecasts;
    })
    .WithName("GetWeatherForecast")
    .CacheOutput(policyBuilder => policyBuilder.Expire(TimeSpan.FromSeconds(15)))
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record WeatherForecastList(IReadOnlyCollection<WeatherForecast> Forecasts, DateTimeOffset LastModifiedDate)
{
    [BsonId] 
    [BsonRepresentation(BsonType.ObjectId)]
    private string Id { get; set; }
}