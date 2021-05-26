# Quickstart in Couchbase with C# and ASP .NET
#### Build a REST API with Couchbase's C# SDK 3 and ASP .NET

> This repo is designed to teach you how to connect to a Couchbase cluster to create, read, update, and delete documents and how to write simple parametrized N1QL queries.

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

## What We'll Cover

A simple REST API using ASP.NET and the Couchbase SDK version 3.x with the following endpoints:

- **post-profile** – Create a new user profile 
- **getbykey-profile** – Get a specific profile 
- **put-profile** – Update a profile 
- **delete-profile** – Delete a profile 
- **get-profiles** – Get all profiles matching First or Last Name

## Document Structure

We will be setting up a REST API to manage some profile documents. Our profile document will have an auto-generated GUID for its key, first and last name of the user, an email, and hashed password. For this demo we will store all profile information in just one document in a collection named `profile`: 

```json
{
  "pid": "b181551f-071a-4539-96a5-8a3fe8717faf",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@couchbase.com",
  "password": "$2a$10$tZ23pbQ1sCX4BknkDIN6NekNo1p/Xo.Vfsttm.USwWYbLAAspeWsC"
}
```

As we can see, we want our user's password to be encrypted in the database too, we can achieve this simply with bcrypt, a dependency we have installed.

## Review the Code

**`Org.Quickstart.API.Startup`:**

ASP.NET has an interface called `IHostApplicationLifetime` that you can add to your Configure method to help with registration of lifetime events.  The Couchbase SDK provides the `ICouchbaseLifetimeService` interface for handling closing the database connections when the application closes.  

It's best practice to register for the ASP.NET `ApplicationStop` lifetime event and call the `ICouchbaseLifetimeService` Close method so that the database connection and resources are closed and removed gracefully.

The Couchbase .NET SDK will handle all communications to the database cluster, so you shouldn't need to worry about creating a pool of connections.  

The DatabaseService class SetupDatabase method is used when the ASP.NET `ApplicationStarted` lifetime event fires to automatically create the bucket, collection, scope, and index used in this Quickstart and reads this configuration information in from the appsettings.Development.json file.

```csharp

  if (_env.EnvironmentName == "Testing"){
    //setup the database once everything is setup and running integration tests need to make sure database is fully working before running,hence running Synchronously
    appLifetime.ApplicationStarted.Register(() => {
      var db = app.ApplicationServices.GetService<DatabaseService>();
      db.SetupDatabase().RunSynchronously();
    });
  } else {
    //setup the database once everything is setup and running
    appLifetime.ApplicationStarted.Register(async () => {
      var db = app.ApplicationServices.GetService<DatabaseService>();
      await db.SetupDatabase();
    });
  }

  //remove couchbase from memory when ASP.NET closes
  appLifetime.ApplicationStopped.Register(() => {
    app.ApplicationServices
        .GetRequiredService<ICouchbaseLifetimeService>()
        .Close();
  });

```

### POST a Profile

Add the following code to our `Controllers/ProfileController.cs` file in our Org.Quickstart.API project by replacing the text: `/* Endpoints Here */`

```csharp
[HttpPost]
[SwaggerOperation(OperationId = "UserProfile-Post", Summary = "Create a user profile", Description = "Create a user profile from the request")]
[SwaggerResponse(201, "Create a user profile")]
[SwaggerResponse(409, "the email of the user already exists")]
[SwaggerResponse(500, "Returns an internal error")]
public async Task<IActionResult> Post([FromBody] ProfileCreateRequestCommand request)
{
    try
    {
      if (!string.IsNullOrEmpty(request.Email) && !string.IsNullOrEmpty(request.Password))
      {
        var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
        var collection = bucket.Collection(_couchbaseConfig.CollectionName);
        var profile = request.GetProfile();
        profile.Pid = Guid.NewGuid();
        await collection.InsertAsync(profile.Pid.ToString(), profile);

            return Created($"/api/v1/profile1/{profile.Pid}", profile);
      }
      else 
      {
        return UnprocessableEntity();  
      }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
    }
}
```

Let’s break this code down.

First, we check that both an email and password exist in the request.  Next, we get a variable that we can use to get a connection to the bucket which allows us to get a variable to connect to the collection that we want to add our document to.  After this we use a helper method built into the ProfileCreateRequestCommand class that returns a new Profile object:

```csharp
public Profile GetProfile()
{
  return new Profile
  {
    Pid = new Guid(),
    FirstName = this.FirstName,
    LastName = this.LastName,
    Email = this.Email,
    Password = this.Password
  };
}
```
~from Models/ProfileCreateRequestCommand.cs~

