using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System.Security.Cryptography;


namespace MyFunction
{
    public static class ProvaUpload
    {


        [FunctionName("ProvaUpload")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("ContainerName");


            Stream myfile = new MemoryStream();
            var file = req.Form.Files["File"];
            string md5=CalculateMd5(file);
            Console.WriteLine($"{md5}");
            GetImage(md5);
            myfile.Position = 0;
            myfile = file.OpenReadStream();
            var BlobClient = new BlobContainerClient(Connection, containerName);
            var blob = BlobClient.GetBlobClient(file.FileName);
            await blob.UploadAsync(myfile);
            return new OkObjectResult("upload succesful");
        }
        
                 

        public static string CalculateMd5(IFormFile file){
        Stream myBlob = new MemoryStream();
            myBlob = file.OpenReadStream();
            SHA256 mySHA256 = SHA256.Create();
            var hashValue = mySHA256.ComputeHash(myBlob);
            string md5 = BitConverter.ToString(hashValue);
            return  md5;
        }
    }
}
