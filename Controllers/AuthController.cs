using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using OrderDispatcher.AuthService.Entities;
using OrderDispatcher.AuthService.Models;
using OrderDispatcher.AuthService.Services;
using OrderDispatcher.CatalogService.API.Base;
using System.Net;

namespace OrderDispatcher.AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : APIControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TokenService _tokenService;
    private readonly IProfileMessagePublisher _publisher;

    public AuthController(UserManager<ApplicationUser> userManager,
                          RoleManager<IdentityRole> roleManager,
                         TokenService tokenService,
                         IProfileMessagePublisher publisher)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _publisher = publisher;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<Response<HttpStatusCode>> Register([FromBody] RegisterModel request)
    {
        Response<HttpStatusCode> response = new();
        try
        {
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                return response;
            }

            var username = request.Username.Trim();
            var email = request.Email.Trim();

            var existingByEmail = await _userManager.FindByEmailAsync(email);
            if (existingByEmail is not null)
            {
                response.IsSuccess = false;
                response.Message = "Email already in use.";
                return response;
            }

            var existingByName = await _userManager.FindByNameAsync(username);

            if (existingByName is not null)
            {
                response.IsSuccess = false;
                response.Message = "Username already in use.";
                return response;
            }

            if (request.UserType < 1 || request.UserType > 4)
            {
                response.IsSuccess = false;
                response.Message = "Invalid userType.";
                return response;
            }

            var user = new ApplicationUser
            {
                UserName = username,
                Email = email,
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                response.Message = result.Errors.ToList()[0].ToString();
                response.IsSuccess = false;

                return response;
            }

            var roleName = request.UserType switch
            {
                1 => "customer",
                2 => "driver",
                3 => "store",
                4 => "admin",
                _ => null
            };

            if (string.IsNullOrWhiteSpace(roleName))
            {
                response.IsSuccess = false;
                response.Message = "Invalid userType.";
                return response;
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                response.IsSuccess = false;
                response.Message = "Role not found.";
                return response;
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!addToRoleResult.Succeeded)
            {
                response.Message = addToRoleResult.Errors.ToList()[0].ToString();
                response.IsSuccess = false;
                return response;
            }

            var profileModel = new ProfileModel
            {
                UserId = user.Id,
                Username = username,
                Email = email,
                FirstName = request.FirstName,
                UserRole = request.UserType
            };

            await _publisher.PublishProfileCreatedAsync(profileModel);
        }
        catch (Exception e)
        {
            response.Exception = e;
            response.IsSuccess = false;
        }

        response.Value = HttpStatusCode.OK;
        response.IsSuccess = true;

        return response;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<Response<AuthResultModel>> Login([FromBody] LoginModel request, CancellationToken ct)
    {
        var response = new Response<AuthResultModel>();

        try
        {
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.Message = "Invalid payload.";
                return response;
            }

            var id = request.Email?.Trim();
            var pwd = request.Password?.Trim();

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(pwd))
            {
                response.IsSuccess = false;
                response.Message = "Username/email and password are required.";
                return response;
            }

            ApplicationUser? user =
                await _userManager.FindByEmailAsync(id) ??
                await _userManager.FindByNameAsync(id);

            if (user is null)
            {
                response.IsSuccess = false;
                response.Message = "Invalid credentials.";
                return response;
            }

            // No lockout usage; simple password check
            var valid = await _userManager.CheckPasswordAsync(user, pwd);
            if (!valid)
            {
                response.IsSuccess = false;
                response.Message = "Invalid credentials.";
                return response;
            }

            // Create JWT
            var token = await _tokenService.CreateAsync(user, ct);

            response.Value = new AuthResultModel
            {
                UserId = user.Id,
                BearerToken = token,
                Email = user.Email
            };

            response.IsSuccess = true;
            response.Message = "Login successful.";
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
