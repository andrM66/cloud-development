using CloudDevelopment.ServiceDefaults;
using Service.Api.Generator;
using Amazon.SimpleNotificationService;
using Service.Api.Messaging;
using LocalStack.Client.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisDistributedCache("RedisCache");


builder.Services.AddLocalStack(builder.Configuration);
builder.Services.AddScoped<IProducerService, SnsPublisherService>();
builder.Services.AddAwsService<IAmazonSimpleNotificationService>();

builder.Services.AddScoped<IGeneratorService, GeneratorService>();
var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGet("/api/study-course", (IGeneratorService service, int id) => service.ProcessCourse(id));
app.Run();
