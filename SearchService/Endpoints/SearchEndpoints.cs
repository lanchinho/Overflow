using Microsoft.AspNetCore.Http.HttpResults;
using SearchService.Models;
using System.Text.RegularExpressions;
using Typesense;

namespace SearchService.Endpoints;

public static class SearchEndpoints
{
	public static void MapSearchEndpoints(this WebApplication app)
	{
		app.MapGet("search", SearchAsync);
		app.MapGet("search/similar-titles", SearchSimilarTitlesAsync);
	}

	public static async Task<Results<Ok<IEnumerable<SearchQuestion>>, ProblemHttpResult>> SearchAsync(string query, ITypesenseClient client)
	{
		// [aspire]something
		string? tag = null;

		var tagMatch = Regex.Match(query, @"\[(.*?)\]");
		if (tagMatch.Success)
		{
			tag = tagMatch.Groups[1].Value;
			query = query.Replace(tagMatch.Value, "").Trim();
		}

		var searchParams = new SearchParameters(query, "title,content");
		if (!string.IsNullOrWhiteSpace(tag))
		{
			searchParams.FilterBy = $"tags:=[{tag}]";
		}
		try
		{
			var result = await client.Search<SearchQuestion>("questions", searchParams);
			return TypedResults.Ok(result.Hits.Select(hit => hit.Document));
		}
		catch (Exception e)
		{
			return TypedResults.Problem(detail: "Typesense search failed", statusCode: 500, instance: e.Message);
		}
	}
	
	public static async Task<Results<Ok<IEnumerable<SearchQuestion>>, ProblemHttpResult>> SearchSimilarTitlesAsync(string query,ITypesenseClient client)
	{
		var searchParams = new SearchParameters(query, "title");
		try
		{
			var result = await client.Search<SearchQuestion>("questions", searchParams);
			return TypedResults.Ok(result.Hits.Select(hit => hit.Document));
		}
		catch (Exception e)
		{
			return TypedResults.Problem("Typesense search failed", e.Message);
		}
	}
}
