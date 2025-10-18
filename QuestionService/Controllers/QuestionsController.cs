using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionService.Data;
using QuestionService.DTOs;
using QuestionService.Models;
using QuestionService.Services;
using System.Security.Claims;
using Wolverine;

namespace QuestionService.Controllers;

[ApiController]
[Route("[controller]")]
public class QuestionsController(QuestionDbContext db,
	IMessageBus bus,
	TagService tagService) : ControllerBase
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

		if (!await tagService.AreTagsValidAsync(question.TagSlugs))
			return BadRequest("Invalid tags!");

		db.Questions.Add(question);
		await db.SaveChangesAsync();

		await bus.PublishAsync(new QuestionCreated(
			question.Id,
			question.Title,
			question.Content,
			question.CreatedAt,
			question.TagSlugs));

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

		if (!await tagService.AreTagsValidAsync(request.Tags))
			return BadRequest("Invalid tags!");

		questionInDb.Title = request.Title;
		questionInDb.Content = request.Content;
		questionInDb.TagSlugs = request.Tags;
		questionInDb.UpdatedAt = DateTime.UtcNow;
		await db.SaveChangesAsync();

		await bus.PublishAsync(new QuestionUpdated(questionInDb.Id, questionInDb.Title, questionInDb.Content, [.. questionInDb.TagSlugs]));
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
		await bus.PublishAsync(new QuestionDeleted(questionToRemove.Id));

		return NoContent();
	}
}
