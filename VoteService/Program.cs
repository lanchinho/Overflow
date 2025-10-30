using Common;
using VoteService.Data;
using VoteService.Endpoints;

namespace VoteService
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddAuthorization();

			builder.Services.AddOpenApi();
			builder.AddServiceDefaults();
			builder.Services.AddKeyCloakAuthentication();
			await builder.UseWolverineWithRabbitMqAsync(builder.Configuration, opts =>
			{
				opts.ApplicationAssembly = typeof(Program).Assembly;
			});

			builder.AddNpgsqlDbContext<VoteDbContext>("voteDb");

			var app = builder.Build();

			app.MapVoteEndpoints();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi();
			}

			await app.MigrateDbContextAsync<VoteDbContext>();

			app.Run();
		}
	}
}
