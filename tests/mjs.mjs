import{
	equal,
	deepEqual
}
from 'node:assert/strict';

function print(
	...strings
){
	let print = console.log
	
	for(
		let string of strings
	){
		print(
			string
		)
	}
	print('')
}

async function send_request(
	body
){
	let
	response
	=
	await
	(
		await
		fetch(
			'http://localhost:5002',
			
			{
				method:
				'POST',
				
				headers:
				{
					'Content-Type':
					'application/json'
				},
				
				body:
				JSON.stringify(
					body
				)
			}
		)
	)
	.text()
	
	try{
		response
		=
		JSON.parse(
			response
		)
	}
	catch{}
	
	return response
}

async function wait_for_server(
	port
){
	while(true){
		try{
			await
			fetch(
				'http://localhost:'
				+
				port
			)
			
			return
		}
		catch{}
		
		await new Promise(
			resolve=>
			setTimeout(
				resolve,
				200
			)
		)
	}
}

await wait_for_server(5002) //minimay
await wait_for_server(5001) //payment provider

deepEqual(
	await send_request(
		{
			name: 'get all'
		}
	),
	
	[
		{
			id: 1,
			name: 'paypal',
			currency: 'EUR',
			url: 'http://localhost:5001',
			enabled: true,
			description: ''
		},
		
		{
			id: 2,
			name: 'stripe',
			currency: 'USD',
			url: 'http://localhost:5001',
			enabled: true,
			description: ''
		}
	]
)

equal(
	await send_request(
		{
			name: 'add',
			
			provider
			:
			{
				name: 'paybuddy',
				currency: 'GBP'
			}
		}
	),
	
	'success!'
)

deepEqual(	
	await send_request(
		{
			name: 'get single',
			provider_name: 'paybuddy'
		}
	),
		
	{
		id: 3,
		name: 'paybuddy',
		currency: 'GBP',
		url: 'http://localhost:5001',
		enabled: true,
		description: ''
	}
)

equal(
	await send_request(
		{
			name: 'get single',
			provider_name: 'non-existent provider'
		}
	),
	
	null
)

equal(
	await send_request(
		{
			name: 'add',
			
			provider
			:
			{
				name: 'paybuddy',
				currency: 'GBP'
			}
		}
	),
	
	'This provider already exists!'
)

equal(
	await send_request(
		{
			name: 'remove',
			provider_name:'paypal'
		}
	),
	
	'success!'
)

equal(
	await send_request(
		{
			name: 'remove',
			provider_name:'paypal'
		}
	),
	
	'This provider does not exist!'
)

equal(
	await send_request(
		{
			name: 'update',
			provider_name: 'paypal',
			
			new_data
			:
			{
				enabled: false
			}
		}
	),
	
	'This provider does not exist!'
)

equal(
	await send_request(
		{
			name: 'update',
			provider_name: 'paybuddy',
			
			new_data
			:
			{
				enabled: false
			}
		}
	),
	
	'success!'
)

equal(	
	await send_request(
		{
			name: 'pay',
			provider_name: 'paypal',
			amount: 100.5,
			description: 'test transaction'
		}
	),
	
	'Provider does not exist!'
)

equal(	
	await send_request(
		{
			name: 'pay',
			provider_name: 'paybuddy',
			amount: 100.5,
			description: 'test transaction'
		}
	),
	
	'currently disabled!'
)

let response
= 
await
send_request(
	{
		name: 'pay',
		provider_name: 'stripe',
		amount: 100.5,
		description: 'test transaction'
	}
)

let{
	status,
	message,
	transactionId,
	timestamp,
	referenceId
}
=
response

if(
	!(
		status === 'Success'
		&&
		message === 'Payment processed successfully'
		
		||
		
		status === 'Failure'
		&&
		message === 'Payment processing failed'
	)
	
	||
	
	[
		transactionId,
		timestamp,
		referenceId
	]
	.includes(
		undefined
	)
){
	console.log(
		response
	)
	
	throw new Error('bad payment server response')
}

equal(
	await send_request(
		{
			name: 'update',
			provider_name: 'stripe',
			
			new_data
			:
			{
				url: 'https://stripe.com'
			}
		}
	),
	
	'success!'
)

equal(	
	await send_request(
		{
			name: 'pay',
			provider_name: 'stripe',
			amount: 100.5,
			description: 'test transaction'
		}
	),
	
	'currently unavailable!'
)

equal(
	await send_request(
		{
			name: 'bad request'
		}
	),
	
	'bad request'
)

console.log(
	'all tests passed!'
)