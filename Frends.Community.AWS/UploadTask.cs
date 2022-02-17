using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace Frends.Community.AWS
{
    /// <summary>
    /// Filemask, eg. *.*, *file?.txt.
    /// Bucket Name without s3://-prefix.
    /// </summary>
    public class UploadTask
    {
        /// <summary>
        /// Filemask, eg. *.*, *file?.txt
        /// Bucketname without s3://-prefix.
        /// </summary>
        /// <param name="input" />
        /// <param name="parameters" />
        /// <param name="options" />
        /// <param name="cancellationToken" />
        /// <returns>List&lt;string&gt;</returns>
        public static async Task<List<string>> UploadFiles(
            [PropertyTab] UploadInput input,
            [PropertyTab] Parameters parameters,
            [PropertyTab] UploadOptions options,
            CancellationToken cancellationToken
        )
        {
            if (!parameters.UseDefaultCredentials && parameters.AwsCredentials == null) parameters.IsAnyNullOrWhiteSpaceThrow();

            if (!Directory.Exists(input.FilePath)) throw new ArgumentException(@"Source path not found. ", nameof(input.FilePath));

            var localRoot = new DirectoryInfo(input.FilePath);

            // If filemask is not set, get all files.
            var filesToCopy = localRoot.GetFiles(
                input.FileMask ?? "*",
                options.UploadFromCurrentDirectoryOnly
                    ? SearchOption.TopDirectoryOnly
                    : SearchOption.AllDirectories);

            if (options.ThrowErrorIfNoMatch && filesToCopy.Length < 1)
                throw new ArgumentException(
                    $"No files match the filemask within supplied path. {nameof(input.FileMask)}");

            return await ExecuteUpload(filesToCopy, input.S3Directory, parameters, options, cancellationToken, input);
        }

        /// <summary>
        /// Prepare to file upload by checking options and file structures.
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

            using (var client = (AmazonS3Client)Utilities.GetS3Client(parameters))
            {
                foreach (var file in filesToCopy)
                {
                    cancellationToken.ThrowIfCancellationRequested();
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
                                await client.GetObjectAsync(request, cancellationToken);
                                throw new ArgumentException($"File {file.Name} already exists in S3 at {fullPath}. Set Overwrite-option to true to overwrite the existing file.");
                            }
                            catch (AmazonS3Exception) { }
                        }
                        await UploadFileToS3(cancellationToken, file, parameters, client, fullPath, input);
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
                                await client.GetObjectAsync(request, cancellationToken);
                                throw new ArgumentException($"File {file.Name} already exists in S3 at {request.Key}. Set Overwrite-option to true to overwrite the existing file.");
                            }
                            catch (AmazonS3Exception) { }
                        }
                        await UploadFileToS3(cancellationToken, file, parameters, client, s3Directory + file.Name, input);
                        if (options.ReturnListOfObjectKeys) result.Add(s3Directory + file.Name);
                        else result.Add(file.FullName);
                    }
                    if (options.DeleteSource) Utilities.DeleteSourceFile(client, cancellationToken, parameters.BucketName, file.FullName, false);
                }
            }
            return result;
        }

        /// <summary>
        /// Upload file(s) to S3.
        /// </summary>
        /// <param name="cancellationToken" />
        /// <param name="file" />
        /// <param name="parameters" />
        /// <param name="client" />
        /// <param name="path" />
        /// <param name="input" />
        /// <returns></returns>
        private static async Task<PutObjectResponse> UploadFileToS3(
            CancellationToken cancellationToken,
            FileInfo file,
            Parameters parameters,
            AmazonS3Client client,
            string path,
            UploadInput input
        )
        {

            try
            {
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = parameters.BucketName,
                    Key = path,
                    FilePath = file.FullName,
                    CannedACL = (input.S3CannedACL) ? Utilities.GetS3CannedACL(input.CannedACL) : Amazon.S3.S3CannedACL.NoACL
            };
                var response = await client.PutObjectAsync(putObjectRequest, cancellationToken);

                return response;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (parameters.ThrowExceptionOnErrorResponse)
                {
                    if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                        throw new SecurityException("Invalid Amazon S3 Credentials - data was not uploaded.", amazonS3Exception);

                    throw new Exception("Unspecified error attempting to upload data: " + amazonS3Exception.Message, amazonS3Exception);
                }

                return null;
                
            }
            
        }
    }
}