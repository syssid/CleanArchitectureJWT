using Application.Contracts;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IUser user;

        public UserController(IUser user)
        {
			this.user = user;
        }
		[HttpPost("login")]
		public async Task<ActionResult<LoginResponse>> LoginUser(LoginDTO loginDTO)
		{
			var result = await user.LoginUserAsync(loginDTO);

			return Ok(result);
		}

		[HttpPost("register")]
		public async Task<ActionResult<RegistrationResponse>> RegisterUser(RegisterUserDTO registerUserDTO)
		{
			var result = await user.RegisterUserAsync(registerUserDTO);

			return Ok(result);
		}
	}
}
