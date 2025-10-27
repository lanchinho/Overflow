﻿using Microsoft.AspNetCore.Mvc;
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
		public async Task<IActionResult> GetTagsAsync(string? sort)
		{
			var query = db.Tags.AsNoTracking().AsQueryable();

			query = sort == "popular"
				? query.OrderByDescending(x => x.UsageCount).ThenBy(x => x.Name)
				: query.OrderBy(x => x.Name);

			return Ok(await query.ToListAsync());
		}
	}
}
