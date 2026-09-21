using System.IdentityModel.Tokens.Jwt;

namespace UrlShortener.Data.Services
{
    public class CurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public CurrentUser GetCurrentUser()
        {
            var sub = _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (sub != null && int.TryParse(sub, out int managerId))
            {
                return new CurrentUser { ManagerId = managerId };
            }
            throw new InvalidOperationException("No current user");
        }
    }
    public class CurrentUser
    {
        public int ManagerId { get; set; }
    }
}
