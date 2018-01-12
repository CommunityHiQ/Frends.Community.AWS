using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.Tasks.Attributes;
using Amazon.S3;
using Amazon.S3.Transfer;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frends.Community.AWS
{
    /// <summary>        
    ///     Amazon AWS S3 File Download task
    /// </summary>
    public class Download
    {
        /// <summary>
        ///     Amazon AWS S3 Transfer files.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>List&lt;string&gt;</returns>
        public static async Task<JToken> DownloadAsync(
            [CustomDisplay(DisplayOption.Tab)] DownloadInput input,
            [CustomDisplay(DisplayOption.Tab)] Parameters parameters,
            CancellationToken cancellationToken
            )
        {
            cancellationToken.ThrowIfCancellationRequested();

            #region Error checks and helps
            if (string.IsNullOrWhiteSpace(parameters.AWSAccessKeyID))
                throw new ArgumentNullException(nameof(parameters.AWSAccessKeyID), "Cannot be empty. ");
            if (string.IsNullOrWhiteSpace(parameters.AWSSecretAccessKey))
                throw new ArgumentNullException(nameof(parameters.AWSSecretAccessKey), "Cannot be empty. ");
            if (string.IsNullOrWhiteSpace(parameters.BucketName))
                throw new ArgumentNullException(nameof(parameters.BucketName), "Cannot be empty. ");
            if (input.DownloadWholeDirectory) {
                if (String.IsNullOrWhiteSpace(input.SourcePrefix))
                    throw new ArgumentNullException(nameof(input.SourcePrefix), "Cannot be empty. ");
                if (String.IsNullOrWhiteSpace(input.DestinationPath))
                    throw new ArgumentNullException(nameof(input.DestinationPath), "Cannot be empty. ");
            }
            else
            {
                if (String.IsNullOrWhiteSpace(input.SourcePrefixAndKey))
                    throw new ArgumentNullException(nameof(input.SourcePrefixAndKey), "Cannot be empty. ");
                if (String.IsNullOrWhiteSpace(input.DestinationPathAndFilename))
                    throw new ArgumentNullException(nameof(input.DestinationPathAndFilename), "Cannot be empty. ");
                if (input.DestinationPathAndFilename.Trim().EndsWith(@"\"))
                    throw new ArgumentException(@"No filename supplied. ", nameof(input.DestinationPathAndFilename));
            }
            #endregion
            // For returning data from events
            var tcs = new TaskCompletionSource<JToken>();

            // awaited method, tcs gets our data and fills up when .Result is called.
            try
            {
                await DownloadUtility(input, parameters, cancellationToken, tcs);;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in AWS Download: {ex.Message} : {ex.InnerException.Message}");
            }

            return tcs.Task.Result;
        }
        /// <summary>
        /// Method to create transfer utility object and run download operations.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="tcs"></param>
        /// <returns></returns>
        private static async Task DownloadUtility(
            DownloadInput input, 
            Parameters parameters, 
            CancellationToken cancellationToken, 
            TaskCompletionSource<JToken> tcs
            )
        {
            using (var fileTransferUtility =
                 new TransferUtility(
                     new AmazonS3Client(
                         parameters.AWSAccessKeyID,
                         parameters.AWSSecretAccessKey,
                         Utilities.RegionSelection(parameters.Region))))
            {
                if (input.DownloadWholeDirectory)
                    await DownloadDirectory(
                        input, parameters, cancellationToken, tcs, fileTransferUtility);
                else
                    await DownloadFile(
                        input, parameters, cancellationToken, tcs, fileTransferUtility);                
            }
        }

        /// <summary>
        /// Directory download operation.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="tcs"></param>
        /// <param name="fileTransferUtility"></param>
        /// <returns>Task</returns>
        private static async Task DownloadDirectory(
            DownloadInput input, 
            Parameters parameters, 
            CancellationToken cancellationToken, 
            TaskCompletionSource<JToken> tcs, 
            TransferUtility fileTransferUtility
            )
        {
            // Create the request for directory download
            var request = new TransferUtilityDownloadDirectoryRequest()
            {
                BucketName = parameters.BucketName,
                DownloadFilesConcurrently = false,
                LocalDirectory = input.DestinationPath,
                S3Directory = input.SourcePrefix
            };

            var filelist = new DownloadDirectoryResultToken();

            // anon function lets us to have list in same scope
            request.DownloadedDirectoryProgressEvent += (sender, e) =>
            {
                if (e.TransferredBytesForCurrentFile >= e.TotalNumberOfBytesForCurrentFile)
                {
                    var path = Path.Combine(request.LocalDirectory, e.CurrentFile.Replace(@"/", @"\"));
                    var file = new DownloadResultToken()
                    {
                        FilePath = path,
                        ObjectKey = input.SourcePrefix + e.CurrentFile,
                        Size = e.TotalNumberOfBytesForCurrentFile
                    };

                    filelist.Add(file);
                }

                if (e.NumberOfFilesDownloaded >= e.TotalNumberOfFiles)
                {
                    filelist.NumberOfFiles = e.NumberOfFilesDownloaded;
                    filelist.TotalSize = e.TotalBytes;
                }
            };

            await fileTransferUtility.DownloadDirectoryAsync(request, cancellationToken);

            // set the produced list as task completion source result when async method has finished.
            tcs.SetResult(filelist.ToJToken());
        }

        /// <summary>
        /// Single file download operation.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="tcs"></param>
        /// <param name="fileTransferUtility"></param>
        /// <returns>Task</returns>
        private static async Task DownloadFile(
            DownloadInput input, 
            Parameters parameters, 
            CancellationToken cancellationToken, 
            TaskCompletionSource<JToken> tcs, 
            TransferUtility fileTransferUtility
            )
        {
            var file = new DownloadResultToken();
            // Create the request for single download
            var request = new TransferUtilityDownloadRequest()
            {
                BucketName = parameters.BucketName,
                FilePath = input.DestinationPathAndFilename,
                Key = input.SourcePrefixAndKey
            };

            request.WriteObjectProgressEvent += (sender, e) =>
            {
                if (e.IsCompleted)
                {
                    file.ObjectKey = e.Key;
                    file.FilePath = e.FilePath;
                    file.Size = e.TotalBytes;
                }
            };
            
            await fileTransferUtility.DownloadAsync(request, cancellationToken);

            tcs.SetResult(file.ToJToken());
        }

    }
}
