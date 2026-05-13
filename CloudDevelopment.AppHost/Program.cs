using Amazon;
using Aspire.Hosting.LocalStack.Container;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("course-cache")
    .WithRedisInsight(containerName: "course-insight");
var gateway = builder.AddProject<Projects.Api_Gateway>("api-gateway");

var awsConfig = builder.AddAWSSDKConfig()
    .WithProfile("default")
    .WithRegion(RegionEndpoint.EUCentral1);

var localstack = builder
    .AddLocalStack("study-localstack", awsConfig: awsConfig, configureContainer: container =>
    {
        container.Lifetime = ContainerLifetime.Session;
        container.DebugLevel = 1;
        container.LogLevel = LocalStackLogLevel.Debug;
        container.Port = 4566;
        container.AdditionalEnvironmentVariables
            .Add("DEBUG", "1");
        container.AdditionalEnvironmentVariables
            .Add("SNS_CERT_URL_HOST", "sns.eu-central-1.amazonaws.com");
    });

var awsResources = builder
    .AddAWSCloudFormationTemplate("resources", "CloudFormation/study-template-sns.yaml", "study")
    .WithReference(awsConfig)
    .WaitFor(localstack!);

for (var i = 0; i < 5; i++)
{
    var service = builder.AddProject<Projects.Service_Api>($"service-api-{i}", launchProfileName: null)
    .WithReference(cache, "RedisCache")
    .WaitFor(cache)
    .WithReference(awsResources)
    .WithEnvironment("Settings_MessageBroker", "SNS")
    .WaitFor(awsResources)
    .WithHttpsEndpoint(port: 5666 + i);
    gateway.WaitFor(service).WithReference(service);
}

builder.AddProject<Projects.Client_Wasm>("client")
    .WaitFor(gateway);

var minio = builder.AddMinioContainer("study-minio");

var sink = builder.AddProject<Projects.FileStorage>("service-filestorage")
    .WithReference(awsResources)
    .WithEnvironment("Settings__MessageBroker", "SNS")
    .WithEnvironment("AWS__Resources__SNSUrl", "http://host.docker.internal:5241/api/sns")
    .WithEnvironment("AWS__Resources__MinioBucketName", "study-bucket")
    .WithReference(minio)
    .WaitFor(minio)
    .WaitFor(awsResources);

builder.UseLocalStack(localstack);

builder.Build().Run();
