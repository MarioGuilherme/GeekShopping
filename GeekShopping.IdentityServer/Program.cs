using Duende.IdentityServer.Services;
using GeekShopping.IdentityServer.Configuration;
using GeekShopping.IdentityServer.Initializer;
using GeekShopping.IdentityServer.Model;
using GeekShopping.IdentityServer.Model.Context;
using GeekShopping.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string connection = builder.Configuration["MySQLConnection:MySQLConnectionString"];

builder.Services.AddDbContext<MySQLContext>(
    options => options.UseMySql(connection, new MariaDbServerVersion(new Version(10, 4, 27)))
);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<MySQLContext>()
                .AddDefaultTokenProviders();

IIdentityServerBuilder builderIdentyService = builder.Services.AddIdentityServer(options => {
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
    options.EmitStaticAudienceClaim = true;
}).AddInMemoryIdentityResources(IdentityConfiguration.IdentityResources)
  .AddInMemoryApiScopes(IdentityConfiguration.ApiScopes)
  .AddInMemoryClients(IdentityConfiguration.Clients)
  .AddAspNetIdentity<ApplicationUser>();

builder.Services.AddScoped<IDBInitializer, DbInitializer>();
builder.Services.AddScoped<IProfileService, ProfileService>();

builderIdentyService.AddDeveloperSigningCredential();

// Add services to the container.
builder.Services.AddControllersWithViews();

WebApplication? app = builder.Build();

IDBInitializer initializer = app.Services.CreateScope().ServiceProvider.GetService<IDBInitializer>()!;

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();

app.UseAuthorization();

initializer.Initialize();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();