using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;

namespace My.Function
{
    public static class ProvaUpload
    {
        [FunctionName("ProvaUpload")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName =Environment.GetEnvironmentVariable("ContainerName");

            Stream myBlob = new MemoryStream();
            var file = req.Form.Files["File"];
            myBlob =file.OpenReadStream();
            var BlobClient = new BlobContainerClient(Connection, containerName);
            var blob = BlobClient.GetBlobClient(file.FileName);
            await blob.UploadAsync(myBlob);

            

         

            return new OkObjectResult("upload succesful");
        }
    }
}
