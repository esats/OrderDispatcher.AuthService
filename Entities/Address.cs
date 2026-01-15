namespace OrderDispatcher.AuthService.Entities
{
    public class Address : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
    }
}
