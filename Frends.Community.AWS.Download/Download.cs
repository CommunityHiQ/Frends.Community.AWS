using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Frends.Tasks.Attributes;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using System.Threading.Tasks;

namespace Frends.Community.AWS.DL
{
#pragma warning disable 1591
    public enum Regions
    {
        EUWest1, EUWest2, EUCentral1,
        APNortheast1, APNortheast2, APSouth1, APSoutheast1, APSoutheast2,
        CACentral1, CNNorth1, SAEast1,
        USEast1, USEast2, USGovCloudWest1, USWest1, USWest2
    }
#pragma warning restore 1591

    #region Input, Options and Param
    /// <summary>
    ///     Input class, you can download whole directories or single files.
    /// </summary>
    public class Input
    {
        /// <summary>
        ///     Uses different method to download. Whole directory gets all objects recursively.
        /// </summary>
        [DefaultValue(false)]
        public Boolean DownloadWholeDirectory { get; set; }

        /// <summary>
        ///     Downloads ALL objects with this prefix. Creates folder structure.
        ///     Examples: folder/, this/is/prefix/
        /// </summary>
        [ConditionalDisplay(nameof(DownloadWholeDirectory), true)]
        [DefaultDisplayType(DisplayType.Text)]
        [DefaultValue(null)]
        public string SourceDirectory { get; set; }

        /// <summary>
        ///     Downloads single object (file).
        ///     Example: folder/file.txt, this/is/prefix/file
        /// </summary>
        [ConditionalDisplay(nameof(DownloadWholeDirectory), false)]
        [DefaultDisplayType(DisplayType.Text)]
        [DefaultValue(null)]
        public string SourcePrefixAndFilename { get; set; }

        /// <summary>
        ///     Directory to create folders and files to.
        ///     Use trailing backlash ( \ ).
        /// </summary>
        [ConditionalDisplay(nameof(DownloadWholeDirectory), true)]
        [DefaultDisplayType(DisplayType.Text)]
        [DefaultValue(null)]
        public string DestinationPath { get; set; }

        /// <summary>
        ///     Folder to write file.
        ///     You can use different filename.
        /// </summary>
        [ConditionalDisplay(nameof(DownloadWholeDirectory), false)]
        [DefaultDisplayType(DisplayType.Text)]
        [DefaultValue(null)]
        public string DestinationPathAndFilename { get; set; }
    }

    /// <summary>
    ///     Parameter class with username and keys.
    /// </summary>
    public class Parameters
    {
        /// <summary>
        ///     AWS Bucket name with Path
        ///     Example: bucketname/path/to/directory or #env.variable.
        /// </summary>
        [DefaultDisplayType(DisplayType.Expression)]
        public string BucketName { get; set; }

        /// <summary>
        ///     Key name for Amazon s3 File transfer aws_access_key_id
        ///     Use #env.variable.
        /// </summary>
        [PasswordPropertyText(true)]
        [DefaultDisplayType(DisplayType.Expression)]
        public string AWSAccessKeyID { get; set; }

        /// <summary>
        ///     Secret  key name for Amazon s3 File transfer aws_secret_access_key
        ///     Use #env.variable.
        /// </summary>
        [PasswordPropertyText(true)]
        [DefaultDisplayType(DisplayType.Expression)]
        public string AWSSecretAccessKey { get; set; }

        /// <summary>
        ///     Region selection, default EUWest1.
        /// </summary>
        [DisplayName("Region")]
        public Regions Region { get; set; }
    }
    #endregion
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
        public static async Task<List<string>> DownloadAsync(Input input, Parameters parameters, CancellationToken cancellationToken)
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

        private static async Task DownloadUtility(Input input, Parameters parameters, CancellationToken cancellationToken, TaskCompletionSource<List<string>> tcs)
        {
            using (var fileTransferUtility =
                 new TransferUtility(
                     new AmazonS3Client(
                         parameters.AWSAccessKeyID,
                         parameters.AWSSecretAccessKey,
                         RegionSelection(parameters.Region))))
            {
                if (input.DownloadWholeDirectory)
                    await DownloadDirectory(
                        input, parameters, cancellationToken, tcs, fileTransferUtility);
                else
                    await DownloadFile(
                        input, parameters, cancellationToken, tcs, fileTransferUtility);                
            }
        }

        private static async Task DownloadDirectory(Input input, Parameters parameters, CancellationToken cancellationToken, TaskCompletionSource<List<string>> tcs, TransferUtility fileTransferUtility)
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

        private static async Task DownloadFile(Input input, Parameters parameters, CancellationToken cancellationToken, TaskCompletionSource<List<string>> tcs, TransferUtility fileTransferUtility)
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

        /// <summary>
        ///     To create dropdown box for task with enum through RegionEndpoint static list from SDK.
        /// </summary>
        /// <param name="region">Region from enum list.</param>
        /// <returns>Amazon.RegionEndpoint static resource.</returns>
        private static RegionEndpoint RegionSelection(Regions region)
        {
            switch (region)
            {
                case Regions.EUWest1:
                    return RegionEndpoint.EUWest1;
                case Regions.EUWest2:
                    return RegionEndpoint.EUWest2;
                case Regions.EUCentral1:
                    return RegionEndpoint.EUCentral1;
                case Regions.APSoutheast1:
                    return RegionEndpoint.APSoutheast1;
                case Regions.APSoutheast2:
                    return RegionEndpoint.APSoutheast2;
                case Regions.APNortheast1:
                    return RegionEndpoint.APNortheast1;
                case Regions.APNortheast2:
                    return RegionEndpoint.APNortheast2;
                case Regions.APSouth1:
                    return RegionEndpoint.APSouth1;
                case Regions.CACentral1:
                    return RegionEndpoint.CACentral1;
                case Regions.CNNorth1:
                    return RegionEndpoint.CNNorth1;
                case Regions.SAEast1:
                    return RegionEndpoint.SAEast1;
                case Regions.USEast1:
                    return RegionEndpoint.USEast1;
                case Regions.USEast2:
                    return RegionEndpoint.USEast2;
                //case Regions.USGovCloudWest1:
                //return RegionEndpoint.USGovCloudWest1;
                case Regions.USWest1:
                    return RegionEndpoint.USWest1;
                case Regions.USWest2:
                    return RegionEndpoint.USWest2;
                default:
                    return RegionEndpoint.EUWest1;
            }
        }
    }
}
