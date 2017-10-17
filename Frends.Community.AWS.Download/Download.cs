using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Threading;
using Frends.Community.AWS.Helpers;
using Frends.Tasks.Attributes;
using Amazon.S3;
using Amazon.S3.Transfer;
using System.Threading.Tasks;

namespace Frends.Community.AWS
{
    /// <summary>        
    /// Amazon AWS S3 File Download task
    /// </summary>
    public class Download
    {
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

        /// <summary>
        /// Amazon AWS S3 Download task
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>List&lt;string&gt;</returns>
        public static List<string> DownloadFiles(Input input, Parameters parameters, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Errors to throw if parameters are entered incorrectly.
            // TODO: Change this so user doesnt have to CARE about trailing slash. Add it if DownloadWholeDirectory is true.
            if (input.DownloadWholeDirectory && !input.SourceDirectory.Trim().EndsWith(@"/"))
                throw new ArgumentException(@"Use trailing slash ( / ) to indicate directory. ", nameof(input.SourceDirectory));

            // TODO: Trim trailing slashes away.
            if (!input.DownloadWholeDirectory && input.SourcePrefixAndFilename.Trim().EndsWith(@"/"))
                throw new ArgumentException(@"Filename cannot end with trailing slash ( / ). ", nameof(input.SourcePrefixAndFilename));

            if (!input.DownloadWholeDirectory && input.DestinationPathAndFilename.Trim().EndsWith(@"\"))
                throw new ArgumentException(@"No filename supplied. ", nameof(input.DestinationPathAndFilename));

            var tcs = new TaskCompletionSource<List<string>>();
            var dl = tcs.Task;

            Task.Factory.StartNew(async () => { await DownloadStuff(input, parameters, cancellationToken, tcs); });

            var resultList = new List<string>();

            //if(dl.IsCompleted)

            //var asdf = new Task(() => resultList.Add()

            return dl.Result;
        }

        private static async Task DownloadStuff(Input input, Parameters parameters, CancellationToken cancellationToken, TaskCompletionSource<List<string>> tcs)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested(); // Obligatory cancellation.

                using (var fileTransferUtility = new TransferUtility(
                    new AmazonS3Client(
                        parameters.AWSAccessKeyID,
                        parameters.AWSSecretAccessKey,
                        Region.RegionSelection(parameters.Region))))
                {
                    if (input.DownloadWholeDirectory)
                    {

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
                        var list = new List<string>();

                        request.DownloadedDirectoryProgressEvent += (sender, e) => 
                        {
                            list.Add(e.CurrentFile);
                        };

                        
                        await fileTransferUtility.DownloadDirectoryAsync(request, cancellationToken);
                        tcs.SetResult(list);
                    }
                    else
                    {
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
                        await fileTransferUtility.DownloadAsync(
                            input.DestinationPathAndFilename,
                            parameters.BucketName,
                            input.SourcePrefixAndFilename,
                            cancellationToken);

                        //request.WriteObjectProgressEvent += ReturnProgress;
                        //resultList.Add(input.DestinationPathAndFilename);
                    }
                }
            }
            catch (AmazonS3Exception s3Exception)
            {
                throw new Exception(s3Exception.Message,
                                  s3Exception.InnerException);
            }
            finally
            {
                
            }
        }        
    }
}
