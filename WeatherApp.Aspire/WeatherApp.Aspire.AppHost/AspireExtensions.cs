using System.Globalization;

namespace WeatherApp.Aspire.AppHost;

public static class AspireExtensions
{
    public static IResourceBuilder<MongoDBContainerResource> WithMongoExpress(this IResourceBuilder<MongoDBContainerResource> builder)
    {
        builder.ApplicationBuilder.AddContainer("mongo-express", "mongo-express", "latest")
            .WithServiceBinding(8081, 8081, scheme: "http")
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
            .WithServiceBinding(8001, 8001, scheme: "http")
            .WithAnnotation(ManifestPublishingCallbackAnnotation.Ignore)
            .WithVolumeMount("redinsightvolume", "/var/volume/", VolumeMountType.Named);
        
        return builder;
    }
}