using System.Globalization;
using System.Net.Sockets;

namespace WeatherApp.Aspire.AppHost;

public static class AspireExtensions
{
    public static IResourceBuilder<MongoDBContainerResource> WithMongoExpress(this IResourceBuilder<MongoDBContainerResource> builder)
    {
        builder.ApplicationBuilder.AddContainer("mongo-express", "mongo-express", "latest")
            .WithEndpoint(8081, 8081, scheme: "http")
            .WithAnnotation(ManifestPublishingCallbackAnnotation.Ignore)
            .WithEnvironment(context =>
            {
                if (builder.Resource.GetConnectionString() is not { } connectionString)
                {
                    throw new DistributedApplicationException($"MongoDBContainer resource '{builder.Resource.Name}' did not return a connection string.");
                }
                var connectionStringUri = new Uri(connectionString);
                context.EnvironmentVariables.Add("ME_CONFIG_MONGODB_SERVER", "host.docker.internal");
                context.EnvironmentVariables.Add("ME_CONFIG_MONGODB_PORT", connectionStringUri.Port.ToString(CultureInfo.InvariantCulture));
                context.EnvironmentVariables.Add("ME_CONFIG_MONGODB_URL", $"{connectionStringUri.Scheme}://host.docker.internal:{connectionStringUri.Port}");
            });

        return builder;
    }
    
    public static IResourceBuilder<RedisContainerResource> WithRedisRedInsight(this IResourceBuilder<RedisContainerResource> builder)
    {   
        builder.ApplicationBuilder.AddContainer("red-insight", "redislabs/redisinsight", "latest")
            .WithEndpoint(8001, 8001, scheme: "http")
            .WithAnnotation(ManifestPublishingCallbackAnnotation.Ignore)
            .WithVolumeMount("redinsightvolume", "/var/volume/", VolumeMountType.Named);
        
        return builder;
    }

    public static IResourceBuilder<RedisStackContainerResource> AddRedisStackContainer(this IDistributedApplicationBuilder builder, string name, int? port = null)
    {
        var redis = new RedisStackContainerResource(name);
        
        return builder.AddResource(redis)
            .WithAnnotation(ManifestPublishingCallbackAnnotation.Ignore)
            .WithAnnotation(new EndpointAnnotation(ProtocolType.Tcp, port: port, containerPort: 6379))
            .WithEndpoint(8001, 8001, scheme: "http")
            .WithAnnotation(new ContainerImageAnnotation { Image = "redis/redis-stack", Tag = "latest" });        
    }

    public static IResourceBuilder<ContainerResource> AddGrafanaContainer(this IDistributedApplicationBuilder builder, string grafanaHttp)
    {
        var grafana = builder.AddContainer("grafana", "grafana/grafana")
            .WithVolumeMount("../../grafana/config", "/etc/grafana")
            .WithVolumeMount("../../grafana/dashboards", "/var/lib/grafana/dashboards")
            .WithEndpoint(containerPort: 3000, hostPort: 3000, name: grafanaHttp, scheme: "http");

        builder.AddContainer("prometheus", "prom/prometheus")
            .WithVolumeMount("../../prometheus", "/etc/prometheus")
            .WithEndpoint(9090, hostPort: 9090);

        return grafana;
    }
}