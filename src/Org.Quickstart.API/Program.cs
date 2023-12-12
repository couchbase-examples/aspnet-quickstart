using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Org.Quickstart.API.Models;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Logging;
using Org.Quickstart.API.Services;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration.GetSection("Couchbase");

// Register the configuration for Couchbase and Dependency Injection Framework
if (builder.Environment.EnvironmentName == "Testing")
{
    var connectionString = Environment.GetEnvironmentVariable("DB_CONN_STR");
    var username = Environment.GetEnvironmentVariable("DB_USERNAME");
    var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
    config["ConnectionString"] = connectionString;
    config["Username"] = username;
    config["Password"] = password;
    
    builder.Services.Configure<CouchbaseConfig>(config);
    builder.Services.AddCouchbase(config);
}
else
{
    builder.Services.Configure<CouchbaseConfig>(config);
    builder.Services.AddCouchbase(config);
}

// ConfigureServices
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "_devAllowSpecificOrigins",
                      policyBuilder =>
                      {
                          policyBuilder.WithOrigins("https://*.gitpod.io",
                                              "https://*.github.com",
                                              "http://localhost:5000",
                                              "https://localhost:5001")
                                 .AllowAnyHeader()
                                 .AllowAnyMethod()
                                 .AllowCredentials();
                      });
});

builder.Services.AddHttpClient();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var description = new StringBuilder()
        .AppendLine("A quickstart API using C# and ASP.NET with Couchbase and travel-sample data.\n\n")
        .AppendLine("We have a visual representation of the API documentation using Swagger which allows you to interact with the API's endpoints directly through the browser. It provides a clear view of the API including endpoints, HTTP methods, request parameters, and response objects.\n\n")
        .AppendLine("Click on an individual endpoint to expand it and see detailed information. This includes the endpoint's description, possible response status codes, and the request parameters it accepts.\n\n")
        .AppendLine("Trying Out the API\n\n")
        .AppendLine("You can try out an API by clicking on the \"Try it out\" button next to the endpoints.\n\n")
        .AppendLine("- Parameters: If an endpoint requires parameters, Swagger UI provides input boxes for you to fill in. This could include path parameters, query strings, headers, or the body of a POST/PUT request.\n\n")
        .AppendLine("- Execution: Once you've inputted all the necessary parameters, you can click the \"Execute\" button to make a live API call. Swagger UI will send the request to the API and display the response directly in the documentation. This includes the response code, response headers, and response body.\n\n")
        .AppendLine("Models\n\n")
        .AppendLine("Swagger documents the structure of request and response bodies using models. These models define the expected data structure using JSON schema and are extremely helpful in understanding what data to send and expect.\n\n")
        .AppendLine("For details on the API, please check the tutorial on the Couchbase Developer Portal: https://developer.couchbase.com/tutorial-quickstart-csharp-aspnet\n\n")
        .ToString();

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "1.0",
        Title = "Quickstart in Couchbase with C# and ASP.NET",
        Description = description
    });
    
    options.EnableAnnotations();
});


// Register the InventoryScopeFactory
builder.Services.AddSingleton<IInventoryScopeService, InventoryScopeService>();

// Build the application
var app = builder.Build();

// Get the application lifetime object
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStarted.Register(() =>
{
    
    // Get the logger
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    // Get the address
    var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault();

    // Log the Swagger URL
    logger.LogInformation("Swagger UI is available at: {Address}/index.html", address);
});

// Configure
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors("_devAllowSpecificOrigins");

    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Couchbase Quickstart API v1"); 
        c.RoutePrefix = string.Empty;
    });
}

if (app.Environment.EnvironmentName == "Testing")
{
    app.UseCors("_devAllowSpecificOrigins");
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

// required for integration testing from asp.net
// https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0
public abstract partial class Program { }
