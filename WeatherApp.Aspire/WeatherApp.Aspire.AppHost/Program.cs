using WeatherApp.Aspire.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var outputCache = builder.AddRedisContainer("outputCache")
    .WithRedisRedInsight();

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
    .WithLaunchProfile("https");

builder.Build().Run();
