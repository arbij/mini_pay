#pragma warning disable 8618, 8981, 8604, 8602

#:sdk Microsoft.NET.Sdk.Web

#:package Microsoft.EntityFrameworkCore.SqlServer@*
#:package Newtonsoft.Json@*

#:property PublishAot=false

using static System.Text.Json.JsonSerializer;
using Microsoft.EntityFrameworkCore;

var http_client = new HttpClient();

var builder = WebApplication.CreateBuilder();

builder.Services.AddCors();

var server = builder.Build();

server.UseCors(
	policy=>
	policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
);

bool testing
=
args.Length == 1
&&
args[0] == "test";

server.Map(
	"/",
	
	async(
		System.Text.Json.JsonElement body
	)=>{
		using
		var
		database
		=
		new
		database(
			testing
		);
		
		var
		payment_providers
		=
		database
		.payment_providers;
		
		switch(
			body
			.GetProperty(
				"name"
			)
			.GetString()
		){
			case "get all":
				return
				(object)
				await
				payment_providers
				.ToListAsync();
			
			case "get single":
				return
				await
				payment_providers
				.Where(
					provider
					=>
					provider.name
					==
					body
					.GetProperty(
						"provider_name"
					)
					.GetString()
				)
				.SingleOrDefaultAsync();
			
			case "add":
				var new_provider
				=
				Deserialize<
					payment_provider
				>(
					body
					.GetProperty(
						"provider"
					)
					.GetRawText()
				);
				
				if(
					await
					payment_providers
					.Where(
						provider
						=>
						provider
						.name
						==
						new_provider
						.name
					)
					.AnyAsync()
				){
					return "This provider already exists!";
				}
				
				payment_providers
				.Add(
					new_provider
				);
				
				await
				database
				.SaveChangesAsync();
				
				return "success!";
			
			case "remove":
				var provider
				=
				await
				payment_providers
				.Where(
					provider
					=>
					provider
					.name
					==
					body
					.GetProperty(
						"provider_name"
					)
					.GetString()
				)
				.SingleOrDefaultAsync();
				
				if(
					provider is null
				)
				return "This provider does not exist!";
				
				payment_providers
				.Remove(
					provider
				);
				
				await
				database
				.SaveChangesAsync();
				
				return "success!";
				
			case "update":
				provider
				=
				await
				payment_providers
				.Where(
					provider
					=>
					provider.name
					==
					body
					.GetProperty(
						"provider_name"
					)
					.GetString()
				)
				.SingleOrDefaultAsync();
				
				if(
					provider is null
				){
					return "This provider does not exist!";
				}
				
				Newtonsoft.Json.JsonConvert.PopulateObject(
					body
					.GetProperty(
						"new_data"
					)
					.GetRawText(),
					
					provider
				);
				
				await
				database
				.SaveChangesAsync();
				
				return "success!";
			
			case "pay":
				provider
				=
				await
				payment_providers
				.Where(
					provider
					=>
					provider
					.name
					==
					body
					.GetProperty(
						"provider_name"
					)
					.GetString()
				)
				.SingleOrDefaultAsync();
				
				if(
					provider is null
				)
				return "Provider does not exist!";
				
				if(
					!provider.enabled
				)
				return "currently disabled!";
				
				var mock_url
				=
				"http://localhost:5001";
				
				if(
					!
					new[]{
						mock_url,
						mock_url+"/"
					}
					.Contains(
						provider.url
					)
				)
				return "currently unavailable!";
				
				var referenceId="ORDER-";
				
				var iteration=0;
				
				while(
					iteration<5
				){
					referenceId
					+=
					Random.Shared.Next(0, 10);
					
					++iteration;
				}
				
				return
				await
				(
					await
					http_client
					.PostAsync(
						provider
						.url,
						
						new StringContent(
							Serialize(
								new{
									amount
									=
									body
									.GetProperty(
										"amount"
									)
									.GetDecimal(),
									
									currency
									=
									provider.currency,
									
									description
									=
									body
									.GetProperty(
										"description"
									)
									.GetString(),
									
									referenceId
								}
							),
							System.Text.Encoding.UTF8,
							"application/json"
						)
					)
				)
				.Content
				.ReadAsStringAsync();
		}
		
		return "bad request";
	}
);

if(
	testing
)
server.Run("http://localhost:5002");
else
server.Run(); //default port is 5000

class payment_provider{
	public int id {get; private set;}
	public string name {get;set;}
	public string currency {get;set;}
	public string url {get;set;}= "http://localhost:5001";
	public bool enabled {get;set;}= true;
	public string description {get;set;}= "";
}

class database: DbContext{
	public DbSet<payment_provider> payment_providers {get;set;}
	
	private string database_name= "mini_pay";
	
	public database(
		bool test
	){
		if(
			test
		)
		database_name += "_test";
	}
	
	protected override void OnConfiguring(
		DbContextOptionsBuilder optionsBuilder
	){
		optionsBuilder.UseSqlServer(
			$"Server=localhost;Database={database_name};Trusted_Connection=True;TrustServerCertificate=True;"
		);
	}
}