The `Pid` that we’re saving into the account object is a unique key.  Rather than saving the password in the account object as plain text, we hash it with [Bcrypt](https://www.nuget.org/packages/BCrypt.Net-Next/) in the setter of the Profile object:

```csharp
private string _password;
public string Password 
{
  get
  {
    return _password;
  }
  set
  {
    _password = BCrypt.Net.BCrypt.HashPassword(value);
  }
}
```
~from Models/Profile.cs~

Our `profile` document is ready to be persisted to the database.  We create an async call to the `collection` using the `InsertAsync` method and then return the document saved and the result all as part of the same object back to the user.  `InsertAsync` is a [basic key-value operation](https://docs.couchbase.com/dotnet-sdk/current/howtos/kv-operations.html).  Key-value operations are a powerful way to work with documents in a Couchbase database.

### GET a Profile by Key

Retrieve a Profile by Profile ID - next add this method call into the ProfileController class.

```csharp
[HttpGet("{id:Guid}", Name = "UserProfile-GetById")]
[SwaggerOperation(OperationId = "UserProfile-GetById", Summary = "Get user profile by Id", Description = "Get a user profile by Id from the request")]
[SwaggerResponse(200, "Returns a report")]
[SwaggerResponse(404, "Report not found")]
[SwaggerResponse(500, "Returns an internal error")]
public async Task<IActionResult> GetById([FromRoute] Guid id)
{
  try
  {
    var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
    var scope = bucket.Scope(_couchbaseConfig.ScopeName);
    var collection = scope.Collection(_couchbaseConfig.CollectionName); 
    var result = await collection.GetAsync(id.ToString());

    return Ok(result.ContentAs<Profile>());

  }
  catch (DocumentNotFoundException)
  {
    return NotFound();
  }
  catch (Exception ex)
  {
    _logger.LogError(ex.Message);
    return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
  }
}
```

We only need the profile ID from the user to retrieve a particular profile document using a basic key-value operation. Since we created the document with a unique key we can use that key to find the document in the scope and collection it is stored in.  If the document wasn't found in the database we return the NotFound method which results in a 404 status code.  We can catch the error if the key value operation fails and return the error message.

### PUT Profile 

Update a Profile by Profile ID

```csharp
[HttpPut]
[SwaggerOperation(OperationId = "UserProfile-Update", Summary = "Update a user profile", Description = "Update a user profile from the request")]
[SwaggerResponse(200, "Update a user profile")]
[SwaggerResponse(404, "user profile not found")]
[SwaggerResponse(500, "Returns an internal error")]
public async Task<IActionResult> Update([FromBody] ProfileUpdateRequestCommand request)
{
    try
    {
        var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
        var collection = bucket.Collection(_couchbaseConfig.CollectionName);
        var result = await collection.GetAsync(request.Pid.ToString());
        var profile = result.ContentAs<Profile>();

        var updateResult = await collection.ReplaceAsync<Profile>(request.Pid.ToString(), request.GetProfile());
        return Ok(request);

    }
    catch (Exception ex)
    {
        _logger.LogError(ex.Message);
        return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
    }
}
```

We first look up the existing document and make sure it exists, if it does not, return a 500 level error code and message: "Cannot update: document not found". 

Then, the entire document gets replaced except for the document key and the `pid` field.  The ProfileUpdateRequestCommand has a helper method that returns a Profile from the request object.  

Finally, we create an async call to the `collection` using the `ReplaceAsync` method and then return the document saved and the result just as we did in the previous endpoint.

### DELETE Profile

Delete Profile by Profile ID 

```csharp
[HttpDelete("{id:Guid}")]
[SwaggerOperation(OperationId = "UserProfile-Delete", Summary = "Delete a profile", Description = "Delete a profile from the request")]
[SwaggerResponse(200, "Delete a profile")]
[SwaggerResponse(404, "profile not found")]
[SwaggerResponse(500, "Returns an internal error")]
public async Task<IActionResult> Delete([FromRoute] Guid id)
{
  try
  {
    var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
    var collection = bucket.Collection(_couchbaseConfig.CollectionName);
    await collection.RemoveAsync(id.ToString());

    return this.Ok();
  }
  catch (Exception ex)
  {
      _logger.LogError(ex.Message);
      return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
  }
}
```

We only need the profile ID from the user to either delete using a basic key-value operation.

### GET Profiles by Searching

[N1QL](https://docs.couchbase.com/dotnet-sdk/current/howtos/n1ql-queries-with-sdk.html) is a powerful query language based on SQL, but designed for structed and flexible JSON documents. We will use a N1QL query to search for profiles with Skip, Limit, and Search options.  

```csharp
[HttpGet]
[Route("/api/v1/profiles")]
[SwaggerOperation(OperationId = "UserProfile-List", Summary = "Search for user profiles", Description = "Get a list of user profiles from the request")]
[SwaggerResponse(200, "Returns the list of user profiles")]
[SwaggerResponse(500, "Returns an internal error")]
public async Task<IActionResult> List([FromQuery] ProfileRequestQuery request)
{
  try
  {
    var cluster = await _clusterProvider.GetClusterAsync();
    var query = $"SELECT p.* FROM  {_couchbaseConfig.BucketName}.{_couchbaseConfig.ScopeName}.{_couchbaseConfig.CollectionName} p WHERE lower(p.firstName) LIKE '%{request.FirstNameSearch.ToLower()}%' LIMIT {request.Limit} OFFSET {request.Skip}";

    var results = await cluster.QueryAsync<Profile>(query);
    var items = await results.Rows.ToListAsync<Profile>();
    if (items.Count == 0)
        return NotFound();

    return Ok(items);
  }
  catch (Exception ex)
  {
    _logger.LogError(ex.Message);
    return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
  }
}
```

This endpoint is different from all of the others because it makes the N1QL query rather than a key-value operation. This means more overhead because the query engine is involved. We did create an [index](https://docs.couchbase.com/server/current/learn/services-and-indexes/indexes/indexing-and-query-perf.html) specific for this query, so it should be performant.

First, the method signature uses the [FromQuery] annotation to destructure the query to get the individual `skip`, `limit`, and `firstNameSearch` values and store them in the ProfileRequestQuery object. 

Then, we build our N1QL query using the parameters we just built.

Finally, we pass that `query` to the `cluster.QueryAsync` method and return the result.

Take notice of the N1QL syntax and how it targets the `bucket`.`scope`.`collection`.

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
