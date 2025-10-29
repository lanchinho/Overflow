using Common;
using Microsoft.EntityFrameworkCore;
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

			using var scope = app.Services.CreateScope();
			var services = scope.ServiceProvider;
			try
			{
				var context = services.GetRequiredService<VoteDbContext>();
				await context.Database.MigrateAsync();
			}
			catch (Exception e)
			{
				var logger = services.GetRequiredService<ILogger<Program>>();
				logger.LogError(e, "An error occurred while migrating or seeding the database.");
			}

			app.Run();
		}
	}
}
