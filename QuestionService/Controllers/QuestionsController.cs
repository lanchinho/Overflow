using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionService.Data;
using QuestionService.DTOs;
using QuestionService.Models;
using System.Security.Claims;

namespace QuestionService.Controllers;

[ApiController]
[Route("[controller]")]
public class QuestionsController(QuestionDbContext db) : ControllerBase
{
	[Authorize]
	[HttpPost]
	public async Task<IActionResult> CreateQuestionAsync(CreateQuestionDto dto)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		var name = User.FindFirstValue("Name");

		if (userId is null || name is null)
			return BadRequest("Cannot get user details");

		var question = new Question
		{
			Title = dto.Title,
			Content = dto.Content,
			TagSlugs = dto.Tags,
			AskerId = userId,
			AskerDisplayName = name
		};

		var TagErrorMsg = await ValidateTags(question.TagSlugs);
		if (!string.IsNullOrWhiteSpace(TagErrorMsg))
			return BadRequest(TagErrorMsg);

		db.Questions.Add(question);
		await db.SaveChangesAsync();

		return Created($"/questions/{question.Id}", question);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetQuestionAsync(string id)
	{
		var question = await db.Questions.FindAsync(id);
		if (question is null)
			return NotFound("Question not found !");

		await db.Questions.Where(x => x.Id == id)
			.ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ViewCount,
				x => x.ViewCount + 1));

		return Ok(question);
	}

	[HttpGet]
	public async Task<IActionResult> GetQuestionsAsync(string? tag)
	{
		var query = db.Questions
			.AsNoTracking()
			.AsQueryable();

		if (!string.IsNullOrWhiteSpace(tag))
			query = query.Where(x => x.TagSlugs.Contains(tag));

		return Ok(await query.OrderByDescending(x => x.CreatedAt).ToListAsync());
	}

	[Authorize]
	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateQuestionAsync([FromRoute] string id, [FromBody] CreateQuestionDto request)
	{
		var questionInDb = await db.Questions.FindAsync(id);
		if (questionInDb is null)
			return NotFound("Question was not found !");

		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (userId is null)
			return Forbid();

		var TagErrorMsg = await ValidateTags(request.Tags);
		if (!string.IsNullOrWhiteSpace(TagErrorMsg))
			return BadRequest(TagErrorMsg);

		questionInDb.Title = request.Title;
		questionInDb.Content = request.Content;
		questionInDb.TagSlugs = request.Tags;
		questionInDb.UpdatedAt = DateTime.UtcNow;
		await db.SaveChangesAsync();

		return NoContent();
	}

	[Authorize]
	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteQuestionAsync([FromRoute] string id)
	{
		var questionToRemove = await db.Questions.FindAsync(id);
		if (questionToRemove is null)
			return NotFound("Question was not found !");

		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (userId is null)
			return Forbid();

		db.Questions.Remove(questionToRemove);
		await db.SaveChangesAsync();
		return NoContent();
	}

	private async Task<string> ValidateTags(List<string> tags)
	{
		var validTags = await db.Tags.Where(x => tags.Contains(x.Slug))
			.AsNoTracking()
			.ToListAsync();

		var invalidTags = tags.Except(validTags.Select(x => x.Slug)).ToList();
		if (invalidTags.Count != 0)
			return $"Invalid tags: {string.Join(",", invalidTags)}";

		return "";
	}
}
