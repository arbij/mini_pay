function create_request_sender(
	port
){
	return async function(
		body
	){
		let
		response
		=
		await
		(
			await
			fetch(
				'http://localhost:'
				+
				port,
				
				{
					method:
					'POST',
					
					headers:
					{
						'Content-Type':
						'application/json'
					},
					
					body:
					JSON
					.stringify(
						body
					)
				}
			)
		)
		.text()
		
		try{
			response
			=
			JSON
			.parse(
				response
			)
		}
		catch{}
		
		return response
	}
}

if(
	typeof window === 'undefined'
)
module.exports=
create_request_sender