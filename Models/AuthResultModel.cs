namespace OrderDispatcher.AuthService.Models
{
    public class AuthResultModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string BearerToken { get; set; }
    }
}
