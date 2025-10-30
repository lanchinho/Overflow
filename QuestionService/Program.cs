using Common;
using Contracts;
using Microsoft.EntityFrameworkCore;
using QuestionService.Data;
using QuestionService.Services;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

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

			//RIP aspire way to get cs lol...
			//but it was needed for message durability feature in wolverine
			//builder.AddNpgsqlDbContext<QuestionDbContext>("questionDb");

			var connectionString = builder.Configuration.GetConnectionString("questionDb")!;
			builder.Services.AddDbContext<QuestionDbContext>(options =>
			{
				options.UseNpgsql(connectionString);
			}, optionsLifetime: ServiceLifetime.Singleton);

			await builder.UseWolverineWithRabbitMqAsync(builder.Configuration, opts =>
			{				
				opts.ApplicationAssembly = typeof(Program).Assembly;
				opts.PersistMessagesWithPostgresql(connectionString);
				opts.UseEntityFrameworkCoreTransactions();
				opts.PublishMessage<QuestionCreated>().ToRabbitExchange("Contracts.QuestionCreated").UseDurableOutbox();
				opts.PublishMessage<QuestionUpdated>().ToRabbitExchange("Contracts.QuestionUpdated").UseDurableOutbox();
				opts.PublishMessage<QuestionDeleted>().ToRabbitExchange("Contracts.QuestionDeleted").UseDurableOutbox();
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
