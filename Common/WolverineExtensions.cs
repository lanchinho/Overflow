using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Common;

public static class WolverineExtensions
{
	public static async Task UseWolverineWithRabbitMqAsync(
		this IHostApplicationBuilder builder,
		IConfiguration config,
		Action<WolverineOptions> configureMessaging)
	{
		var isEfDesignTime = AppDomain.CurrentDomain.FriendlyName.StartsWith("ef", StringComparison.OrdinalIgnoreCase); 			
		if (!isEfDesignTime)
		{
			var retryPolicy = Policy
				  .Handle<BrokerUnreachableException>()
				  .Or<SocketException>()
				  .WaitAndRetryAsync(
				  	retryCount: 5,
				  	retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)),
				  	(exception, timespan, retryCount) =>
				  	{
				  		Console.WriteLine($"Retry attempt {retryCount} failed. Retrying in {timespan.Seconds} seconds...");
				  	});

			await retryPolicy.ExecuteAsync(async () =>
			{
				var endpoint = config.GetConnectionString("messaging")
					?? throw new InvalidOperationException("messaging connection string no found");

				var factory = new ConnectionFactory
				{
					Uri = new Uri(endpoint)
				};
				await using var connection = await factory.CreateConnectionAsync();
			});
		}

		builder.Services.AddOpenTelemetry().WithTracing(traveProviderBuilder =>
		{
			traveProviderBuilder.SetResourceBuilder(ResourceBuilder.CreateDefault()
				.AddService(builder.Environment.ApplicationName))
				.AddSource("Wolverine");
		});

		builder.UseWolverine(opts =>
		{
			opts.UseRabbitMqUsingNamedConnection("messaging")
				.AutoProvision()
				.UseConventionalRouting(x =>
				{
					x.QueueNameForListener(t => $"{t.FullName}.{builder.Environment.ApplicationName}");
				});

			configureMessaging(opts);
		});
	}
}
