using GeekShopping.OrderAPI.MessageConsumer;
using GeekShopping.OrderAPI.Model.Context;
using GeekShopping.OrderAPI.RabbitMQSender;
using GeekShopping.OrderAPI.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string connection = builder.Configuration["MySQLConnection:MySQLConnectionString"];

builder.Services.AddDbContext<MySQLContext>(
    options => options.UseMySql(connection, new MariaDbServerVersion(new Version(10, 4, 27)))
);

DbContextOptionsBuilder<MySQLContext> dbContextOptionsBuilder = new();
dbContextOptionsBuilder.UseMySql(connection, new MariaDbServerVersion(new Version(10, 4, 27)));

builder.Services.AddSingleton(new OrderRepository(dbContextOptionsBuilder.Options));

builder.Services.AddHostedService<RabbitMQCheckoutConsumer>();
builder.Services.AddHostedService<RabbitMQPaymentConsumer>();
builder.Services.AddSingleton<IRabbitMQMessageSender, RabbitMQMessageSender>();

builder.Services.AddControllers();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options => {
        options.Authority = "https://localhost:4435/";
        options.TokenValidationParameters = new() { ValidateAudience = false };
    });

builder.Services.AddAuthorization(options => {
    options.AddPolicy("ApiScope", policy => {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "geek_shopping");
    });
});

builder.Services.AddSwaggerGen(configs => {
    configs.SwaggerDoc("v1", new() { Title = "GeekShopping.OrderAPI", Version = "v1" });
    configs.AddSecurityDefinition("Bearer", new() {
        Description = "Enter \"Bearer\" [space] and your token!",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    configs.AddSecurityRequirement(new() {{
        new() {
            Reference = new() {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            },
            Scheme = "oauth2",
            Name = "Bearer",
            In = ParameterLocation.Header
        },
        new List<string>()
    }});
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();