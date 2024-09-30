using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace TableStorageFunction
{
    public class TableStorageFunction
    {
        // Declare a private TableClient object to interact with Azure Table Storage
        private readonly TableClient _tableClient;
        private readonly ILogger<TableStorageFunction> _logger; // Add a logger field

        public TableStorageFunction(ILogger<TableStorageFunction> logger)
        {
            _logger = logger; // Initialize the logger
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient("Products");
            _tableClient.CreateIfNotExists();
        }

        [Function("AddProductFunction")] // This attribute defines the Azure Function name as "AddProductFunction".
        public async Task<IActionResult> Run(
    // This parameter triggers the function on HTTP requests with the specified authorization level and method (POST).
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            // Log an informational message indicating that a request has been received to process adding a product.
            _logger.LogInformation("AddProductFunction processed a request for a product");

            // Read the body of the HTTP request asynchronously to obtain the product data.
            // This is done by creating a new StreamReader on the request body stream and reading it until the end.
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Deserialize the request body JSON into a Product object using JsonConvert.
            // This converts the JSON representation of the product into a C# Product instance.
            Product product = JsonConvert.DeserializeObject<Product>(requestBody);

            // Check if the product object was successfully deserialized.
            // If deserialization fails (i.e., the product is null), return a BadRequest response with an error message.
            if (product == null)
            {
                return new BadRequestObjectResult("Invalid product data."); // Return an error message indicating invalid data.
            }

            // If the product is valid, proceed to add it to the Azure Table Storage.
            // The AddEntityAsync method is called on the _tableClient instance to store the product entity.
            await _tableClient.AddEntityAsync(product); // Use the instance's TableClient to add the entity.

            // Construct a response message indicating the successful addition of the product.
            // The message includes the product's name for clarity.
            string responseMessage = $"Product {product.Name} added successfully.";

            // Return an OK response along with the success message.
            // This indicates that the function has successfully processed the request.
            return new OkObjectResult(responseMessage);
        }

    }

    // Define the Product class implementing ITableEntity
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Ensure the property names match what you are setting in the controller
        public string Name { get; set; }
        public string ProductDescription { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }
        public string ImageUrlPath { get; set; }  // Add this if you want to store the image URL
    }


}


