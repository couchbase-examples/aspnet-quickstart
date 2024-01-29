using System;
using System.Linq;
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
[Route("/api/v1/airline")]
public class AirlineController: Controller
{
    private readonly ILogger _logger;
    private readonly IScope _inventoryScope;
    
    private const string CollectionName = "airline";
    
    public AirlineController(ILogger<AirlineController> logger, IInventoryScopeService inventoryScopeService)
    {
        _logger = logger;
        _inventoryScope = inventoryScopeService.GetInventoryScope();
    }

     [HttpGet]
     [Route("/api/v1/airline/list")]
     [SwaggerOperation(Description = "Get list of Airlines. Optionally, you can filter the list by Country.\n\nThis provides an example of using SQL++ query in Couchbase to fetch a list of documents matching the specified criteria. \n\n Code: [`Controllers/AirlineController`](https://github.com/couchbase-examples/aspnet-quickstart/blob/main/src/Org.Quickstart.API/Controllers/AirlineController.cs) \n Class: `AirlineController` \n Method: `List`")]
     [SwaggerResponse(200, "List of airlines")]
     [SwaggerResponse(500, "Unexpected Error")]
     public async Task<IActionResult> List([FromQuery(Name = "country"), SwaggerParameter("Country (Example: France, United Kingdom, United States)", Required = false)] string country, 
	     [FromQuery(Name = "limit"), SwaggerParameter("Number of airlines to return (page size). Default value: 10.", Required = false)] int? limit, 
	     [FromQuery(Name = "offset"), SwaggerParameter("Number of airlines to skip (for pagination). Default value: 0.", Required = false)] int? offset) 
     {
	     try
	     {
		         // setup parameters
			     var queryParameters = new Couchbase.Query.QueryOptions();
			     queryParameters.Parameter("limit", limit ?? 10);
			     queryParameters.Parameter("offset", offset ?? 0);

			     string query;
			     if (!string.IsNullOrEmpty(country))
			     {
				     query = $@"SELECT airline.callsign,
                            airline.country,
                            airline.iata,
                            airline.icao,
                            airline.name
                            FROM airline AS airline
                            WHERE lower(airline.country) = $country
                            ORDER BY airline.name
                            LIMIT $limit
                            OFFSET $offset";
				     queryParameters.Parameter("country", country.ToLower());
			     }
			     else
			     {
				     query = $@"SELECT airline.callsign,
                            airline.country,
                            airline.iata,
                            airline.icao,
                            airline.name
                            FROM airline AS airline
                            ORDER BY airline.name
                            LIMIT $limit
                            OFFSET $offset";
			     }

			     var results = await _inventoryScope.QueryAsync<Airline>(query, queryParameters);
			     var items = await results.Rows.ToListAsync();

			     return items.Count == 0 ? NotFound() : Ok(items);
		     
	     }
	     catch (Exception ex)
	     {
		     _logger.LogError("An error occurred: {Message}", ex.Message);
		     return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
	     }
     }
     
     [HttpGet]
     [Route("/api/v1/airline/to-airport")]
     [SwaggerOperation(Description = "Get Airlines flying to specified destination Airport.\n\nThis provides an example of using SQL++ query in Couchbase to fetch a list of documents matching the specified criteria. \n\n Code: [`Controllers/AirlineController`](https://github.com/couchbase-examples/aspnet-quickstart/blob/main/src/Org.Quickstart.API/Controllers/AirlineController.cs) \n Class: `AirlineController` \n Method: `ToAirport`")]
     [SwaggerResponse(200, "List of airlines")]
     [SwaggerResponse(500, "Unexpected Error")]
     public async Task<IActionResult> ToAirport([FromQuery(Name = "airport"), SwaggerParameter("Destination airport (Example: SFO, JFK, LAX)", Required = true)] string airport, 
	     [FromQuery(Name = "limit"), SwaggerParameter("Number of airlines to return (page size). Default value: 10.", Required = false)] int? limit, 
	     [FromQuery(Name = "offset"), SwaggerParameter("Number of airlines to skip (for pagination). Default value: 0.", Required = false)] int? offset)
     {
	     try
	     {
		         //setup parameters
			     var queryParameters = new Couchbase.Query.QueryOptions();
			     queryParameters.Parameter("airport", airport.ToLower());
			     queryParameters.Parameter("limit", limit ?? 10);
			     queryParameters.Parameter("offset", offset ?? 0);
                
			     const string query = $@"SELECT air.callsign,
                                   air.country,
                                   air.iata,
                                   air.icao,
                                   air.name
                          FROM (
                            SELECT DISTINCT META(airline).id AS airlineId
                            FROM route AS route
                            JOIN airline AS airline
                            ON route.airlineid = META(airline).id
                            WHERE lower(route.destinationairport) = $airport
                          ) AS SUBQUERY
                          JOIN airline AS air
                          ON META(air).id = SUBQUERY.airlineId
                          LIMIT $limit
                          OFFSET $offset";

			     var results = await _inventoryScope.QueryAsync<Airline>(query, queryParameters);
			     var items = await results.Rows.ToListAsync();

			     return items.Count == 0 ? NotFound() : Ok(items);
	     }
	     
	     catch (Exception ex)
	     {
		     _logger.LogError("An error occurred: {Message}", ex.Message);
		     return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
	     }
     }
     
