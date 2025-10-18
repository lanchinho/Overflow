using Typesense.Setup;

namespace SearchService;

internal static class SearchServiceDependency
{
	internal static IServiceCollection AddTypesense(this IServiceCollection service, IConfiguration configuration)
	{
		var typesenseUri = configuration["services:typesense:typesense:0"];
		if (string.IsNullOrWhiteSpace(typesenseUri))
			throw new InvalidOperationException("Typesense URI not found in config");

		var typesenseApiKey = configuration["typesense-api-key"];
		if (string.IsNullOrWhiteSpace(typesenseApiKey))
			throw new InvalidOperationException("Typesense API key not found in config");

		var uri = new Uri(typesenseUri);
		return service.AddTypesenseClient(config =>
		{
			config.ApiKey = typesenseApiKey;
			config.Nodes =
			[
				new (uri.Host, uri.Port.ToString(), uri.Scheme)
			];
		});
	}
}
