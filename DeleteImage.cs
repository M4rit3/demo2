using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace api.Image.Functions
{
    public class DeleteImage
    {
        private readonly ILogger<DeleteImage> _logger;
        private readonly IImageRepository _imageRepository;

        public DeleteImage(IImageRepository imageRepository, ILogger<DeleteImage> logger)
        {
            _logger = logger;
            _imageRepository = imageRepository;
        }
        [FunctionName("DeleteImage")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "deleteImag/{md5}")] HttpRequest req, string md5)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request for Delete Image.");
            IActionResult result;
            try
            {
                var image = await _imageRepository.GetImage(md5);
                if (image == null)
                {
                    result = new StatusCodeResult(StatusCodes.Status404NotFound);
                    _logger.LogInformation($"Image with md5 {md5} doesn't exist. ");
                }
                else
                {
                    await _imageRepository.Delete(image);
                    result = new StatusCodeResult(StatusCodes.Status204NoContent);
                }
                
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
