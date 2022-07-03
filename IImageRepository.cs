using System.Collections.Generic;
using System.Threading.Tasks;
using api.Image.Models;
public interface IImageRepository
{
    Task<List<Image>> GetImages(string partitionKey);
    Task<Image> GetImage(string md5);
    Task CreateImage (Image image);
    Task UpdateImage(Image image, string md5, string partitionKey);
    Task Delete(Image image);

}