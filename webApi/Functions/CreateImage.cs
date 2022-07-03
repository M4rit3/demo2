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

namespace api.Image.Functions
{
    public class CreateImage
    {
        private readonly ILogger<CreateImage> _logger;
        private readonly IImageRepository _imageRepository;

        public CreateImage(IImageRepository imageRepository, ILogger<CreateImage> logger)
        {
            _logger = logger;
            _imageRepository = imageRepository;
        }
        [FunctionName("AddImage")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "createImage")] HttpRequest req)
        {
            
            try
            {
                using var read = new StreamReader(req.Body, Encoding.UTF8);
                var incomingReq = await read.ReadToEndAsync();
                if (!string.IsNullOrEmpty(incomingReq))
                {
                    var imageReq = JsonConvert.DeserializeObject<Models.Image>(incomingReq);
                    var imageModel = new Models.Image
                    {
                        id = Guid.NewGuid().ToString(),
                        partitionKey = imageReq.partitionKey,
                        Name = imageReq.Name,
                        Size = imageReq.Size,
                        Path = imageReq.Path,
                        TimeStamp = imageReq.TimeStamp,
                        md5 = imageReq.md5,
                        operation = imageReq.operation
                    };

                    await _imageRepository.CreateImage(imageModel);
                    return new StatusCodeResult(StatusCodes.Status201Created);


                }
                else
                {
                    return new StatusCodeResult(StatusCodes.Status400BadRequest);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Internal Server Error. Exception, Exception: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

        }
    }
}
