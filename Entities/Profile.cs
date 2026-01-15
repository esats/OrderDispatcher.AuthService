using System;

namespace OrderDispatcher.AuthService.Entities
{
    public class Profile : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
