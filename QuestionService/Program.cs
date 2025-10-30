using Common;
using QuestionService.Data;
using QuestionService.Services;

namespace QuestionService
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			builder.Services.AddOpenApi();
			builder.AddServiceDefaults();
			builder.Services.AddMemoryCache();
			builder.Services.AddScoped<TagService>();
			builder.Services.AddKeyCloakAuthentication();

			builder.AddNpgsqlDbContext<QuestionDbContext>("questionDb");
			await builder.UseWolverineWithRabbitMqAsync(builder.Configuration, opts =>
			{				
				opts.ApplicationAssembly = typeof(Program).Assembly;
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi();
			}

			app.UseAuthorization();


			app.MapControllers();
			app.MapDefaultEndpoints();

			await app.MigrateDbContextAsync<QuestionDbContext>();

			app.Run();
		}
	}
}
