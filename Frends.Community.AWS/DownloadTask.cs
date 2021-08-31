using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;

namespace Frends.Community.AWS
{
    /// <summary>
    ///     Amazon AWS S3 File DownloadTask
    /// </summary>
    public class DownloadTask
    {
        /// <summary>
        ///     Amazon AWS S3 DownloadFiles task.
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
            cToken.ThrowIfCancellationRequested();
            if (parameters.AwsCredentials == null) parameters.IsAnyNullOrWhiteSpaceThrow();

            if (string.IsNullOrWhiteSpace(input.DestinationPath))
                throw new ArgumentNullException(nameof(input.DestinationPath));

            return DownloadUtility(input, parameters, option, cToken).Result;
        }

        /// <summary>
        ///     Prepare for download by checking options and finding files from S3.
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
            List<string> paths = new List<string>();
            string targetPath = input.S3Directory + input.SearchPattern;
            Regex mask = new Regex(input.SearchPattern.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));
            using (AmazonS3Client s3Client = (AmazonS3Client)Utilities.GetS3Client(parameters, cancellationToken))
            {
                List<S3Object> allObjectsInDirectory = new List<S3Object>();
                ListObjectsResponse allObjectsResponse = await s3Client.ListObjectsAsync(parameters.BucketName, cancellationToken);
                allObjectsInDirectory = allObjectsResponse.S3Objects;
                foreach (S3Object fileObject in allObjectsInDirectory)
                {
                    if (mask.IsMatch(fileObject.Key.Split('/').Last()) && (targetPath.Split('/').Length == fileObject.Key.Split('/').Length || !option.DownloadFromCurrentDirectoryOnly) && !fileObject.Key.EndsWith("/") && fileObject.Key.StartsWith(input.S3Directory))
                    {
                        if (!input.DestinationPath.EndsWith("/")) Path.Combine(input.DestinationPath, "/");
                        string fullPath = Path.Combine(input.DestinationPath, fileObject.Key.Split('/').Last());
                        if (File.Exists(fullPath) & !option.Overwrite) throw new IOException($"File {fileObject.Key.Split('/').Last()} already exists at {fullPath}. Set Overwrite to true from options to overwrite the file.");
                        paths.Add(await WriteToFile(parameters, fileObject, s3Client, input.DestinationPath, fullPath));
                        if (option.DeleteSourceFile) Utilities.DeleteSourceFile(s3Client, cancellationToken, parameters.BucketName, fileObject.Key, true);
                    }
                }
            }
            if (paths.Count == 0 && option.ThrowErrorIfNoMatches) throw new ArgumentException($"No matches found with search pattern {input.SearchPattern}");
            return paths;
        }

        /// <summary>
        ///     Write files to desired destination path.
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
            string responseBody;
            var request = new GetObjectRequest
            {
                BucketName = parameters.BucketName,
                Key = fileObject.Key
            };

            using (var response = await s3Client.GetObjectAsync(request))
            using (var responseStream = response.ResponseStream)
            using (var reader = new StreamReader(responseStream))
            {
                responseBody = reader.ReadToEnd();
            }
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                File.WriteAllText(fullPath, responseBody);
                return fullPath;
            }
            else
            {
                if (!Directory.Exists(destinationFolder)) Directory.CreateDirectory(destinationFolder);
                File.WriteAllText(fullPath, responseBody);
                return fullPath;
            }
        }
    }
}