using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Frends.Tasks.Attributes;
using Amazon.S3;
using Amazon.S3.Transfer;

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
        public static async Task<List<string>> DownloadAsync(
            [CustomDisplay(DisplayOption.Tab)] DownloadInput input,
            [CustomDisplay(DisplayOption.Tab)] Parameters parameters,
            [CustomDisplay(DisplayOption.Tab)] CancellationToken cancellationToken
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
                if (String.IsNullOrWhiteSpace(input.SourceDirectory))
                    throw new ArgumentNullException(nameof(input.SourceDirectory), "Cannot be empty. ");
                if (String.IsNullOrWhiteSpace(input.DestinationPath))
                    throw new ArgumentNullException(nameof(input.DestinationPath), "Cannot be empty. ");
                // Just to help developers out to fix empty folders.
                if (!input.SourceDirectory.Trim().EndsWith(@"/"))
                    input.SourceDirectory += "/";
            }
            else
            {
                if (String.IsNullOrWhiteSpace(input.SourcePrefixAndFilename))
                    throw new ArgumentNullException(nameof(input.SourcePrefixAndFilename), "Cannot be empty. ");
                if (String.IsNullOrWhiteSpace(input.DestinationPathAndFilename))
                    throw new ArgumentNullException(nameof(input.DestinationPathAndFilename), "Cannot be empty. ");
                // Just to help developers out to fix empty folders.
                if (input.SourcePrefixAndFilename.Trim().EndsWith(@"/"))
                    input.SourcePrefixAndFilename.TrimEnd('/');
                if (input.DestinationPathAndFilename.Trim().EndsWith(@"\"))
                    throw new ArgumentException(@"No filename supplied. ", nameof(input.DestinationPathAndFilename));
            }
            #endregion
            // For returning data from events
            var tcs = new TaskCompletionSource<List<string>>();

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
            TaskCompletionSource<List<string>> tcs
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
        /// <returns></returns>
        private static async Task DownloadDirectory(
            DownloadInput input, 
            Parameters parameters, 
            CancellationToken cancellationToken, 
            TaskCompletionSource<List<string>> tcs, 
            TransferUtility fileTransferUtility
            )
        {
            var list = new List<string>();

            // Create the request for directory download
            var request = new TransferUtilityDownloadDirectoryRequest()
            {
                BucketName = parameters.BucketName,
                DownloadFilesConcurrently = false,
                LocalDirectory = input.DestinationPath,
                S3Directory = input.SourceDirectory
            };

            // anon function lets us to have list in same scope
            request.DownloadedDirectoryProgressEvent += (sender, e) =>
            {
                if (e.TransferredBytesForCurrentFile >= e.TotalNumberOfBytesForCurrentFile)
                    list.Add(String.Join(@"\", request.LocalDirectory + e.CurrentFile.Replace(@"/", @"\")));
            };

            await fileTransferUtility.DownloadDirectoryAsync(request, cancellationToken);

            // set the produced list as task completion source result when async method has finished.
            tcs.SetResult(list);
        }

        /// <summary>
        /// Single file download operation.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="tcs"></param>
        /// <param name="fileTransferUtility"></param>
        /// <returns></returns>
        private static async Task DownloadFile(
            DownloadInput input, 
            Parameters parameters, 
            CancellationToken cancellationToken, 
            TaskCompletionSource<List<string>> tcs, 
            TransferUtility fileTransferUtility
            )
        {
            var list = new List<string>();
            // Create the request for single download
            var request = new TransferUtilityDownloadRequest()
            {
                BucketName = parameters.BucketName,
                FilePath = input.DestinationPathAndFilename,
                Key = input.SourcePrefixAndFilename
            };

            request.WriteObjectProgressEvent += (sender, e) =>
            {
                if (e.IsCompleted)
                    list.Add(e.FilePath);
            };
            
            await fileTransferUtility.DownloadAsync(request, cancellationToken);

            tcs.SetResult(list);
        }

    }
}
