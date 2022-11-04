using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using frontend.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton(ctx => {
    var connFactory = new ConnectionFactory() { HostName = "localhost" };
    return connFactory.CreateConnection();
});

builder.Services.AddScoped(ctx => {
    var conn = ctx.GetRequiredService<IConnection>();
    var model = conn.CreateModel();
    var queueName = model.QueueDeclare().QueueName;
    model.QueueBind(queue: queueName,
        exchange: "device",
        routingKey: "");
    var consumer = new EventingBasicConsumer(model);
    model.BasicConsume(
        queue: queueName,
        autoAck: true,
        consumer: consumer
    );
    
    return consumer;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
