using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using WebApplication3.Models;
using Amazon.S3;
using Amazon.Runtime;
using Amazon.S3.Model;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Text;
using Amazon;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> ResumeSave(IFormFile file, string text)
        {
            return await Homecoming(file, text);
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeCandidate(IFormFile? file, string? text)
        {
            if ((file == null || file.Length == 0) && string.IsNullOrEmpty(text))
            {
                return BadRequest("No file or text provided");
            }

            var repo = HttpContext.RequestServices.GetRequiredService<WebApplication3.Services.ICandidateRepository>();
            var analyzer = HttpContext.RequestServices.GetRequiredService<WebApplication3.Services.IAnalysisService>();

            var candidate = new Candidate();
            candidate.UploadedAt = DateTime.UtcNow;

            if (file != null && file.Length > 0)
            {
                var bucketName = WebApplication3.Services.AwsSettings.BucketName;
                var key = $"uploads/{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var creds = new BasicAWSCredentials(WebApplication3.Services.AwsSettings.AccessKey, WebApplication3.Services.AwsSettings.SecretKey);
                using var s3Client = new AmazonS3Client(creds, Amazon.RegionEndpoint.GetBySystemName(WebApplication3.Services.AwsSettings.Region));
                using var stream = file.OpenReadStream();
                var put = new PutObjectRequest { BucketName = bucketName, Key = key, InputStream = stream, ContentType = file.ContentType };
                await s3Client.PutObjectAsync(put);
                candidate.ResumeKey = key;
                if (file.ContentType?.StartsWith("text") == true)
                {
                    var get = await s3Client.GetObjectAsync(bucketName, key);
                    using var sr = new StreamReader(get.ResponseStream);
                    candidate.RawText = await sr.ReadToEndAsync();
                }
            }
            else
            {
                candidate.RawText = text;
            }

            await repo.SaveCandidateAsync(candidate);

            var analysis = await analyzer.AnalyzeAsync(candidate.RawText ?? string.Empty);
            candidate.Analysis = analysis;
            await repo.UpdateCandidateAsync(candidate);

            return Ok(new { id = candidate.Id, analysis });
        }

        [HttpGet]
        public async Task<IActionResult> GetCandidates()
        {
            var repo = HttpContext.RequestServices.GetRequiredService<WebApplication3.Services.ICandidateRepository>();
            var list = await repo.GetAllAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Homecoming(IFormFile? file, string? text)
        {
            if ((file == null || file.Length == 0) && string.IsNullOrEmpty(text))
            {
                return BadRequest("No file or text provided");
            }
            var bucketName = WebApplication3.Services.AwsSettings.BucketName;

            Stream uploadStream;
            string contentType;
            string fileName;

            if (file != null && file.Length > 0)
            {
                uploadStream = file.OpenReadStream();
                contentType = file.ContentType;
                fileName = Path.GetFileName(file.FileName);
            }
            else
            {
                var bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
                uploadStream = new MemoryStream(bytes);
                contentType = "text/plain";
                fileName = "paste.txt";
            }

            var key = $"uploads/{Guid.NewGuid()}_{fileName}";

            try
            {
                var creds = new BasicAWSCredentials(WebApplication3.Services.AwsSettings.AccessKey, WebApplication3.Services.AwsSettings.SecretKey);
                using var s3Client = new AmazonS3Client(creds, Amazon.RegionEndpoint.GetBySystemName(WebApplication3.Services.AwsSettings.Region));

                using (uploadStream)
                {
                    var put = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = key,
                        InputStream = uploadStream,
                        ContentType = contentType
                    };

                    var resp = await s3Client.PutObjectAsync(put);
                    if (resp.HttpStatusCode == System.Net.HttpStatusCode.OK || resp.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        return Ok(new { message = "Uploaded", bucket = bucketName, key });
                    }
                    return StatusCode(500, new { error = "Upload failed", code = resp.HttpStatusCode.ToString() });
                }
            }
            catch (AmazonS3Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, code = ex.StatusCode.ToString() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
