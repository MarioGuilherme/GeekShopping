using AutoMapper;
using GeekShopping.CartAPI.Config;
using GeekShopping.CartAPI.Model.Context;
using GeekShopping.CartAPI.RabbitMQSender;
using GeekShopping.CartAPI.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string connection = builder.Configuration["MySQLConnection:MySQLConnectionString"];

builder.Services.AddDbContext<MySQLContext>(
    options => options.UseMySql(connection, new MariaDbServerVersion(new Version(10, 4, 27)))
);

IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();

builder.Services.AddSingleton<IRabbitMQMessageSender, RabbitMQMessageSender>();

builder.Services.AddControllers();

builder.Services.AddHttpClient<ICouponRepository, CouponRepository>(s => s.BaseAddress = new(builder.Configuration["ServiceUrls:CouponAPI"]));
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
    configs.SwaggerDoc("v1", new() { Title = "GeekShopping.CartAPI", Version = "v1" });
    configs.EnableAnnotations();
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