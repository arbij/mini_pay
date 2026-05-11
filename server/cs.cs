#pragma warning disable 8618, 8981, 8604, 8602

#:sdk Microsoft.NET.Sdk.Web

#:package Microsoft.EntityFrameworkCore.SqlServer@*
#:package Newtonsoft.Json@*

#:property PublishAot=false

using static System.Text.Json.JsonSerializer;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

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
args
.Length
==
1

&&

args[
	0
]
==
"test";

server
.MapGet(
	"/",
	
	()=>
	Results
	.File(
		Path
		.Combine(
			server
			.Environment
			.ContentRootPath,
			
			"..",
			
			"download client.html"
		),
		
		"text/html"
	)
);

server
.MapGet(
	"/download",
	
	async()=>{
		var memory_stream = new MemoryStream();
		
		{
			using var archive
			=
			new ZipArchive(
				memory_stream,
				ZipArchiveMode.Create,
				true
			);
			
			foreach(
				var file_name
				in new[]{
					"client.html",
					"send request.js"
				}
			){
				using var writer
				=
				new StreamWriter(
					archive
					.CreateEntry(
						file_name
					)
					.Open()
				);
				
				await
				writer
				.WriteAsync(
					await
					File
					.ReadAllTextAsync(
						"../"
						+
						file_name
					)
				);
			}
		}
		
		memory_stream
		.Seek(
			0,
			SeekOrigin.Begin
		);
		
		return
		Results
		.File(
			memory_stream,
			"application/zip",
			"mini-pay-client.zip"
		);
	}
);

server
.MapPost(
	"/",
	
	async(
		System.Text.Json.JsonElement
		body
		//crashes and returns an empty string if the request does not contain a body
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
		
		var request_name
		=
		body
		.GetProperty(
			"name"
		)
		.GetString();
		//crashes and returns an empty string if name property is missing
		
		if(
			request_name
			==
			"get all"
		)
		return
		(object)
		await
		payment_providers
		.ToListAsync();
		
		if(
			request_name
			==
			"add"
		){
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
				//crashes and returns an empty string if the provider is missing or not deserializable
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
			)
			return "This provider already exists!";
			
			payment_providers
			.Add(
				new_provider
			);
		}
		else{
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
				//crashes and returns an empty string if provider_name is missing
			)
			.SingleOrDefaultAsync();
			
			if(
				provider
				is null
			)
			return "This provider does not exist!";
			
			switch(
				request_name
			){
				case "get single":
					return
					provider;
				
				case "remove":
					payment_providers
					.Remove(
						provider
					);
				break;
					
				case "update":
					Newtonsoft.Json.JsonConvert.PopulateObject(
						body
						.GetProperty(
							"new_data"
						)
						.GetRawText(),
						//crashes if new_data missing or malformed
						
						provider
					);
				break;
				
				case "pay":
					if(
						!
						provider
						.enabled
					)
					return "currently disabled!";
					
					const string mock_url=
					"http://localhost:5001";
					
					switch(
						provider
						.url
					){
						case mock_url:
						case mock_url + "/":
							break;
						
						default:
							return "currently unavailable!";
					}
					
					var referenceId=
					"ORDER-";
					
					var iteration=
					0;
					
					while(
						iteration
						<5
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
										//crashes if amount missing or malformed
										
										currency
										=
										provider
										.currency,
										
										description
										=
										body
										.GetProperty(
											"description"
										)
										.GetString(),
										//crashes if description missing
										
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
				
				//matches when the request body contains a malformed name and a provider_name. Same response as crashes.
				default:
					return "";
			}
		}
		
		await
		database
		.SaveChangesAsync();
		
		return "success!";
	}
);

if(
	testing
)
	server
	.Run(
		"http://localhost:5002"
	);
else
	server
	.Run(); //default port is 5000

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