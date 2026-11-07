using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Volo.Abp.Security.Claims;

namespace Tutor.Controllers.Articles;

public static class CustomClaimsExtensions
{
    public static Guid? FindMyUserId(this ClaimsPrincipal principal)
    {
        // Пытаемся найти ID по разным стандартным именам
        var claim = principal.FindFirst(AbpClaimTypes.UserId) 
                    ?? principal.FindFirst(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)
                    ?? principal.FindFirst("sub");

        if (claim != null && Guid.TryParse(claim.Value, out var guid))
        {
            return guid;
        }
        
        return null;
    }
}