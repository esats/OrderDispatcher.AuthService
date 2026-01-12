using System.ComponentModel.DataAnnotations;

namespace OrderDispatcher.AuthService.Models
{
    public class RegisterModel
    {  
        [Required, MinLength(3), MaxLength(32)]
        public string Username { get; init; } = default!;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; init; } = default!;

        [Required, MinLength(6), MaxLength(128)]
        public string Password { get; init; } = default!;
    }
}
