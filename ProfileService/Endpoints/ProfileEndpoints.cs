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
		app.MapGet("/profiles/me", GetUserProfileAsync).RequireAuthorization();
		app.MapGet("/profiles/batch", GetUserProfilesAsync);
		app.MapGet("/profiles", GetUserProfileByFilterAsync);
		app.MapGet("/profiles/{id}", GetUserProfileByIdAsync);
		app.MapPut("/profiles/edit", EditProfileAsync).RequireAuthorization();
	}

	public static async Task<Results<Ok<UserProfile>, NotFound, UnauthorizedHttpResult>> GetUserProfileAsync(ClaimsPrincipal user, ProfileDbContext db)
	{
		var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
		if (userId is null) return TypedResults.Unauthorized();

		var profile = await db.UserProfiles.FindAsync(userId);
		return profile is null ? TypedResults.NotFound() : TypedResults.Ok(profile);
	}

	public static async Task<Results<Ok<List<ProfileSummaryDto>>, NotFound>> GetUserProfilesAsync(string ids, ProfileDbContext db)
	{
		var list = ids.Split(",", StringSplitOptions.RemoveEmptyEntries).Distinct();
		var rows = await db.UserProfiles
			.AsNoTracking()
			.Where(x => list.Contains(x.Id))
				.Select(x => new ProfileSummaryDto(x.Id, x.DisplayName, x.Reputation))
				.ToListAsync();

		return rows.Count > 0 ? TypedResults.Ok(rows) : TypedResults.NotFound();
	}

	public static async Task<Results<Ok<List<UserProfile>>, NotFound>> GetUserProfileByFilterAsync(string? sortBy, ProfileDbContext db)
	{
		var query = db.UserProfiles.AsNoTracking().AsQueryable();
		if (query == null || !query.Any())
			return TypedResults.NotFound();

		query = sortBy == "reputation"
			? query.OrderByDescending(x => x.Reputation)
			: query.OrderBy(x => x.DisplayName);

		return TypedResults.Ok(await query.ToListAsync());
	}

	public static async Task<Results<Ok<UserProfile>, NotFound>> GetUserProfileByIdAsync(string id, ProfileDbContext db)
	{
		var profile = await db.UserProfiles.FindAsync(id);
		return profile is null ? TypedResults.NotFound() : TypedResults.Ok(profile);
	}

	public static async Task<Results<NoContent, UnauthorizedHttpResult, NotFound>> EditProfileAsync(EditProfileDto dto, ClaimsPrincipal user, ProfileDbContext db)
	{
		var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
		if (userId is null) return TypedResults.Unauthorized();

		var profile = await db.UserProfiles.FindAsync(userId);
		if (profile is null) return TypedResults.NotFound();

		profile.DisplayName = dto.DisplayName ?? profile.DisplayName;
		profile.Description = dto.Description ?? profile.Description;

		await db.SaveChangesAsync();

		return TypedResults.NoContent();		
	}
}
