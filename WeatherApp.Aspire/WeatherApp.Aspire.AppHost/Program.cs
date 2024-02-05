using Microsoft.Extensions.Hosting;
using WeatherApp.Aspire.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// var outputCache = builder.AddRedisContainer("outputCache");
    // .WithRedisRedInsight();

var outputCache = builder.AddRedisStackContainer("outputCache");

var mongo = builder.AddMongoDBContainer("mongo")
    .WithMongoExpress()
    .AddDatabase("weather");

var api = builder.AddProject<Projects.WeatherApp_Api>("weatherapi")
    .WithReference(mongo)
    .WithReference(outputCache)
    .WithLaunchProfile("https");

builder.AddProject<Projects.WeatherApp_Web>("frontend")
    .WithReference(api)
    .WithReference(outputCache)
    .WithLaunchProfile("https")
    .WithReplicas(builder.Environment.IsDevelopment() ? 1 : 2);

builder.Build().Run();
