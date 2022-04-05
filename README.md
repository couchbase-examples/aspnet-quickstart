# Quickstart in Couchbase with C# and ASP .NET
#### Build a REST API with Couchbase's C# SDK 3 and ASP .NET

> This repo is designed to teach you how to connect to a Couchbase cluster to create, read, update, and delete documents and how to write simple parametrized N1QL queries.

<!-- [![Try it now!](https://da-demo-images.s3.amazonaws.com/runItNow_outline.png?couchbase-example=aspnet-quickstart-repo&source=github)](https://gitpod.io/#https://github.com/couchbase-examples/aspnet-quickstart) -->

Full documentation can be found on the [Couchbase Developer Portal](https://developer.couchbase.com/tutorial-quickstart-csharp-aspnet/).
## Prerequisites
To run this prebuilt project, you will need:

- Follow [Couchbase Installation Options](/tutorial-couchbase-installation-options) for installing the lastest Couchbase Database Server Instance
- [.NET SDK v5](https://dotnet.microsoft.com/download/dotnet/5.0) installed
- Code Editor installed (Visual Studio Professional, Visual Studio for Mac, or Visual Studio Code)

### Install Dependencies 

```sh
cd src/Org.Quickstart.API
dotnet restore
```
> Note: Nuget packages auto restore when building the project in Visual Studio Professional 2019 and Visual Studio for Mac

#### DependencyInjection Nuget package

The Couchbase SDK for .NET includes a nuget package called `Couchbase.Extensions.DependencyInjection` which is designed for environments like ASP.NET that takes in a configuration to connect to Couchbase and automatically registers interfaces that you can use in your code to perform full `CRUD (create, read, update, delete)` operations and queries against the database. 

### Database Server Configuration

All configuration for communication with the database is stored in the appsettings.Development.json file.  This includes the connection string, username, password, bucket name, colleciton name, and scope name.  The default username is assumed to be `admin` and the default password is assumed to be `P@$$w0rd12`.  If these are different in your environment you will need to change them before running the application.

### Creating the bucket, username, and password 

With this tutorial, it's required that a database user and bucket be created prior to running the application.  

#### Capella Users 

For Capella users, follow the directions found on the [documentation website](https://docs.couchbase.com/cloud/clusters/data-service/manage-buckets.html#add-bucket) for creating a bucket called `user_profile`.  Next, follow the directions for [Configure Database Credentials](https://docs.couchbase.com/cloud/clusters/manage-database-users.html); name it `admin` with a password of `P@$$w0rd12`.  

Next, open the [appsettings.Development.json](https://github.com/couchbase-examples/aspnet-quickstart/blob/main/src/Org.Quickstart.API/appsettings.Development.json#L13) file.  Locate the ConnectionString property and update it to match your Wide Area Network name found in the [Capella Portal UI Connect tab](https://docs.couchbase.com/cloud/get-started/connect-to-cluster.html#connect-to-your-cluster-using-the-built-in-sdk-examples). Note that Capella uses TLS so the connection string must start with couchbases://.

```json
  "Couchbase": {
    "BucketName": "user_profile",
    "ScopeName": "_default",
    "CollectionName": "profile",
    "ConnectionString": "couchbases://yourassignedhostname.cloud.couchbase.com",
    "Username": "admin",
    "Password": "P@$$w0rd12",
    "IgnoreRemoteCertificateNameMismatch": true,
    "HttpIgnoreRemoteCertificateMismatch": true,
    "KvIgnoreRemoteCertificateNameMismatch":  true
  }
```

Couchbase Capella users that do not follow these directions will get  exception errors and the Swagger portal will return errors when running the APIs.

#### Local Installation and Docker Users

For local installation and docker users, follow the directions found on the [documentation website](https://docs.couchbase.com/server/current/manage/manage-buckets/create-bucket.html) for creating a bucket called `user_profile`.  Next, follow the directions for [Creating a user](https://docs.couchbase.com/server/current/manage/manage-security/manage-users-and-roles.html); name it `admin` with a password of `P@$$w0rd12`.  For this tutorial, make sure it has `Full Admin` rights so that the application can create collections and indexes. 

Next, open the [appsettings.Development.json](https://github.com/couchbase-examples/aspnet-quickstart/blob/main/src/Org.Quickstart.API/appsettings.Development.json#L13) file and validate the configuration information matches your setup. 

> **NOTE:** For docker and local Couchbase installations, Couchbase must be installed and running on localhost (<http://127.0.0.1:8091>) prior to running the the ASP.NET app.

### Running The Application

At this point the application is ready and you can run it:

```shell
dotnet run
```

Once the site is up and running you can launch your browser and go to the [Swagger start page](https://localhost:5001/swagger/index.html) to test the APIs.
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
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.3.0" />
    <PackageReference Include="Swashbuckle.AspNetCore.Annotations" Version="6.3.0" />
     <PackageReference Include="CouchbaseNetClient" Version="3.2.8" />
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
    <PackageReference Include="Couchbase.Extensions.DependencyInjection" Version="3.2.8" />
```

## Conclusion

Setting up a basic REST API in ASP.NET with Couchbase is fairly simple, this project when run with Couchbas that creates a collection, an index for our parameterized [N1QL query](https://docs.couchbase.com/dotnet-sdk/current/howtos/n1ql-queries-with-sdk.html), and showcases basic CRUD operations needed in most applications.
