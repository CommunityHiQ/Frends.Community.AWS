using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.Community.AWS
{
    /// <summary>
    /// Amazon AWS S3 File DownloadTask.
    /// </summary>
    public class DownloadTask
    {
        /// <summary>
        /// Amazon AWS S3 DownloadFiles task.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        /// <param name="option"></param>
        /// <param name="cToken"></param>
        /// <returns>List&lt;string&gt;</returns>
        public static List<string> DownloadFiles(
            [PropertyTab] DownloadInput input,
            [PropertyTab] Parameters parameters,
            [PropertyTab] DownloadOptions option,
            CancellationToken cToken
        )
        {
            if (!parameters.UseDefaultCredentials && parameters.AwsCredentials == null) parameters.IsAnyNullOrWhiteSpaceThrow();
            if (string.IsNullOrWhiteSpace(input.DestinationPath)) throw new ArgumentNullException(nameof(input.DestinationPath));
            return DownloadUtility(input, parameters, option, cToken).Result;
        }

        /// <summary>
        /// Prepare for download by checking options and finding files from S3.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        /// <param name="option"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>List&lt;string&gt;</returns>
        private static async Task<List<string>> DownloadUtility(
            DownloadInput input,
            Parameters parameters,
            DownloadOptions option,
            CancellationToken cancellationToken
        )
        {
            var paths = new List<string>();
            var targetPath = input.S3Directory + input.SearchPattern;
            var mask = new Regex(input.SearchPattern.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));


            var request = new ListObjectsV2Request
            {
                BucketName = parameters.BucketName,
                Encoding = null,
                FetchOwner = false,
                // Added ternary to account for Frends not including null as parameter by default.
                Prefix = string.IsNullOrWhiteSpace(input.S3Directory) ? null : input.S3Directory,
            };


            using (var s3Client = (AmazonS3Client)Utilities.GetS3Client(parameters))
            {
                var allObjectsResponse = await s3Client.ListObjectsV2Async(request, cancellationToken);
                var allObjectsInDirectory = allObjectsResponse.S3Objects;
                foreach (var fileObject in allObjectsInDirectory)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (mask.IsMatch(fileObject.Key.Split('/').Last()) && (targetPath.Split('/').Length == fileObject.Key.Split('/').Length || !option.DownloadFromCurrentDirectoryOnly) && !fileObject.Key.EndsWith("/") && fileObject.Key.StartsWith(input.S3Directory))
                    {
                        if (!input.DestinationPath.EndsWith(Path.DirectorySeparatorChar.ToString())) input.DestinationPath += Path.DirectorySeparatorChar.ToString();

                        var fullPath = Path.Combine(input.DestinationPath, fileObject.Key.Split('/').Last());
                        if (File.Exists(fullPath) & !option.Overwrite) throw new IOException($"File {fileObject.Key.Split('/').Last()} already exists at {fullPath}. Set Overwrite to true from options to overwrite the file.");
                        paths.Add(await WriteToFile(parameters, fileObject, s3Client, input.DestinationPath, fullPath));
                        if (option.DeleteSourceFile) await Utilities.DeleteSourceFile(s3Client, cancellationToken, parameters.BucketName, fileObject.Key, true);
                    }
                }
            }
            if (paths.Count == 0 && option.ThrowErrorIfNoMatches) throw new ArgumentException($"No matches found with search pattern {input.SearchPattern}");
            return paths;
        }

        /// <summary>
        /// Write files to desired destination path.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="fileObject"></param>
        /// <param name="s3Client"></param>
        /// <param name="destinationFolder"></param>
        /// <param name="fullPath"></param>
        /// <returns>string</returns>
        private static async Task<string> WriteToFile(
            Parameters parameters,
            S3Object fileObject,
            AmazonS3Client s3Client,
            string destinationFolder,
            string fullPath
        )
        {
            var request = new GetObjectRequest
            {
                BucketName = parameters.BucketName,
                Key = fileObject.Key
            };

            using (var response = await s3Client.GetObjectAsync(request))
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                if (!Directory.Exists(destinationFolder))
                    Directory.CreateDirectory(destinationFolder);

                using (var stream = response.ResponseStream)
                {
                    using (var outFile = File.Create(fullPath))
                        stream.CopyTo(outFile);
                }
            }
            return fullPath;
        }
    }
}