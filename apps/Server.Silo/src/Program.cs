using Infra.Consts;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults (includes service discovery, health checks, etc.)
builder.AddServiceDefaults();
builder.AddKeyedAzureTableServiceClient(Consts.ClusteringServiceClient);
builder.AddKeyedAzureTableServiceClient(Consts.GrainStateServiceClient);

// Set up clustering storage
builder.UseOrleans();
// builder.UseOrleans(siloBuilder =>
// {
//   siloBuilder
//     .UseAzureStorageClustering(options =>
//     {
//       options.TableName = Consts.ClusteringTableName;
//     });
// });

var app = builder.Build();

app.MapDefaultEndpoints();

if(app.Environment.IsDevelopment())
  app.MapOpenApi();

app.MapGet("/", () => "OK");


await app.RunAsync();