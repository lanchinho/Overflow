using Common;
using SearchService.Data;
using SearchService.Endpoints;
using Typesense;
using Wolverine.RabbitMQ;

namespace SearchService;

public class Program
{
	public static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		builder.Services.AddAuthorization();

		// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
		builder.Services.AddOpenApi()
			.AddTypesense(builder.Configuration);

		builder.AddServiceDefaults();

		await builder.UseWolverineWithRabbitMqAsync(builder.Configuration, opts =>
		{
			opts.ListenToRabbitQueue("questions.search", cfg =>
			{
				cfg.BindExchange("questions");
			});
			opts.ApplicationAssembly = typeof(Program).Assembly;
		});

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.MapOpenApi();
		}

		app.MapDefaultEndpoints();
		app.MapSearchEndpoints();

		using var scope = app.Services.CreateScope();
		var client = scope.ServiceProvider.GetRequiredService<ITypesenseClient>();
		await SearchInitializer.EnsureIndexExistsAsynxc(client);

		app.Run();
	}
}
