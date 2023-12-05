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
using Swashbuckle.AspNetCore.Annotations;

namespace Org.Quickstart.API.Controllers;

[ApiController]
[Route("/api/v1/airport")]
public class AirportController: Controller
{
     private readonly ILogger _logger;
     private readonly IScope _inventoryScope;
     
     public AirportController(ILogger<AirportController> logger, IScope inventoryScope)
     {
            _logger = logger;
            _inventoryScope = inventoryScope;
     }
     
     [HttpGet]
     [Route("/api/v1/airport/list")]
     [SwaggerOperation(Summary = "Get list of Airports. Optionally, you can filter the list by Country.\n\nThis provides an example of using a SQL++ query in Couchbase to fetch a list of documents matching the specified criteria.")]
     [SwaggerResponse(200, "List of airports")]
     [SwaggerResponse(500, "Unexpected Error")]
     public async Task<IActionResult> List(string country, int? limit, int? offset)
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
			   query = $@"SELECT airport.airportname,
                          airport.city,
                          airport.country,
                          airport.faa,
                          airport.geo,
                          airport.icao,
                          airport.tz
                        FROM airport AS airport
                        WHERE lower(airport.country) = $country
                        ORDER BY airport.airportname
                        LIMIT $limit
                        OFFSET $offset";
                    
			   queryParameters.Parameter("country", country.ToLower());
		   }
		   else
		   {
			   query = $@"SELECT airport.airportname,
                              airport.city,
                              airport.country,
                              airport.faa,
                              airport.geo,
                              airport.icao,
                              airport.tz
                            FROM airport AS airport
                            ORDER BY airport.airportname
                            LIMIT $limit
                            OFFSET $offset";
		   }

		   var results = await _inventoryScope.QueryAsync<Airport>(query, queryParameters);
		   var items = await results.Rows.ToListAsync();

		   return items.Count == 0 ? NotFound() : Ok(items);
	   }
	   catch (Exception ex)
	   {
		   _logger.LogError(ex.Message);
		   return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
	   }
     }
     
       [HttpGet]
       [Route("/api/v1/airport/direct-connections")]
       [SwaggerOperation(Summary = "Get Direct Connections from specified Airport.\n\nThis provides an example of using a SQL++ query in Couchbase to fetch a list of documents matching the specified criteria.")]
       [SwaggerResponse(200, "List of direct connections")]
       [SwaggerResponse(500, "Unexpected Error")]
     public async Task<IActionResult> ToAirport(string airport, int? limit, int? offset)
     {
	     try
	     {
				 //setup parameters
			     var queryParameters = new Couchbase.Query.QueryOptions();
			     queryParameters.Parameter("airport", airport.ToLower());
			     queryParameters.Parameter("limit", limit ?? 10);
			     queryParameters.Parameter("offset", offset ?? 0);
                
			     const string query = $@"SELECT DISTINCT route.destinationairport
                 FROM airport AS airport
                 JOIN route AS route
                 ON route.sourceairport = airport.faa
                 WHERE lower(airport.faa) = $airport AND route.stops = 0
                 ORDER BY route.destinationairport
                 LIMIT $limit
                 OFFSET $offset";

			     var results = await _inventoryScope.QueryAsync<DestinationAirport>(query, queryParameters);
			     var items = await results.Rows.ToListAsync();

			     return items.Count == 0 ? NotFound() : Ok(items);
	     }
	     
	     catch (Exception ex)
	     {
		     _logger.LogError(ex.Message);
		     return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
	     }
     }
     
     [HttpGet("{id}")]
     [SwaggerOperation(Description = "Get Airport with specified ID.\n\nThis provides an example of using Key Value operations in Couchbase to get a document with specified ID.")]
     [SwaggerResponse(200, "Found Airport")]
     [SwaggerResponse(404, "Airport ID not found")]
     [SwaggerResponse(500, "Unexpected Error")]
     public async Task<IActionResult> GetById([FromRoute] string id)
     {
	     try
	     {
		     //get the collection
		     var collection = await _inventoryScope.CollectionAsync("airport");

		     //get the document from the bucket using the id
		     var result = await collection.GetAsync(id);

		     //validate we have a document
		     var resultAirport = result.ContentAs<Airport>();
		     if (resultAirport != null)
		     {
			     return Ok(resultAirport);
		     }
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
	     return NotFound();
     }

     [HttpPost("{id}")]
     [SwaggerOperation(Description = "Create Airport with specified ID.\n\nThis provides an example of using Key Value operations in Couchbase to create a new document with a specified ID.")]
     [SwaggerResponse(201, "Created")]
     [SwaggerResponse(409, "Airport already exists")]
     [SwaggerResponse(500, "Unexpected Error")]
     public async Task<IActionResult> Post(string id, [FromBody] AirportCreateRequestCommand request)
     {
	     try
	     {
		     //get the collection
			     var collection = await _inventoryScope.CollectionAsync("airport");

			     //get airport from request
			     var airport = request.GetAirport();
            
			     // Attempt to insert the document
			     await collection.InsertAsync(id, airport);
			     return Created($"/api/v1/airport/{id}", airport);
	     }
	     catch (DocumentExistsException)
	     {
		     // If a document with the same ID already exists, an exception will be thrown
		     return Conflict($"A document with the ID '{id}' already exists.");
	     }
	     catch (Exception ex)
	     {
		     _logger.LogError(ex.Message);
		     return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
	     }
     }

     [HttpPut("{id}")]
     [SwaggerOperation(Description = "Update Airport with specified ID.\n\nThis provides an example of using Key Value operations in Couchbase to upsert a document with specified ID.")]
     [SwaggerResponse(200, "Airport Updated")]
     [SwaggerResponse(404, "Airport ID not found")]
     [SwaggerResponse(500, "Unexpected Error")]
     public async Task<IActionResult> Update(string id, [FromBody] AirportCreateRequestCommand request)
     {
	     try
	     { 
				 //get the collection
			     var collection = await _inventoryScope.CollectionAsync("airport");

			     //get current airport from the database and update it
			     if (await collection.GetAsync(id) is { } result)
			     {
				     result.ContentAs<Airport>();
				     await collection.ReplaceAsync(id, request.GetAirport());
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
		     _logger.LogError(ex.Message);
		     return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
	     }

	     return NotFound();
     }
     
     [HttpDelete("{id}")]
     [SwaggerOperation(Description = "Delete Airport with specified ID.\n\nThis provides an example of using Key Value operations in Couchbase to delete a document with specified ID.")]
     [SwaggerResponse(204, "Airport Deleted")]
     [SwaggerResponse(404, "Airport ID not found")]
     [SwaggerResponse(500, "Unexpected Error")]
     public async Task<IActionResult> Delete([FromRoute] string id)
     {
	     try
	     {
		         //get the collection
			     var collection = await _inventoryScope.CollectionAsync("airport");

			     //get the document from the bucket using the id
			     var result = await collection.GetAsync(id);

			     //validate we have a document
			     var resultAirport = result.ContentAs<Airport>();
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
		     _logger.LogError(ex.Message);
		     return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
	     }
	     
	     return NotFound();
     }

}