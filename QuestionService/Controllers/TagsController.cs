using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionService.Data;
using QuestionService.Models;


namespace QuestionService.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TagsController(QuestionDbContext db) : ControllerBase
	{
		[HttpGet]
		public async Task<IActionResult> GetTagsAsync()
		{
			IReadOnlyList<Tag> tags = await db.Tags
				.AsNoTracking()
				.OrderBy(x => x.Name)
				.ToListAsync();

			return Ok(tags);
		}
	}
}
