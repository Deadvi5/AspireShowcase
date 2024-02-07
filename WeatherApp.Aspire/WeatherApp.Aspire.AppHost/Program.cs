using Microsoft.Extensions.Hosting;
using WeatherApp.Aspire.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var grafana = builder.AddGrafanaContainer("grafana-http");

var outputCache = builder.AddRedisStackContainer("outputCache");

var mongo = builder.AddMongoDBContainer("mongo")
    .WithMongoExpress()
    .AddDatabase("weather");

var api = builder.AddProject<Projects.WeatherApp_Api>("weatherapi")
    .WithReference(mongo)
    .WithReference(outputCache)
    .WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("grafana-http"))
    .WithLaunchProfile("https");

builder.AddProject<Projects.WeatherApp_Web>("frontend")
    .WithReference(api)
    .WithReference(outputCache)
    .WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("grafana-http"))
    .WithReplicas(builder.Environment.IsDevelopment() ? 1 : 2)
    .WithLaunchProfile("https");

builder.Build().Run();
