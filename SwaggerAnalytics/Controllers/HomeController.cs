using Microsoft.AspNetCore.Mvc;
using System.Dynamic;

namespace SwaggerAnalytics.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        [HttpGet("GetRealTimeUsers")]
        public IActionResult GetActiveUsers()
        {
            dynamic response = new ExpandoObject();
            var activeUsers = ActiveUsersService.ActiveUsers;
            response.count = activeUsers.Count();
            response.activeUsers = activeUsers;
            return Ok(response);
        }
    }
}
