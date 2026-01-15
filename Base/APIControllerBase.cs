using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Linq;
using System;
using System.Security.Claims;
using System.Net.Http;

namespace OrderDispatcher.CatalogService.API.Base
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class APIControllerBase : ControllerBase
    {
        [NonAction]
        public string GetUser()
        {
            var userId = HttpContext.User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).ToList()[0]?.Value;
            return userId;
        }
    }
}
