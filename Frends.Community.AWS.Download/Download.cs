using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Threading;
using Frends.Tasks.Attributes;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace FRENDS.Common.AWS
{
    /// <summary>        
    /// Amazon AWS S3 File Download task
    /// </summary>
    public class Download
    {
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
        /// Parameter class.
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

        public class Options
        {

        }


        /// <summary>
        /// Amazon AWS S3 Single File Download
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static List<string> DownloadFiles(Input input, Parameters parameters, Options options, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<string> resultList = new List<string>();

            TransferUtility fileTransferUtility = new TransferUtility(
                new AmazonS3Client(
                    parameters.AWSAccessKeyID, parameters.AWSSecretAccessKey, Region.RegionSelection(parameters.Region)));

            if (input.DownloadWholeDirectory && !input.SourceDirectory.Trim().EndsWith(@"/"))
                throw new ArgumentException(@"Use trailing slash ( / ) to indicate directory. ", nameof(input.SourceDirectory));

            if (!input.DownloadWholeDirectory && input.SourcePrefixAndFilename.Trim().EndsWith(@"/"))
                throw new ArgumentException(@"Filename cannot end with trailing slash ( / ). ", nameof(input.SourcePrefixAndFilename));

            if (!input.DownloadWholeDirectory && input.DestinationPathAndFilename.Trim().EndsWith(@"\"))
                throw new ArgumentException(@"No filename supplied. ", nameof(input.DestinationPathAndFilename));

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (input.DownloadWholeDirectory) {
                    fileTransferUtility.DownloadDirectory(parameters.BucketName, input.SourceDirectory, input.DestinationPath);
                    resultList = new List<string>(Directory.GetFiles(input.DestinationPath, "*.*", SearchOption.AllDirectories));
                } else {
                    fileTransferUtility.Download(input.DestinationPathAndFilename, parameters.BucketName, input.SourcePrefixAndFilename);
                    resultList.Add(input.DestinationPathAndFilename);
                }
            }
            catch (AmazonS3Exception s3Exception)
            {
                throw new Exception(s3Exception.Message,
                                  s3Exception.InnerException);
            }
            return resultList;
        }
    }
}
