using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace Frends.Community.AWS
{
    /// <summary>
    ///     Filemask is Windows-style, eg. *.*, *file?.txt.
    ///     Bucket Name without s3://-prefix.
    /// </summary>
    public class UploadTask
    {
        /// <summary>
        ///     TASK OVERWRITES FILES WITH SAME PREFIX AND KEY!
        ///     Trailing slashes in bucketname or prefix will show as folder.
        ///     Filemask is Windows-style, eg. *.*, *file?.txt
        ///     Bucketname without s3://-prefix.
        /// </summary>
        /// <param name="input" />
        /// <param name="parameters" />
        /// <param name="options" />
        /// <param name="cancellationToken" />
        /// <returns>List&lt;string&gt;</returns>
        public static async Task<List<string>> UploadAsync(
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

            var filesToCopy = string.IsNullOrWhiteSpace(input.FileMask)
                ? Directory.GetFiles(input.FilePath)
                : Directory.GetFiles(input.FilePath, input.FileMask,
                    options.UploadFromCurrentDirectoryOnly
                        ? SearchOption.TopDirectoryOnly
                        : SearchOption.AllDirectories);

            if (options.ThrowErrorIfNoMatch && filesToCopy.Length < 1)
                throw new ArgumentException(
                    $"No files match the filemask within supplied path. {nameof(input.FileMask)}");

            #endregion

            var result = new List<string>();

            using (var fileTransferUtility = new TransferUtility(
                new AmazonS3Client(
                    parameters.AWSAccessKeyID,
                    parameters.AWSSecretAccessKey,
                    Utilities.RegionSelection(parameters.Region)
                )))
            {
                foreach (var file in filesToCopy)
                {
                    var request = new TransferUtilityUploadRequest
                    {
                        AutoCloseStream = true,
                        BucketName = parameters.BucketName,
                        FilePath = file,
                        StorageClass = Utilities.StorageClassSelection(options.StorageClass),
                        //PartSize = 6291456, // 6 MB.
                        Key = input.Prefix + Path.GetFileName(file)
                    };

                    //register to event, when done, add to result list
                    request.UploadProgressEvent += (o, e) =>
                    {
                        if (e.PercentDone != 100) return;
                        result.Add(options.ReturnListOfObjectKeys ? request.Key : e.FilePath);
                    };

                    await fileTransferUtility.UploadAsync(request, cancellationToken);
                }

                return result;
            }
        }
    }
}