using Marten;
using StatsService.Models;

namespace StatsService.Endpoints;

public static class StatsEndpoints
{
	public static void MapStatsEndpoints(this WebApplication app)
	{
		app.MapGet("/stats/trending-tags", GetTrendingTagsAsync);
		app.MapGet("/stats/top-users", GetTopUsersAsync);
	}

	public static async Task<IResult> GetTrendingTagsAsync(IQuerySession session)
	{
		var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
		var start = today.AddDays(-6);

		var rows = await session.Query<TagDailyUsage>()
			.Where(x => x.Date >= start && x.Date <= today)
			.Select(x => new { x.Tag, x.Count })
			.ToListAsync();

		var top = rows
			.GroupBy(x => x.Tag)
			.Select(x => new { tag = x.Key, count = x.Sum(t => t.Count) })
			.OrderByDescending(x => x.count)
			.Take(5)
			.ToList();

		return Results.Ok(top);
	}

	public static async Task<IResult> GetTopUsersAsync(IQuerySession session)
	{
		var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
		var start = today.AddDays(-6);

		var rows = await session.Query<UserDailyReputation>()
			.Where(x => x.Date >= start && x.Date <= today)
			.Select(x => new { x.UserId, x.Delta })
			.ToListAsync();

		var top = rows
			.GroupBy(x => x.UserId)
			.Select(g => new { userId = g.Key, delta = g.Sum(t => t.Delta) })
			.OrderByDescending(x => x.delta)
			.Take(5)
			.ToList();

		return Results.Ok(top);
	}
}
