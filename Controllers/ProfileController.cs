using Microsoft.AspNetCore.Mvc;
using OrderDispatcher.AuthService.Entities;
using OrderDispatcher.AuthService.Models;
using OrderDispatcher.AuthService.Services;
using OrderDispatcher.CatalogService.API.Base;

namespace OrderDispatcher.AuthService.Controllers
{
    [ApiController]
    [Route("api/auth/profile")]
    public class ProfileController : APIControllerBase
    {
        private readonly ProfileService _profileService;

        public ProfileController(ProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpPost("save")]
        public async Task<Response<Profile>> Save([FromBody] ProfileSaveModel request)
        {
            var userId = GetUser();
            return await _profileService.SaveAsync(request, userId);
        }

        [HttpGet("getOne/{userId}")]
        public async Task<Response<Profile>> GetOne([FromRoute] string userId, CancellationToken ct)
        {
            return await _profileService.GetOneAsync(userId, ct);
        }
    }
}
