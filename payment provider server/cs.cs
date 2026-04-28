#pragma warning disable IL2026, IL3050, RDG004

#:sdk Microsoft.NET.Sdk.Web

#:property JsonSerializerIsReflectionEnabledByDefault=true

var server = WebApplication.Create();

server.Map(
	"/",
	
	(
		System.Text.Json.JsonElement
		body
	)=>{
		string status, message;
		
		if(
			Random.Shared.NextDouble() < .7
		){
			status="Success";
			message="Payment processed successfully";
		}
		else{
			status="Failure";
			message="Payment processing failed";
		}
		
		var transactionId="TX";
		
		var iteration=0;
		
		while(
			iteration<8
		){
			transactionId += Random.Shared.Next(0, 10);
			
			++iteration;
		}
		
		return new{
			status,
			message,
			transactionId,
			
			timestamp
			=
			DateTime.UtcNow.ToString(
				"yyyy-MM-ddTHH:mm:ssZ"
			),
			
			referenceId
			=
			body
			.GetProperty(
				"referenceId"
			)
		};
	}
);

server.Run("http://localhost:5001");