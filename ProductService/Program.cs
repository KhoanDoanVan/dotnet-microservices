using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProductService.Data;
using ProductService.Services;
using Microsoft.Extensions.Options;



var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? Environment.GetEnvironmentVariable("CONNECTION_STRING");


builder.Services.AddDbContext<ProductDbContext>(options => {
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});


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

// Custom Services
builder.Services.AddScoped<IProductService, ProductService.Services.ProductService>();


// CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});


var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    dbContext.Database.Migrate();
}


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