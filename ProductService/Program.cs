using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProductService.Data;
using ProductService.Services;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Shared.Services;
using Shared.Events;



var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? Environment.GetEnvironmentVariable("CONNECTION_STRING");


builder.Services.AddDbContext<ProductDbContext>(options => {
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// Redis
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? Environment.GetEnvironmentVariable("DefaultConnection") ?? "redis:6379";

var redis = ConnectionMultiplexer.Connect(redisConnection);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
builder.Services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();


// RabbitMQ
var rabbitMqConnection = builder.Configuration.GetConnectionString("RabbitMQ") ?? Environment.GetEnvironmentVariable("RABBITMQ_CONNECTION") ?? "amqp://admin:admin123@rabbitmq:5672";

builder.Services.AddSingleton<IMessageBusService>(sp => new RabbitMQMessageBusService(rabbitMqConnection));


// Elasticsearch
var elasticsearchUrl = builder.Configuration.GetConnectionString("Elasticsearch") ?? Environment.GetEnvironmentVariable("ELASTICSEARCH_URL") ?? "http://elasticsearch:9200";

builder.Services.AddSingleton<IElasticsearchService>(sp => new ElasticsearchService(elasticsearchUrl));



// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? Environment.GetEnvironmentVariable("JWT_SECRET");
var key = Encoding.ASCII.GetBytes(secretKey!);



builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero
    };
});


builder.Services.AddAuthorization();

// Dependency Injection - Tạo một lần cho mỗi HTTP request o Controllers
builder.Services.AddScoped<IProductService, ProductService.Services.ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();


// CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});


// Health Checks



var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    dbContext.Database.Migrate();


    // Index existing products to Elasticsearch
    var elasticsearchService = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();
    var products = await dbContext.Products.ToListAsync();
    if (products.Any())
    {
        await elasticsearchService.BulkIndexProductsAsync(products);
    }
}



// Subscribe to RabbitMQ Events
var messageBus = app.Services.GetRequiredService<IMessageBusService>();
await messageBus.SubscribeAsync<InventoryUpdatedEvent>(
    queue: "product_inventory_updates",
    exchange: "inventory",
    routingKey: "inventory.updated",
    handler: async (evt) =>
    {
        Console.WriteLine($"Inventory updated: Product {evt.ProductId}, New Quantity: {evt.NewQuantity}");
    }
);


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();