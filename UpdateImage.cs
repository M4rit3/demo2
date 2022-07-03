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
using api.Image.Models;

namespace api.Images.Functions
{
    public class UpdateImage
    {
        private readonly ILogger<UpdateImage> _logger;
        private readonly IImageRepository _imageRepository;

        public UpdateImage(IImageRepository imageRepository, ILogger<UpdateImage> logger)
        {
            _logger = logger;
            _imageRepository = imageRepository;
        }

        [FunctionName("UpdateImage")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "updateImage/{md5}")] HttpRequest req, string md5)
        {
            IActionResult result;

            _logger.LogInformation("C# HTTP trigger function processed fo Update Image.");

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
                    using var read = new StreamReader(req.Body, Encoding.UTF8);
                    var incomingReq = await read.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(incomingReq))
                    {
                        var imageReq = JsonConvert.DeserializeObject<Image.Models.Image>(incomingReq);
                        var imageModel = new Image.Models.Image
                        {
                            // id = Guid.NewGuid().ToString(),
                            id = imageReq.id,
                            partitionKey = imageReq.partitionKey,
                            Name = imageReq.Name,
                            Size = imageReq.Size,
                            Path = imageReq.Path,
                            TimeStamp = imageReq.TimeStamp,
                            md5 = imageReq.md5,
                            operation = imageReq.operation
                        };

                        await _imageRepository.UpdateImage(imageModel, md5, imageModel.partitionKey);
                        result = new StatusCodeResult(StatusCodes.Status201Created);
                    }
                    else
                    {
                        result = new StatusCodeResult(StatusCodes.Status400BadRequest);
                    }
                }
            }

            catch (Exception ex)
            {

            _logger.LogError($"Internal Server Error. Exception, Exception: {ex.Message}");
            result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            return result;

        }
        
    }

}
