using Microsoft.AspNetCore.Identity;

namespace OrderDispatcher.AuthService.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
