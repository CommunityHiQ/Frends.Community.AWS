using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Amazon.S3;
using Amazon.S3.IO;

namespace Frends.Community.AWS
{
    /// <summary>
    ///     Filemask is Windows-style, eg. *.*, *file?.txt.
    ///     Bucket Name without s3://-prefix.
    /// </summary>
    public class UploadTask
    {
        /// <summary>
        ///     Filemask is Windows-style, eg. *.*, *file?.txt
        ///     Bucketname without s3://-prefix.
        /// </summary>
        /// <param name="input" />
        /// <param name="parameters" />
        /// <param name="options" />
        /// <param name="cancellationToken" />
        /// <returns>List&lt;string&gt;</returns>
        public static List<string> UploadFiles(
            [PropertyTab] UploadInput input,
            [PropertyTab] Parameters parameters,
            [PropertyTab] UploadOptions options,
            CancellationToken cancellationToken
        )
        {
            // First check to see if this task gets performed at all.
            cancellationToken.ThrowIfCancellationRequested();

            #region Error checks

            if (string.IsNullOrWhiteSpace(parameters.AWSAccessKeyID))
                throw new ArgumentNullException(nameof(parameters.AWSAccessKeyID), "Cannot be empty. ");
            if (string.IsNullOrWhiteSpace(parameters.AWSSecretAccessKey))
                throw new ArgumentNullException(nameof(parameters.AWSSecretAccessKey), "Cannot be empty. ");
            if (string.IsNullOrWhiteSpace(parameters.BucketName))
                throw new ArgumentNullException(nameof(parameters.BucketName), "Cannot be empty. ");

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

            #endregion

            return ExecuteUpload(localRoot, filesToCopy, input, parameters, options, cancellationToken);
        }

        private static List<string> ExecuteUpload(
            FileSystemInfo localRoot,
            IEnumerable<FileInfo> filesToCopy,
            UploadInput input,
            Parameters parameters,
            UploadOptions options,
            CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = new List<string>();

            var client = GetS3Client(parameters, cancellationToken);

            using (client)
            {
                var root = GetS3Directory(
                    client,
                    input,
                    parameters,
                    cancellationToken);

                foreach (var file in filesToCopy)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!file.Exists)
                        throw new IOException($"Source file does not exist: {file.FullName}");

                    var s3File = new S3FileInfo(
                        client,
                        parameters.BucketName,
                        Path.Combine(
                            root.Name,
                            options.PreserveFolderStructure
                                ? file.FullName.Replace(localRoot.FullName, string.Empty)
                                : file.Name
                        ));

                    s3File = options.DeleteSource
                        ? s3File.MoveFromLocal(file.FullName, options.Overwrite)
                        : s3File.CopyFromLocal(file.FullName, options.Overwrite);

                    result.Add(options.ReturnListOfObjectKeys ? s3File.FullName : file.FullName);
                }
            }

            return result;
        }

        private static IAmazonS3 GetS3Client(
            Parameters parameters,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new AmazonS3Client(
                parameters.AWSAccessKeyID,
                parameters.AWSSecretAccessKey,
                Utilities.RegionSelection(parameters.Region));
        }

        private static S3DirectoryInfo GetS3Directory(
            IAmazonS3 s3Client,
            UploadInput input,
            Parameters parameters,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dirInfo = new S3DirectoryInfo(s3Client, parameters.BucketName, input.S3Directory);
            if (dirInfo.Exists) return dirInfo;
            dirInfo.Create();
            return dirInfo;
        }
    }
}