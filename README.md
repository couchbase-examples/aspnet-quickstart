# Quickstart in Couchbase with C# and ASP .NET
#### Build a REST API with Couchbase's C# SDK 3 and ASP .NET

> This repo is designed to teach you how to connect to a Couchbase cluster to create, read, update, and delete documents and how to write simple parametrized N1QL queries.

<!-- [![Try it now!](https://da-demo-images.s3.amazonaws.com/runItNow_outline.png?couchbase-example=aspnet-quickstart-repo&source=github)](https://gitpod.io/#https://github.com/couchbase-examples/aspnet-quickstart) -->

Full documentation can be found on the [Couchbase Developer Portal](https://developer.couchbase.com/tutorial-quickstart-csharp-aspnet/).
## Prerequisites
To run this prebuilt project, you will need:

- Couchbase 7 Installed
- [.NET SDK v5+](https://dotnet.microsoft.com/download/dotnet/5.0) installed 
- Code Editor installed (Visual Studio Professional, Visual Studio for Mac, or Visual Studio Code)

### Install Dependencies 

```sh
cd src/Org.Quickstart.API
dotnet restore
```
> Note: Nuget packages auto restore when building the project in Visual Studio Professional 2019 and Visual Studio for Mac

#### DependencyInjection Nuget package

The Couchbase SDK for .NET includes a nuget package called `Couchbase.Extensions.DependencyInjection` which is designed for environments like ASP.NET that takes in a configuration to connect to Couchbase and automatically registers interfaces that you can use in your code to perform full `CRUD (create, read, update, delete)` operations and queries against the database. 

### Completed version of the ProfileController

We provide a completed version of the Controller that this guide will walk you through in the Controller folder called `ProfileController1.cs`.  This version is used for our integration tests and you can use it to check your work that you do on the ProfileController.cs file.

### Database Server Configuration

All configuration for communication with the database is stored in the appsettings.Development.json file.  This includes the connection string, username, password, bucket name, colleciton name, and scope name.  The default username is assumed to be `Administrator` and the default password is assumed to be `password`.  If these are different in your environment you will need to change them before running the application.

## Running The Application

*Couchbase 7 must be installed and running on localhost (http://127.0.0.1:8091) prior to running hte the ASP.NET app.  

At this point the application is ready and you can run it:

```sh
dotnet run 
```
You can launch your browser and go to the [Swagger start page](https://localhost:5001/swagger/index.html).

## Running The Tests

To run the standard integration tests, use the following commands:

```sh
cd ../Org.Quickstart.IntegrationTests/
dotnet restore 
dotnet test
```

## Project Setup Notes

This project was based on the standard ASP.NET Template project and the default weather controller was removed.  The HealthCheckController is provided as a santity check and is used in our unit tests.  

A fully list of nuget packages are referenced below:
```xml
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.1.4" />
    <PackageReference Include="Swashbuckle.AspNetCore.Annotations" Version="6.1.4" />
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.2" />
    <PackageReference Include="Couchbase.Extensions.DependencyInjection" Version="3.1.5" />
```

## Conclusion

Setting up a basic REST API in ASP.NET with Couchbase is fairly simple, this project when run with Couchbase Server 7 installed creates a bucket in Couchbase, an index for our parameterized [N1QL query](https://docs.couchbase.com/dotnet-sdk/current/howtos/n1ql-queries-with-sdk.html), and showcases basic CRUD operations needed in most applications.
