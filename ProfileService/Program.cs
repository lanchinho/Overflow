
using Common;
using Microsoft.EntityFrameworkCore;
using ProfileService.Data;
using ProfileService.Endpoints;
using ProfileService.Middleware;

namespace ProfileService;

public class Program
{
	public static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		builder.Services.AddAuthorization();

		// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
		builder.Services.AddOpenApi();
		builder.AddServiceDefaults();
		builder.Services.AddKeyCloakAuthentication();
		await builder.UseWolverineWithRabbitMqAsync(builder.Configuration, opts =>
		{
			opts.ApplicationAssembly = typeof(Program).Assembly;
		});

		builder.AddNpgsqlDbContext<ProfileDbContext>("profileDb");

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.MapOpenApi();
		}

		app.MapDefaultEndpoints();
		app.UseMiddleware<UserProfileCreationMiddleware>();
		app.MapProfileEndpoints();

		using var scope = app.Services.CreateScope();
		var services = scope.ServiceProvider;
		try
		{
			var context = services.GetRequiredService<ProfileDbContext>();
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
