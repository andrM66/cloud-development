var builder = DistributedApplication.CreateBuilder(args);

var service = builder.AddProject<Projects.Service_Api>("service-api");

builder.AddProject<Projects.Client_Wasm>("client")
    .WaitFor(service);

builder.Build().Run();