     [HttpGet("{id}")]
     [SwaggerOperation(Description = "Get Airline with specified ID.\n\nThis provides an example of using Key Value operations in Couchbase to get a document with specified ID.\n\n Code: [`Controllers/AirlineController`](https://github.com/couchbase-examples/aspnet-quickstart/blob/main/src/Org.Quickstart.API/Controllers/AirlineController.cs) \n Class: `AirlineController` \n Method: `GetById` ")]
     [SwaggerResponse(200, "Found Airline")]
     [SwaggerResponse(404, "Airline ID not found")]
     [SwaggerResponse(500, "Unexpected Error")]
     public async Task<IActionResult> GetById([FromRoute(Name = "id"), SwaggerParameter("Airline ID like airline_10", Required = true)] string id)     {
	     try
	     {
		         //get the collection
			     var collection = await _inventoryScope.CollectionAsync(CollectionName);

			     //get the document from the bucket using the id
			     var result = await collection.GetAsync(id);

			     //validate we have a document
			     var resultAirlines = result.ContentAs<Airline>();
			     if (resultAirlines != null)
			     {
				     return Ok(resultAirlines);
			     }
	     }
	     catch (DocumentNotFoundException)
	     {
		     return NotFound();
	     }
	     catch (Exception ex)
	     {
		     _logger.LogError("An error occurred: {Message}", ex.Message);
		     return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
	     }

	     return NotFound();
     }
     
     [HttpPost("{id}")]
     [SwaggerOperation(Description = "Create Airport with specified ID.\n\nThis provides an example of using Key Value operations in Couchbase to create a new document with a specified ID. \n\n Code: [`Controllers/AirlineController`](https://github.com/couchbase-examples/aspnet-quickstart/blob/main/src/Org.Quickstart.API/Controllers/AirlineController.cs) \n Class: `AirlineController` \n Method: `Post`")]
     [SwaggerResponse(201, "Created")]
     [SwaggerResponse(409, "Airline already exists")]
     [SwaggerResponse(500, "Unexpected Error")]
     public async Task<IActionResult> Post([FromRoute(Name = "id"), SwaggerParameter("Airline ID like airline_10", Required = true)] string id, AirlineCreateRequestCommand request)
     {
	     try
	     {
		         //get the collection
			     var collection = await _inventoryScope.CollectionAsync(CollectionName);

			     //get airline from request
			     var airline = request.GetAirline();
                
			     // Attempt to insert the document
			     await collection.InsertAsync(id, airline);
			     return Created($"/api/v1/airline/{id}", airline);
		  
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
     [SwaggerOperation(Description = "Update Airline with specified ID.\n\nThis provides an example of using Key Value operations in Couchbase to upsert a document with specified ID. \n\n Code: [`Controllers/AirlineController`](https://github.com/couchbase-examples/aspnet-quickstart/blob/main/src/Org.Quickstart.API/Controllers/AirlineController.cs) \n Class: `AirlineController` \n Method: `Update`")]
     [SwaggerResponse(200, "Airline Updated")]
     [SwaggerResponse(404, "Airline ID not found")]
     [SwaggerResponse(500, "Unexpected Error")]
     public async Task<IActionResult> Update([FromRoute(Name = "id"), SwaggerParameter("Airline ID like airline_10", Required = true)] string id, AirlineCreateRequestCommand request)
     {
	     try
	     { 
		     //get the collection
		     var collection = await _inventoryScope.CollectionAsync(CollectionName);

		     //get current airport from the database and update it
		     if (await collection.GetAsync(id) is { } result)
		     {
			     result.ContentAs<Airport>();
			     await collection.ReplaceAsync(id, request.GetAirline());
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
     [SwaggerOperation(Description = "Delete Airline with specified ID.\n\nThis provides an example of using Key Value operations in Couchbase to delete a document with specified ID. \n\n Code: [`Controllers/AirlineController`](https://github.com/couchbase-examples/aspnet-quickstart/blob/main/src/Org.Quickstart.API/Controllers/AirlineController.cs) \n Class: `AirlineController` \n Method: `Delete`")]
     [SwaggerResponse(204, "Airline Deleted")]
     [SwaggerResponse(404, "Airline ID not found")]
     [SwaggerResponse(500, "Unexpected Error")]
     public async Task<IActionResult> Delete([FromRoute(Name = "id"), SwaggerParameter("Airline ID like airline_10", Required = true)] string id)     
     {
	     try
	     {
		     //get the collection
		     var collection = await _inventoryScope.CollectionAsync(CollectionName);

		     //get the document from the bucket using the id
		     var result = await collection.GetAsync(id);

		     //validate we have a document
		     var resultAirport = result.ContentAs<Airline>();
		     if (resultAirport != null)
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