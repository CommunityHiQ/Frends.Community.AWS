﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Frends.Community.AWS.Helpers;
using Frends.Tasks.Attributes;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System.Threading.Tasks;

namespace Frends.Community.AWS
{
    /// <summary>        
    /// Amazon AWS S3 File Download task
    /// </summary>
    public class Download
    {
        #region Input, Options and Param
        /// <summary>
        /// Input class, you can download whole directories or single files.
        /// </summary>
        public class Input
        {
            /// <summary>
            /// Uses different method to download. Whole directory gets all objects recursively.
            /// </summary>
            [DefaultValue(false)]
            public Boolean DownloadWholeDirectory { get; set; }

            /// <summary>
            /// Downloads ALL objects with this prefix. Creates folder structure.
            /// Examples: folder/, this/is/prefix/
            /// </summary>
            [ConditionalDisplay(nameof(DownloadWholeDirectory), true)]
            [DefaultDisplayType(DisplayType.Text)]
            [DefaultValue(null)]
            public string SourceDirectory { get; set; }

            /// <summary>
            /// Downloads single object (file).
            /// Example: folder/file.txt, this/is/prefix/file
            /// </summary>
            [ConditionalDisplay(nameof(DownloadWholeDirectory), false)]
            [DefaultDisplayType(DisplayType.Text)]
            [DefaultValue(null)]
            public string SourcePrefixAndFilename { get; set; }

            /// <summary>
            /// Directory to create folders and files to.
            /// Use trailing backlash ( \ ).
            /// </summary>
            [ConditionalDisplay(nameof(DownloadWholeDirectory), true)]
            [DefaultDisplayType(DisplayType.Text)]
            [DefaultValue(null)]
            public string DestinationPath { get; set; }

            /// <summary>
            /// Folder to write file.
            /// You can use different filename.
            /// </summary>
            [ConditionalDisplay(nameof(DownloadWholeDirectory), false)]
            [DefaultDisplayType(DisplayType.Text)]
            [DefaultValue(null)]
            public string DestinationPathAndFilename { get; set; }
        }

        /// <summary>
        /// Parameter class with username and keys.
        /// </summary>
        public class Parameters
        {
            /// <summary>
            /// AWS Bucket name with Path
            /// Example: bucketname/path/to/directory or #env.variable.
            /// </summary>
            [DefaultValue("")]
            public string BucketName { get; set; }

            /// <summary>
            /// Key name for Amazon s3 File transfer aws_access_key_id
            /// Use #env.variable.
            /// </summary>
            [DefaultValue("")]
            [PasswordPropertyText(true)]
            public string AWSAccessKeyID { get; set; }

            /// <summary>
            /// Secret  key name for Amazon s3 File transfer aws_secret_access_key
            /// Use #env.variable.
            /// </summary>
            [DefaultValue("")]
            [PasswordPropertyText(true)]
            public string AWSSecretAccessKey { get; set; }

            /// <summary>
            /// Region selection, default EUWest1.
            /// </summary>
            [DisplayName("Region")]
            public Regions Region { get; set; }
        }
        #endregion

        /// <summary>
        /// Amazon AWS S3 Transfer files.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>List&lt;string&gt;</returns>
        public static async Task<List<string>> DownloadAsync(Input input, Parameters parameters, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            #region Error checks and helps
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
                         Helpers.Helpers.RegionSelection(parameters.Region))))
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
                S3Directory = input.SourceDirectory,
                //ModifiedSinceDate = "", // TODO: How does this work?
                //UnmodifiedSinceDate = "", // TODO: How does this work?
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
                Key = input.SourcePrefixAndFilename,
                //ModifiedSinceDate = "", // TODO: How does this work? 
                //ServerSideEncryptionCustomerMethod = "", // TODO: How does this work?
                //ServerSideEncryptionCustomerProvidedKey = "", // TODO: How does this work?
                //ServerSideEncryptionCustomerProvidedKeyMD5 = "", // TODO: How does this work?
                //UnmodifiedSinceDate = "", // TODO: How does this work?
                //VersionId = "" // TODO: How does this work?
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
