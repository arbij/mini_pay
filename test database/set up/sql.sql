create database mini_pay_test
go

use mini_pay_test
go

create table payment_providers(
	id int identity(1,1) primary key,
	name nvarchar(30) unique not null,
	currency nvarchar(20) not null,
	url nvarchar(100) not null default 'http://localhost:5001',
	enabled bit not null default 1,
	description nvarchar(100) not null default ''
)

insert into
payment_providers(
	name,
	currency
)
values
	(
		'paypal',
		'EUR'
	),
	(
		'stripe',
		'USD'
	)

select * from payment_providers