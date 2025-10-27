
using Common;
using Marten;

namespace StatsService;

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
		await builder.UseWolverineWithRabbitMqAsync(builder.Configuration, opts =>
		{
			opts.ApplicationAssembly = typeof(Program).Assembly;
		});

		//usado para event sourcing
		//"transforma o Postgres em um BD de documento
		builder.Services.AddMarten(opts =>
		{
			opts.Connection(builder.Configuration.GetConnectionString("statsDb")!);
		}).UseLightweightSessions();

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.MapOpenApi();
		}		

		app.Run();
	}
}
