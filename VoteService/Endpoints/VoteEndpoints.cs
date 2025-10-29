using Contracts;
using Microsoft.EntityFrameworkCore;
using Reputation;
using System.Security.Claims;
using VoteService.Data;
using VoteService.DTOs;
using VoteService.Models;
using Wolverine;

namespace VoteService.Endpoints;

public static class VoteEndpoints
{
	public static void MapVoteEndpoints(this WebApplication app)
	{
		app.MapPost("/votes", PostUserVoteAsync).RequireAuthorization();
		app.MapGet("/votes/{questionId}", GetUserVotesByQuestion).RequireAuthorization();
	}

	public static async Task<IResult> PostUserVoteAsync(CastVoteDto dto, VoteDbContext db, ClaimsPrincipal user, IMessageBus bus)
	{
		var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
		if (userId is null) return Results.Unauthorized();

		if (dto.TargetType is not ("Question" or "Answer"))
			return Results.BadRequest("Invalid target type");

		var alreadyVoted = await db.Votes.AsNoTracking().AnyAsync(x => x.UserId == userId && x.TargetId == dto.TargetId);
			if (alreadyVoted) return Results.BadRequest("Already voted");

		db.Votes.Add(new Vote
		{
			TargetId = dto.TargetId,
			TargetType = dto.TargetType,
			UserId = userId,
			VoteValue = dto.VoteValue,
			QuestionId = dto.QuestionId
		});

		await db.SaveChangesAsync();
		var reason = (dto.VoteValue, dto.TargetType) switch
		{
			(1, "Question") => ReputationReason.QuestionUpvoted,
			(1, "Answer") => ReputationReason.AnswerUpvoted,
			(-1, "Question") => ReputationReason.QuestionDownvoted,
			_ => ReputationReason.QuestionDownvoted

		};

		await bus.PublishAsync(ReputationHelper.MakeEvent(dto.TargetId, reason, userId));
		await bus.PublishAsync(new VoteCasted(dto.TargetId, dto.TargetType, dto.VoteValue));

		return Results.NoContent();
	}

	public static async Task<IResult> GetUserVotesByQuestion(string questionId, VoteDbContext db, ClaimsPrincipal user)
	{
		var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
		if (userId is null) return Results.Unauthorized();

		var votes = await db.Votes
			.AsNoTracking()
			.Where(x => x.UserId == userId && x.QuestionId == questionId)
			.Select(x => new UserVotesResult(x.TargetId, x.TargetType, x.VoteValue))
			.ToListAsync();

		return Results.Ok(votes);
	}
}
