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

        public async Task<Response<Address>> SaveAddressAsync(AddressSaveModel request, string userId)
        {
            var response = new Response<Address>();

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

                var title = request.Title?.Trim();
                var addressLine = request.Address?.Trim();

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(addressLine))
                {
                    response.IsSuccess = false;
                    response.Message = "Title and address are required.";
                    return response;
                }

                var now = DateTime.UtcNow;
                var address = new Address
                {
                    UserId = userId,
                    Title = title,
                    AddressLine = addressLine,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                };

                _db.Addresses.Add(address);
                await _db.SaveChangesAsync();

                response.Value = address;
                response.IsSuccess = true;
                response.Message = "Address saved.";
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

        public async Task<Response<Address>> GetAddressAsync(int addressId, string userId, CancellationToken ct)
        {
            var response = new Response<Address>();

            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    response.IsSuccess = false;
                    response.Message = "User not found.";
                    return response;
                }

                var address = await _db.Addresses.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == addressId && x.UserId == userId, ct);

                response.Value = address;
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

        public async Task<Response<List<Address>>> GetAddressesAsync(string userId, CancellationToken ct)
        {
            var response = new Response<List<Address>>();

            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    response.IsSuccess = false;
                    response.Message = "User not found.";
                    return response;
                }

                var addresses = await _db.Addresses.AsNoTracking()
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.UpdatedAtUtc)
                    .ToListAsync(ct);

                response.Value = addresses;
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
