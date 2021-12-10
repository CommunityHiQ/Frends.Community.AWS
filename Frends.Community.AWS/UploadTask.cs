using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace Frends.Community.AWS
{
    /// <summary>
    ///     Filemask, eg. *.*, *file?.txt.
    ///     Bucket Name without s3://-prefix.
    /// </summary>
    public class UploadTask
    {
        /// <summary>
        ///     Filemask, eg. *.*, *file?.txt
        ///     Bucketname without s3://-prefix.
        /// </summary>
        /// <param name="input" />
        /// <param name="parameters" />
        /// <param name="options" />
        /// <param name="cancellationToken" />
        /// <returns>List&lt;string&gt;</returns>
        public static Task<List<string>> UploadFiles(
            [PropertyTab] UploadInput input,
            [PropertyTab] Parameters parameters,
            [PropertyTab] UploadOptions options,
            CancellationToken cancellationToken
        )
        {
            // First check to see if this task gets performed at all.
            cancellationToken.ThrowIfCancellationRequested();

            if (parameters.AwsCredentials == null) parameters.IsAnyNullOrWhiteSpaceThrow();

            if (!Directory.Exists(input.FilePath))
                throw new ArgumentException(@"Source path not found. ", nameof(input.FilePath));

            var localRoot = new DirectoryInfo(input.FilePath);
            var filesToCopy = localRoot.GetFiles(
                input.FileMask ?? "*", // if filemask is not set, get all files.
                options.UploadFromCurrentDirectoryOnly
                    ? SearchOption.TopDirectoryOnly
                    : SearchOption.AllDirectories);

            if (options.ThrowErrorIfNoMatch && filesToCopy.Length < 1)
                throw new ArgumentException(
                    $"No files match the filemask within supplied path. {nameof(input.FileMask)}");

            return ExecuteUpload(filesToCopy, input.S3Directory, parameters, options, cancellationToken, input);
        }

        /// <summary>
        ///     Prepare to file upload by checking options and file structures.
        /// </summary>
        /// <param name="filesToCopy" />
        /// <param name="s3Directory" />
        /// <param name="parameters" />
        /// <param name="options" />
        /// <param name="cancellationToken" />
        /// <param name="input" />
        /// <returns>List&lt;string&gt;</returns>
        private static async Task<List<string>> ExecuteUpload(
            IEnumerable<FileInfo> filesToCopy,
            string s3Directory,
            Parameters parameters,
            UploadOptions options,
            CancellationToken cancellationToken,
            UploadInput input
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = new List<string>();

            using (var client = (AmazonS3Client)Utilities.GetS3Client(parameters, cancellationToken))
            {
                foreach (var file in filesToCopy)
                {
                    if ((file.FullName.Split(Path.DirectorySeparatorChar).Length > input.FilePath.Split(Path.DirectorySeparatorChar).Length && options.PreserveFolderStructure))
                    {
                        var subfolders = file.FullName.Replace(file.Name, "").Replace(input.FilePath.Replace(file.Name, ""), "").Replace(Path.DirectorySeparatorChar, '/');
                        if (subfolders.StartsWith("/")) subfolders = subfolders.Remove(0, 1);
                        var fullPath = s3Directory + subfolders + file.Name;
                        if (!options.Overwrite)
                        {
                            try
                            {
                                var request = new GetObjectRequest
                                {
                                    BucketName = parameters.BucketName,
                                    Key = fullPath
                                };
                                var _ = await client.GetObjectAsync(request, cancellationToken);
                                throw new ArgumentException($"File {file.Name} already exists in S3 at {fullPath}. Set Overwrite-option to true to overwrite the existing file.");
                            }
                            catch (AmazonS3Exception) { }
                        }
                        UploadFileToS3(cancellationToken, file, parameters, client, fullPath);
                        result.Add(options.ReturnListOfObjectKeys ? fullPath : file.FullName);
                    }
                    else
                    {
                        if (!options.Overwrite)
                        {
                            try
                            {
                                var request = new GetObjectRequest
                                {
                                    BucketName = parameters.BucketName,
                                    Key = s3Directory + file.Name
                                };
                                var _ = await client.GetObjectAsync(request, cancellationToken);
                                throw new ArgumentException($"File {file.Name} already exists in S3 at {request.Key}. Set Overwrite-option to true to overwrite the existing file.");
                            }
                            catch (AmazonS3Exception) { }
                        }
                        UploadFileToS3(cancellationToken, file, parameters, client, s3Directory + file.Name);
                        if (options.ReturnListOfObjectKeys) result.Add(s3Directory + file.Name);
                        else result.Add(file.FullName);
                    }
                    if (options.DeleteSource) Utilities.DeleteSourceFile(client, cancellationToken, parameters.BucketName, file.FullName, false);
                }
            }
            return result;
        }

        /// <summary>
        ///     Upload file(s) to S3.
        /// </summary>
        /// <param name="cancellationToken" />
        /// <param name="file" />
        /// <param name="parameters" />
        /// <param name="client" />
        /// <param name="path" />
        /// <returns></returns>
        private static async void UploadFileToS3(
            CancellationToken cancellationToken,
            FileInfo file,
            Parameters parameters,
            AmazonS3Client client,
            string path
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = parameters.BucketName,
                Key = path,
                FilePath = file.FullName
            };
            _ = await client.PutObjectAsync(putObjectRequest, cancellationToken);
        }
    }
}