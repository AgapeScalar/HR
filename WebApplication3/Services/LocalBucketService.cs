using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WebApplication3.Services
{
    public class LocalBucketService : IBucketService
    {
        private readonly string _root;

        public LocalBucketService()
        {
            _root = Path.Combine(AppContext.BaseDirectory, "App_Data", "buckets");
            Directory.CreateDirectory(_root);
        }

        public Task CreateBucketAsync(string bucketName)
        {
            var path = Path.Combine(_root, bucketName);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return Task.CompletedTask;
        }

        public async Task PutObjectAsync(string bucketName, string key, Stream content)
        {
            var path = EnsureBucketPath(bucketName);
            var file = Path.Combine(path, key.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(file) ?? path);
            using var fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None);
            await content.CopyToAsync(fs);
        }

        public Task<List<BucketObjectInfo>> ListObjectsAsync(string bucketName)
        {
            var path = Path.Combine(_root, bucketName);
            var list = new List<BucketObjectInfo>();
            if (!Directory.Exists(path)) return Task.FromResult(list);
            foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                var fi = new FileInfo(f);
                var key = Path.GetRelativePath(path, f).Replace(Path.DirectorySeparatorChar, '/');
                list.Add(new BucketObjectInfo { Key = key, Size = fi.Length });
            }
            return Task.FromResult(list);
        }

        public Task<Stream?> GetObjectAsync(string bucketName, string key)
        {
            var path = Path.Combine(_root, bucketName);
            var file = Path.Combine(path, key.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(file)) return Task.FromResult<Stream?>(null);
            Stream s = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult<Stream?>(s);
        }

        public Task DeleteObjectAsync(string bucketName, string key)
        {
            var path = Path.Combine(_root, bucketName);
            var file = Path.Combine(path, key.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(file)) File.Delete(file);
            return Task.CompletedTask;
        }

        private string EnsureBucketPath(string bucketName)
        {
            var path = Path.Combine(_root, bucketName);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
    }
}
