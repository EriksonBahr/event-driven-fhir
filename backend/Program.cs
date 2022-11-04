using System.Net;
using backend.client;
using backend.queue;
using Hl7.Fhir.Rest;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("FhirHttpClient").
    ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() {
        AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
    });


builder.Services.AddSingleton(ctx => {
    var connFactory = new ConnectionFactory() { HostName = "localhost" };
    var conn = connFactory.CreateConnection();
    conn.DeclareFhirQueues();
    return conn;
});

// builder.Services.AddTransient<IFhirDataQueue>(ctx => {
//     var conn = ctx.GetRequiredService<IConnection>();
//     var model = conn.CreateModel();
//     return model as IFhirDataQueue;
// });

builder.Services.AddTransient(ctx => {
    // Get a new HttpClient that is using pooled socket handlers behind the scenes
    var httpClient = ctx.GetRequiredService<IHttpClientFactory>()
                        .CreateClient("FhirHttpClient");

    // inject it into your client
    var fc = new FhirClient("https://hapi.fhir.org/baseR4", httpClient, new FhirClientSettings{
        PreferredFormat = ResourceFormat.Json,
        VerifyFhirVersion = false, // avoids calling /metadata on every request
        PreferredParameterHandling = SearchParameterHandling.Lenient
    });

    var conn = ctx.GetRequiredService<IConnection>();

    return new MQFhirClient(fc, conn.CreateModel());
});

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
