using System.Reflection;
using Serilog;
using Serilog.Sinks.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
configureLogging();
builder.Host.UseSerilog();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


void configureLogging()
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var configuring = new ConfigurationBuilder()
    .AddJsonFile("appsetting.json", optional: false, reloadOnChange:true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true).Build();

    Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Debug()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(ConfigureElasticSink(configuring, environment))
    .Enrich.WithProperty("Environment", environment)
    .ReadFrom.Configuration(configuring)
    .CreateLogger();

}

ElasticsearchSinkOptions ConfigureElasticSink(IConfiguration configuration, string environment) {
    return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration"])){
        AutoRegisterTemplate = true,
        IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
        NumberOfReplicas = 1,
        NumberOfShards = 2,
    };
}