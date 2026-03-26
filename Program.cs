using OrderDispatcher.AuthService.Configuration;
using OrderDispatcher.AuthService.Entities;
using OrderDispatcher.AuthService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Scalar.AspNetCore;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;    
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddSignInManager();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtTokenOptions"));
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));

var jwtSection = builder.Configuration.GetSection("JwtTokenOptions");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IProfileMessagePublisher, RabbitMqProfileMessagePublisher>();
builder.Services.AddSingleton<IConnection>(sp =>
{
    var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
    var factory = new ConnectionFactory
    {
        HostName = options.HostName,
        Port = options.Port,
        UserName = options.UserName,
        Password = options.Password,
        VirtualHost = options.VirtualHost,
        AutomaticRecoveryEnabled = true,
        NetworkRecoveryInterval = TimeSpan.FromSeconds(30)
    };
    return factory.CreateConnection();
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });


var app = builder.Build();

app.UseCors("AllowAll"); 
app.UseAuthorization();
app.MapControllers();

app.MapOpenApi(); 
app.MapScalarApiReference("/docs", o => o.Title = "Auth Service API");

app.Run();
