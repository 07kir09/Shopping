using Microsoft.EntityFrameworkCore;
using Shopping.Common.Messaging;
using Shopping.Common.Interfaces;
using Shopping.OrdersService.Data;
using Shopping.OrdersService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Добавляем CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure database
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрируем RabbitMQ
builder.Services.AddSingleton<IMessagePublisher>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<RabbitMQPublisher>>();
    var hostName = configuration["RabbitMQ:Host"] ?? "rabbitmq";
    return new RabbitMQPublisher(hostName, logger);
});

// Register services
builder.Services.AddScoped<IOrderService, OrderService>();

// Configure background service for processing outbox messages
builder.Services.AddHostedService<OutboxProcessorService>();

// Configure background service behavior
builder.Services.Configure<HostOptions>(opts => opts.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Добавляем использование CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
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
