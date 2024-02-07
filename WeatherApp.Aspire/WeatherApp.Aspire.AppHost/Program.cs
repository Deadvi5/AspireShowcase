using Microsoft.Extensions.Hosting;
using WeatherApp.Aspire.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// var outputCache = builder.AddRedisContainer("outputCache");
    // .WithRedisRedInsight();

var grafana = builder.AddContainer("grafana", "grafana/grafana")
    .WithVolumeMount("../../grafana/config", "/etc/grafana")
    .WithVolumeMount("../../grafana/dashboards", "/var/lib/grafana/dashboards")
    .WithServiceBinding(containerPort: 3000, hostPort: 3000, name: "grafana-http", scheme: "http");

builder.AddContainer("prometheus", "prom/prometheus")
    .WithVolumeMount("../../prometheus", "/etc/prometheus")
    .WithServiceBinding(9090, hostPort: 9090);

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
