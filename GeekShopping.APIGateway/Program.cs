using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options => {
    options.Authority = "https://localhost:4435/";
    options.TokenValidationParameters = new() { ValidateAudience = false };
});

builder.Services.AddOcelot();

builder.Services.AddControllers();

var app = builder.Build();

app.UseOcelot();

app.Run();