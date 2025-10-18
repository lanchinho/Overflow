using Typesense;

namespace SearchService.Data;

public static class SearchInitializer
{
	public static async Task EnsureIndexExistsAsynxc(ITypesenseClient client)
	{
		const string schemaName = "questions";
		try
		{
			await client.RetrieveCollection(schemaName);
			Console.WriteLine($"Collectio`n {schemaName} has been created already.");
		}
		catch (TypesenseApiNotFoundException)
		{

			Console.WriteLine($"Collection {schemaName} has not been created yet.");
			var schema = new Schema(schemaName,
			[
				new("id", FieldType.String),
				new("title", FieldType.String),
				new("content", FieldType.String),
				new("tags", FieldType.StringArray),
				new("createdAt", FieldType.Int64),
				new("answerCount", FieldType.Int32),
				new("hasAcceptedAnswer", FieldType.Bool)
			])
			{
				DefaultSortingField = "createdAt"
			};

			await client.CreateCollection(schema);
			Console.WriteLine($"Collection {schemaName} created successfully");
		}
	}
}
