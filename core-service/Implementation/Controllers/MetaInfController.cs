using System;
using System.Threading.Tasks;
using AIQXCommon.Middlewares;
using AIQXCommon.Services;
using AIQXCoreService.Implementation.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Opw.HttpExceptions;

namespace AIQXCoreService.Implementation.Controllers
{
    [ApiController]
    [Route("v1/meta-inf")]
    public class MetaInfController : ControllerBase
    {
        private readonly ILogger<MetaInfController> _logger;
        private readonly UserService _userService;

        public MetaInfController(ILogger<MetaInfController> logger, AttachmentService attachmentService, IMapper mapper)
        {
            _logger = logger;
            _userService = new UserService("");
        }

        [HttpGet("me")]
        public ActionResult<object> GetMe()
        {
            var auth = HttpContext.GetAuthorizationOrNull();

            if (auth == null)
            {
                throw new UnauthorizedException("No authorization available");
            }

            return Ok(auth.ToExternal());
        }

        [HttpGet("user/{id}")]
        public ActionResult GetUser(string id)
        {
            var user = _userService.getUserByIdAsync(id);
            return Ok(user);
        }

        [HttpGet("status")]
        public ActionResult GetStatus()
        {
            return Ok(new
            {
                Ready = true,
                Uptime = DateTimeOffset.Now.ToUnixTimeSeconds() - Startup.StartTime,
            });
        }

    }
}
