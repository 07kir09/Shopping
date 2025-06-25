using Microsoft.EntityFrameworkCore;
using Shopping.Common.Messaging;
using Shopping.Common.Interfaces;
using Shopping.OrdersService.Data;
using Shopping.OrdersService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:80");
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Shopping Orders API", 
        Version = "v1",
        Description = "API для управления заказами в интернет-магазине",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "ВОЯКИН КИРИЛЛ ВЛАДИСЛАВОВИЧ",
            Email = "support@shopping.com"
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IMessagePublisher>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<RabbitMQPublisher>>();
    var hostName = configuration["RabbitMQ:Host"] ?? "rabbitmq";
    return new RabbitMQPublisher(hostName, logger);
});
builder.Services.AddHttpClient("PaymentsService", client =>
{
    client.BaseAddress = new Uri("http://payments-service:80/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<Shopping.OrdersService.Services.IPaymentClient, Shopping.OrdersService.Services.PaymentClient>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddHostedService<OutboxProcessorService>();
builder.Services.Configure<HostOptions>(opts => opts.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);

var app = builder.Build();

    app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shopping Orders API v1");
});
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
