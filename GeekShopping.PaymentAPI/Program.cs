using GeekShopping.PaymentAPI.MessageConsumer;
using GeekShopping.PaymentAPI.RabbitMQSender;
using GeekShopping.PaymentProcessor;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSingleton<IProcessPayment, ProcessPayment>();
builder.Services.AddSingleton<IRabbitMQMessageSender, RabbitMQMessageSender>();
builder.Services.AddHostedService<RabbitMQPaymentConsumer>();

builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options => {
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
    configs.SwaggerDoc("v1", new() { Title = "GeekShopping.PaymentAPI", Version = "v1" });
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

app.UseAuthorization();

app.UseAuthentication();

app.MapControllers();

app.Run();