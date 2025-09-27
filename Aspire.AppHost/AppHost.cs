using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("catalog-db")
                      .WithDataVolume();

var catalogDatabase = postgres.AddDatabase("catalog");

builder.AddProject<Projects.AsyncScapeIA_WebApp>("webapp")
       .WithReference(catalogDatabase);

builder.AddProject<Projects.AsyncScapeIA_Workers>("worker")
       .WithReference(catalogDatabase);

builder.Build().Run();