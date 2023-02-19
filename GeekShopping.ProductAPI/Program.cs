using AutoMapper;
using GeekShopping.ProductAPI.Config;
using GeekShopping.ProductAPI.Model.Context;
using GeekShopping.ProductAPI.Repository;
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

builder.Services.AddScoped<IProductRepository, ProductRepository>();

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
    configs.SwaggerDoc("v1", new() { Title = "GeekShopping.ProductAPI", Version = "v1" });
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

WebApplication? app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GeekShopping.ProductAPI v1"));
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints => {
    endpoints.MapControllers();
});

app.MapControllers();

app.Run();
