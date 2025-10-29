using Common;
using Marten;
using JasperFx.Events;
using Contracts;
using StatsService.Models;
using StatsService.Projections;
using JasperFx.Events.Projections;
using StatsService.Endpoints;

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
		//"transforma o Postgres em um event store
		builder.Services.AddMarten(opts =>
		{
			opts.Connection(builder.Configuration.GetConnectionString("statsDb")!);

			opts.Events.StreamIdentity = StreamIdentity.AsString;
			opts.Events.AddEventType<QuestionCreated>();
			opts.Events.AddEventType<UserReputationChanged>();

			opts.Schema.For<TagDailyUsage>()
				.Index(x => x.Tag)
				.Index(x => x.Date);
			
			opts.Schema.For<UserReputationChanged>()
				.Index(x => x.UserId)
				.Index(x => x.Occurred);


			opts.Projections.Add(new TrendingTagsProjection(), ProjectionLifecycle.Inline);
			opts.Projections.Add(new TopUsersProjection(), ProjectionLifecycle.Inline);

		}).UseLightweightSessions();

		var app = builder.Build();

		app.MapStatsEndpoints();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.MapOpenApi();
		}		

		app.Run();
	}
}
