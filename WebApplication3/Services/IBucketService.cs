using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WebApplication3.Services
{
    public class BucketObjectInfo
    {
        public string Key { get; set; } = string.Empty;
        public long Size { get; set; }
    }

    public interface IBucketService
    {
        Task CreateBucketAsync(string bucketName);
        Task PutObjectAsync(string bucketName, string key, Stream content);
        Task<List<BucketObjectInfo>> ListObjectsAsync(string bucketName);
        Task<Stream?> GetObjectAsync(string bucketName, string key);
        Task DeleteObjectAsync(string bucketName, string key);
    }
}
