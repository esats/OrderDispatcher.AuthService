using Microsoft.EntityFrameworkCore;
using OrderDispatcher.AuthService.Entities;
using OrderDispatcher.AuthService.Models;

namespace OrderDispatcher.AuthService.Services
{
    public class ProfileService
    {
        private readonly AuthDbContext _db;

        public ProfileService(AuthDbContext db)
        {
            _db = db;
        }

        public async Task<Response<Profile>> SaveAsync(ProfileSaveModel request, string userId)
        {
            var response = new Response<Profile>();

            try
            {
                if (request is null)
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid payload.";
                    return response;
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    response.IsSuccess = false;
                    response.Message = "User not found.";
                    return response;
                }

                var profile = await _db.Profiles.FirstOrDefaultAsync(x => x.UserId == userId);
                var now = DateTime.UtcNow;

                if (profile is null)
                {
                    profile = new Profile
                    {
                        UserId = userId,
                        CreatedAtUtc = now
                    };
                    _db.Profiles.Add(profile);
                }

                profile.FirstName = request.FirstName?.Trim();
                profile.LastName = request.LastName?.Trim();
                profile.PhoneNumber = request.PhoneNumber?.Trim();
                profile.UpdatedAtUtc = now;

                await _db.SaveChangesAsync();

                response.Value = profile;
                response.IsSuccess = true;
                response.Message = "Profile saved.";
                return response;
            }
            catch (Exception ex)
            {
                response.Exception = ex;
                response.IsSuccess = false;
                response.Message = "An unexpected error occurred.";
                return response;
            }
        }

        public async Task<Response<Profile>> GetOneAsync(string userId, CancellationToken ct)
        {
            var response = new Response<Profile>();

            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    response.IsSuccess = false;
                    response.Message = "User not found.";
                    return response;
                }

                var profile = await _db.Profiles.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserId == userId, ct);

                response.Value = profile;
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Exception = ex;
                response.IsSuccess = false;
                response.Message = "An unexpected error occurred.";
                return response;
            }
        }
    }
}
