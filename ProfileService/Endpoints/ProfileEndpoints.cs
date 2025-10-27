using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ProfileService.Data;
using ProfileService.DTOs;
using ProfileService.Models;
using System.Security.Claims;

namespace ProfileService.Endpoints;

public static class ProfileEndpoints
{
	public static void MapProfileEndpoints(this WebApplication app)
	{
		app.MapGet("/profiles/me", UserProfileAsync).RequireAuthorization();
		app.MapGet("/profiles/batch", UserProfilesBatchAsync);
	}

	public static async Task<Results<Ok<UserProfile>, NotFound, UnauthorizedHttpResult>> UserProfileAsync(ClaimsPrincipal user, ProfileDbContext db)
	{
		var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
		if (userId is null) return TypedResults.Unauthorized();

		var profile = await db.UserProfiles.FindAsync(userId);
		return profile is null ? TypedResults.NotFound() : TypedResults.Ok(profile);
	}

	public static async Task<Results<Ok<List<ProfileSummaryDto>>, NotFound>> UserProfilesBatchAsync(string ids, ProfileDbContext db)
	{
		var list = ids.Split(",", StringSplitOptions.RemoveEmptyEntries).Distinct();
		var rows = await db.UserProfiles
			.AsNoTracking()
			.Where(x => list.Contains(x.Id))
				.Select(x => new ProfileSummaryDto(x.Id, x.DisplayName, x.Reputation))
				.ToListAsync();

		return rows.Count > 0 ? TypedResults.Ok(rows) : TypedResults.NotFound();
	}


}
