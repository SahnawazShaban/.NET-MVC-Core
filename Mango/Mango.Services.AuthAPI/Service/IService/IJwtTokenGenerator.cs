using Mango.Services.AuthAPI.Models;

namespace Mango.Services.AuthAPI.Service.IService
{
    public interface IJwtTokenGenerator
    {
        // its possible, user have multiple roles - IEnumerable<string> roles
        string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles); 
    }
}
