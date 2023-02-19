using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using GeekShopping.IdentityServer.Model;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GeekShopping.IdentityServer.Services;

public class ProfileService : IProfileService {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;

    public ProfileService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory
    ) {
        this._userManager = userManager;
        this._roleManager = roleManager;
        this._userClaimsPrincipalFactory = userClaimsPrincipalFactory;
    }

    public UserManager<ApplicationUser> UserManager => _userManager;

    public async Task GetProfileDataAsync(ProfileDataRequestContext context) {
        string id = context.Subject.GetSubjectId();
        ApplicationUser user = await this._userManager.FindByIdAsync(id);
        ClaimsPrincipal userClaims = await this._userClaimsPrincipalFactory.CreateAsync(user);
        List<Claim> claims = userClaims.Claims.ToList();
        claims.Add(new(JwtClaimTypes.FamilyName, user.LastName));
        claims.Add(new(JwtClaimTypes.GivenName, user.FirstName));

        if (this._userManager.SupportsUserRole) {
            IList<string> roles = await this._userManager.GetRolesAsync(user);
            foreach (string role in roles) {
                claims.Add(new(JwtClaimTypes.Role, role));
                if (this._roleManager.SupportsRoleClaims) {
                    IdentityRole identityRole = await this._roleManager.FindByNameAsync(role);
                    if (identityRole is not null)
                        claims.AddRange(await this._roleManager.GetClaimsAsync(identityRole));
                }
            }
        }

        context.IssuedClaims = claims;
    }

    public async Task IsActiveAsync(IsActiveContext context) {
        string id = context.Subject.GetSubjectId();
        ApplicationUser user = await this._userManager.FindByIdAsync(id);
        context.IsActive = user is not null;
    }
}