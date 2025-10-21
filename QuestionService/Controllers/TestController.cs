using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace QuestionService.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
	[HttpGet("errors")]
	public ActionResult GetErrorResponse(int code)
	{
		ModelState.AddModelError("Problem one", "Validation problem one");
		ModelState.AddModelError("Problem two", "Validation problem two");

		return code switch
		{
			400 => BadRequest("Opposite of good request"),
			401 => Unauthorized(),
			403 => Forbid(),
			404 => NotFound(),
			500 => throw new Exception("This is a server error"),
			_ => ValidationProblem(ModelState)
		};
	}

	[Authorize]
	[HttpGet("auth")]
	public IActionResult TestAuth()
	{
		var user = User.FindFirstValue("name");
		return Ok($"{user} has been authorized");
	}
}
