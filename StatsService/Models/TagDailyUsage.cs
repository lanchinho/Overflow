namespace StatsService.Models;

public class TagDailyUsage
{
	public required string Id { get; set; }
	public string Tag { get; set; }
	public DateOnly Date { get; set; }
	public int Count { get; set; }
}
