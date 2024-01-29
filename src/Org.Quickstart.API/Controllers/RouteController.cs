using System;
using System.Threading.Tasks;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Org.Quickstart.API.Models;
using Org.Quickstart.API.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Org.Quickstart.API.Controllers;

[ApiController]
[Route("/api/v1/route")]
public class RouteController: Controller
{
    private readonly ILogger _logger;
    private readonly IScope _inventoryScope;
    
    private const string CollectionName = "route";

    public RouteController(ILogger<RouteController> logger, IInventoryScopeService inventoryScopeService)
    {
	    _logger = logger;
	    _inventoryScope = inventoryScopeService.GetInventoryScope();
    }
    
    [HttpGet("{id}")]
    [SwaggerOperation(Description = "Get Route with specified ID.\n\nThis provides an example of using Key Value operations in Couchbase to get a document with specified ID. \n\n Code: [`Controllers/RouteController`](https://github.com/couchbase-examples/aspnet-quickstart/blob/main/src/Org.Quickstart.API/Controllers/RouteController.cs) \n Class: `RouteController` \n Method: `GetById`")]
    [SwaggerResponse(200, "Found Route")]
    [SwaggerResponse(404, "Route ID not found")]
    [SwaggerResponse(500, "Unexpected Error")]
	public async Task<IActionResult> GetById([FromRoute(Name = "id"), SwaggerParameter("Route ID like route_10000", Required = true)] string id)
    {
	     try
	     {
		         //get the collection
			     var collection = await _inventoryScope.CollectionAsync(CollectionName);

			     //get the document from the bucket using the id
			     var result = await collection.GetAsync(id);

			     //validate we have a document
			     var resultRoute = result.ContentAs<Route>();
			     if (resultRoute != null)
			     {
				     return Ok(resultRoute);
			     }
	     }
	     catch (DocumentNotFoundException)
	     {
		     Results.NotFound();
	     }
	     catch (Exception ex)
	     {
		     _logger.LogError("An error occurred: {Message}", ex.Message);
		     return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
	     }

	     return NotFound();
     }
     
     [HttpPost("{id}")]
     [SwaggerOperation(Description = "Create Route with specified ID.\n\nThis provides an example of using Key Value operations in Couchbase to create a new document with a specified ID. \n\n Code: [`Controllers/RouteController`](https://github.com/couchbase-examples/aspnet-quickstart/blob/main/src/Org.Quickstart.API/Controllers/RouteController.cs) \n Class: `RouteController` \n Method: `Post`")]
     [SwaggerResponse(201, "Created")]
     [SwaggerResponse(409, "Route already exists")]
     [SwaggerResponse(500, "Unexpected Error")]
     public async Task<IActionResult> Post([FromRoute(Name = "id"), SwaggerParameter("Route ID like route_10000", Required = true)] string id, 
	     [FromBody, SwaggerRequestBody("The route details to create", Required = true)] RouteCreateRequestCommand request)
     {
	     try
	     {
		         //get the collection
			     var collection = await _inventoryScope.CollectionAsync(CollectionName);

			     //get airline from request
			     var route = request.GetRoute();
                
			     // Attempt to insert the document
			     await collection.InsertAsync(id, route);
			     return Created($"/api/v1/route/{id}", route);
		  
	     }
	     catch (DocumentExistsException)
	     {
		     // If a document with the same ID already exists, an exception will be thrown
		     return Conflict($"A document with the ID '{id}' already exists.");
	     }
	     catch (Exception ex)
	     {
		     _logger.LogError("An error occurred: {Message}", ex.Message);
		     return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
	     }
     }
     
     [HttpPut("{id}")]
     [SwaggerOperation(Description = "Update Route with specified ID.\n\nThis provides an example of using Key Value operations in Couchbase to upsert a document with specified ID. \n\n Code: [`Controllers/RouteController`](https://github.com/couchbase-examples/aspnet-quickstart/blob/main/src/Org.Quickstart.API/Controllers/RouteController.cs) \n Class: `RouteController` \n Method: `Update`")]
     [SwaggerResponse(200, "Route Updated")]
     [SwaggerResponse(404, "Route ID not found")]
     [SwaggerResponse(500, "Unexpected Error")]
     public async Task<IActionResult> Update([FromRoute(Name = "id"), SwaggerParameter("Route ID like route_10000", Required = true)] string id, 
	     [FromBody, SwaggerRequestBody("The route details to update", Required = true)] RouteCreateRequestCommand request)
     {
	     try
	     { 
		     //get the collection
		     var collection = await _inventoryScope.CollectionAsync(CollectionName);

		     //get current airport from the database and update it
		     if (await collection.GetAsync(id) is { } result)
		     {
			     result.ContentAs<Route>();
			     await collection.ReplaceAsync(id, request.GetRoute());
			     return Ok(request);
		     }
		     else
		     {
			     return NotFound();
		     }
		     
	     }
	     catch (DocumentNotFoundException)
	     {
		     Results.NotFound();
	     }
	     catch (Exception ex)
	     {
		     _logger.LogError("An error occurred: {Message}", ex.Message);
		     return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
	     }

	     return NotFound();
     }
     
     [HttpDelete("{id}")]
     [SwaggerOperation(Description = "Delete Route with specified ID.\n\nThis provides an example of using Key Value operations in Couchbase to delete a document with specified ID. \n\n Code: [`Controllers/RouteController`](https://github.com/couchbase-examples/aspnet-quickstart/blob/main/src/Org.Quickstart.API/Controllers/RouteController.cs) \n Class: `RouteController` \n Method: `Delete`")]
     [SwaggerResponse(204, "Route Deleted")]
     [SwaggerResponse(404, "Route ID not found")]
     [SwaggerResponse(500, "Unexpected Error")]
     public async Task<IActionResult> Delete([FromRoute(Name = "id"), SwaggerParameter("Route ID like route_10000", Required = true)] string id)
     {
	     try
	     {
		     //get the collection
		     var collection = await _inventoryScope.CollectionAsync(CollectionName);

		     //get the document from the bucket using the id
		     var result = await collection.GetAsync(id);

		     //validate we have a document
		     var resultRoute = result.ContentAs<Airline>();
		     if (resultRoute != null)
		     {
			     await collection.RemoveAsync(id);
			     return Ok(id);
		     }
		     else
		     {
			     return NotFound();
		     }
	     }
	     catch (DocumentNotFoundException)
	     {
		     Results.NotFound();
	     }
	     catch (Exception ex)
	     {
		     _logger.LogError("An error occurred: {Message}", ex.Message);
		     return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
	     }
	     
	     return NotFound();
     }
}