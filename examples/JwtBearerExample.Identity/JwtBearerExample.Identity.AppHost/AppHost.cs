var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.JwtBearerExample_Identity>("jwtbearerexample-identity");

builder.Build().Run();
