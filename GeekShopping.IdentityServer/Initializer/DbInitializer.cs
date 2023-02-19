using GeekShopping.IdentityServer.Configuration;
using GeekShopping.IdentityServer.Model;
using GeekShopping.IdentityServer.Model.Context;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GeekShopping.IdentityServer.Initializer;

public class DbInitializer : IDBInitializer {
    private readonly MySQLContext _context;
    private readonly UserManager<ApplicationUser> _user;
    private readonly RoleManager<IdentityRole> _role;

    public DbInitializer(
        MySQLContext context,
        UserManager<ApplicationUser> user,
        RoleManager<IdentityRole> role
    ) {
       this._context = context;
       this._user = user;
       this._role = role;
    }

    public void Initialize() {
        if (this._role.FindByNameAsync(IdentityConfiguration.Admin).Result != null)
            return;

        this._role.CreateAsync(new IdentityRole(IdentityConfiguration.Admin)).GetAwaiter().GetResult();
        this._role.CreateAsync(new IdentityRole(IdentityConfiguration.Client)).GetAwaiter().GetResult();

        ApplicationUser admin = new() {
            UserName = "mario-admin",
            Email = "mario-admin@gmail.com",
            EmailConfirmed = true,
            PhoneNumber = "+55 (14) 123456789",
            FirstName = "Mário",
            LastName = "Admin"
        };

        this._user.CreateAsync(admin, "Mario123$").GetAwaiter().GetResult();
        this._user.AddToRoleAsync(admin, IdentityConfiguration.Admin).GetAwaiter().GetResult();

        IdentityResult adminClaims = this._user.AddClaimsAsync(admin, new Claim[] {
            new(JwtClaimTypes.Name, $"{admin.FirstName} {admin.LastName}"),
            new(JwtClaimTypes.GivenName, admin.FirstName),
            new(JwtClaimTypes.FamilyName, admin.LastName),
            new(JwtClaimTypes.Role, IdentityConfiguration.Admin)
        }).Result;

        ApplicationUser client = new() {
            UserName = "mario-client",
            Email = "mario-client@gmail.com",
            EmailConfirmed = true,
            PhoneNumber = "+55 (14) 123456789",
            FirstName = "Mário",
            LastName = "Client"
        };

        this._user.CreateAsync(client, "Mario123$").GetAwaiter().GetResult();
        this._user.AddToRoleAsync(client, IdentityConfiguration.Client).GetAwaiter().GetResult();

        IdentityResult clientClaims = this._user.AddClaimsAsync(client, new Claim[] {
            new(JwtClaimTypes.Name, $"{client.FirstName} {client.LastName}"),
            new(JwtClaimTypes.GivenName, client.FirstName),
            new(JwtClaimTypes.FamilyName, client.LastName),
            new(JwtClaimTypes.Role, IdentityConfiguration.Admin)
        }).Result;
    }
}