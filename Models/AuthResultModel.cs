namespace OrderDispatcher.AuthService.Models
{
    public class AuthResultModel
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string BearerToken { get; set; }
    }
}
