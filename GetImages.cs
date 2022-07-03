using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace api.Image.Functions
{
    public class GetImages
    {
        private readonly ILogger<GetImages> _logger;
        private readonly IImageRepository _imageRepository;

        public GetImages(IImageRepository imageRepository,ILogger<GetImages> logger )
        {
          _logger = logger;
          _imageRepository = imageRepository;
        }

        [FunctionName("GetImages")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getImagesByPk/{partitionKey}")] HttpRequest req, string partitionKey)
        {
            _logger.LogInformation("C# HTTP trigger function processed for get all images by pk.");
          IActionResult result;
            try
            {
                var images = await _imageRepository.GetImages(partitionKey);
                if (images == null)
                {
                    result = new StatusCodeResult(StatusCodes.Status404NotFound);
                    _logger.LogInformation($"Images with pk {partitionKey} not found. ");
                }
                result = new OkObjectResult(images);
            }
            catch (Exception ex)
            {
               _logger.LogError($"Internal server error. Exception thrown: {ex.Message}");
               result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            return result;





            
        }
    }
}
