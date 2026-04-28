SQL Server database containing a list of payment providers and related information

Backend written in C# and compiled with .NET, communicates with the database using Entity Framework Core, able to read/add/remove/update providers and their data.

An additional backend has been written for the purpose of mocking communication with payment providers using a pre-agreed upon API.

The frontend has been written in HTML and javascript, communicates with the backend via fetch calls in a single-endpoint POST API. User interface consists of input fields and buttons. Page does not refresh upon submitting.

An additional database with the same schema has been created for the purpose of automated testing. This database is reset to a known default state before each test run.

The same backend is used for both production and testing, with a command line argument specifying which database to connect to.

The test front-end does not have a user interface. All the testing http requests are automatically sent to the backend when the frontend is opened, and the responses are automatically compared to their expected values. The output indicates whether all tests have passed.

To set up the databases (needed only once after installation), run "set up.bat".

To run the application, open "run.bat".

To run automated testing, open "run tests.bat".

SQL Server and .NET 10 SDK (or later) must be installed. The project runs natively on windows, but the batch commands can be adapted for other operating systems.

The project may be improved with a more polished UI and stronger input sanitization.