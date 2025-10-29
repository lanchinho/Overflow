using Contracts;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionService.Data;
using QuestionService.DTOs;
using QuestionService.Models;
using QuestionService.Services;
using Reputation;
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

		var sanitizer = new HtmlSanitizer();
		var question = new Question
		{
			Title = dto.Title,
			Content = sanitizer.Sanitize(dto.Content),
			TagSlugs = dto.Tags,
			AskerId = userId			
		};

		if (!await tagService.AreTagsValidAsync(question.TagSlugs))
			return BadRequest("Invalid tags!");

		db.Questions.Add(question);
		await db.SaveChangesAsync();

		var slugs = question.TagSlugs.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
		if(slugs.Length > 0)
		{
			await db.Tags
				.Where(t => slugs.Contains(t.Slug))
				.ExecuteUpdateAsync(x => x.SetProperty(t => t.UsageCount,
					t => t.UsageCount + 1));			
		}

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
		var question = await db.Questions.AsNoTracking()
			.Include(x=> x.Answers)
			.FirstOrDefaultAsync(x=> x.Id == id);

		if (question is null)
			return NotFound("Question was not found !");

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

		var original = questionInDb.TagSlugs.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
		var incoming = request.Tags.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

		var removed = original.Except(incoming).ToArray();
		var added = incoming.Except(original).ToArray();

		var sanitizer = new HtmlSanitizer();
		questionInDb.Title = request.Title;
		questionInDb.Content = sanitizer.Sanitize(request.Content);
		questionInDb.TagSlugs = request.Tags;
		questionInDb.UpdatedAt = DateTime.UtcNow;
		await db.SaveChangesAsync();

		if(removed.Length > 0)
		{
			await db.Tags
				.Where(t => removed.Contains(t.Slug) && t.UsageCount > 0)
				.ExecuteUpdateAsync(x => x.SetProperty(t => t.UsageCount, t => t.UsageCount - 1));
		}

		if (added.Length > 0)
		{
			await db.Tags
				.Where(t => added.Contains(t.Slug))
				.ExecuteUpdateAsync(x => x.SetProperty(t => t.UsageCount, t => t.UsageCount + 1));
		}

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

	[Authorize]
	[HttpPost("{questionId}/answers")]
	public async Task<IActionResult> AnswerQuestionAsync([FromRoute] string questionId, CreateAnswerDto dto)
	{
		var question = await db.Questions.FindAsync(questionId);
		if (question is null)
			return NotFound("Question was not found !");

		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		var name = User.FindFirstValue("Name");
		if (userId is null || name is null) return BadRequest("Cannot get user details.");

		var sanitizer = new HtmlSanitizer();
		var answer = new Answer
		{
			Content = sanitizer.Sanitize(dto.Content),
			UserId = userId,			
			QuestionId = questionId
		};

		question.Answers.Add(answer);
		question.AnswerCount++;
		await db.SaveChangesAsync();

		await bus.PublishAsync(new UpdatedAnswerCount(questionId, question.AnswerCount));

		return Created($"/questions/{question.Id}", answer);
	}

	[Authorize]
	[HttpPut("{questionId}/answers/{answerId}")]
	public async Task<IActionResult> UpdateAnswerAsync([FromRoute] string questionId, [FromRoute] string answerId, [FromBody] CreateAnswerDto dto)
	{
		var answerInDb = await db.Answers.FindAsync(answerId);
		if (answerInDb is null)
			return NotFound("Answer was not found !");

		if (answerInDb.QuestionId != questionId)
			return BadRequest("Cannot update answer details.");

		var sanitizer = new HtmlSanitizer();
		answerInDb.Content = sanitizer.Sanitize(dto.Content);
		answerInDb.UpdatedAt = DateTime.UtcNow;
		await db.SaveChangesAsync();

		return NoContent();
	}

	[Authorize]
	[HttpDelete("{questionId}/answers/{answerId}")]
	public async Task<IActionResult> DeleteAnswerAsync([FromRoute] string questionId, [FromRoute] string answerId)
	{
		var question = await db.Questions.FindAsync(questionId);
		var answer = await db.Answers.FindAsync(answerId);
		if (question is null || answer is null)
			return NotFound();

		if (answer.QuestionId != questionId || answer.Accepted)
			return BadRequest("Cannot delete this answer.");

		question.Answers.Remove(answer);
		question.AnswerCount--;
		await db.SaveChangesAsync();

		await bus.PublishAsync(new UpdatedAnswerCount(questionId, question.AnswerCount));

		return NoContent();
	}

	[Authorize]
	[HttpPost("{questionId}/answers/{answerId}/accept")]
	public async Task<IActionResult> AcceptAnswerAsync([FromRoute] string questionId, [FromRoute] string answerId)
	{
		var question = await db.Questions.FindAsync(questionId);
		var answer = await db.Answers.FindAsync(answerId);

		if (question is null || answer is null) return NotFound();

		if (question.HasAcceptedAnswer || answer.QuestionId != questionId)
			return BadRequest("Cannot accept answer");
		
		question.HasAcceptedAnswer = answer.Accepted = true;
		await db.SaveChangesAsync();

		await bus.PublishAsync(new AnswerAccepted(questionId));
		await bus.PublishAsync(ReputationHelper.MakeEvent(answer.UserId,
			ReputationReason.AnswerAccepted, question.AskerId));

		return NoContent();
	}
}
