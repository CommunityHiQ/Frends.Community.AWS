using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Frends.Tasks.Attributes;
using Frends.Community.AWS.Helpers;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace Frends.Community.AWS
{

#pragma warning disable 1591
    public enum StorageClasses
    {
        Standard, StandardInfrequent, Reduced, Glacier
    }
#pragma warning restore 1591

    /// <summary>
    /// Input filepath and filemask.
    /// </summary>
    public class Input
    {
        /// <summary>
        /// Path to folder.
        /// ( c:\temp\ , \\network\folder )
        /// </summary>
        [DefaultValue(@"c:\temp\")]
        [DefaultDisplayType(DisplayType.Text)]
        public string FilePath { get; set; }

        /// <summary>
        /// Windows-style filemask, ( *.* , ?_file.*, foo_*.txt ).
        /// Empty field means whole directory.
        /// </summary>
        [DefaultValue(@"*.*")]
        [DefaultDisplayType(DisplayType.Text)]
        public string FileMask { get; set; }
    }

    /// <summary>
    /// Parameter class.
    /// </summary>
    public class Parameters
    {
        /// <summary>
        /// AWS Bucketname, you can add folder path here.
        /// Prefix will be added after this.
        /// With Expression-mode, you can add prefixes ( #env.bucket + @"/prefix").
        /// Do NOT use trailing slash. It will create empty folders.
        /// </summary>
        [DefaultValue("*REQUIRED*")]
        [DefaultDisplayType(DisplayType.Expression)]
        public string Bucketname { get; set; }

        /// <summary>
        /// Object prefix ( folder path ).
        /// Use this to set prefix for each file.
        /// </summary>
        [DefaultValue(null)]
        [DefaultDisplayType(DisplayType.Text)]
        public string Prefix { get; set; }

        /// <summary>
        /// Access Key.
        /// Use #env-variable with secret field for security.
        /// </summary>
        [DefaultValue("*REQUIRED*")]
        [PasswordPropertyText(true)]
        [DefaultDisplayType(DisplayType.Expression)]
        public string AWSAccessKeyID { get; set; }

        /// <summary>
        /// Secret Access Key.
        /// Use #env-variable with secret field for security.         
        /// </summary>
        [DefaultValue("*REQUIRED*")]
        [PasswordPropertyText(true)]
        [DefaultDisplayType(DisplayType.Expression)]
        public string AWSSecretAccessKey { get; set; }

        /// <summary>
        /// Region selection, choose nearest.
        /// Default is EUWest1.
        /// </summary>
        public Regions Region { get; set; }
    }

    /// <summary>
    /// Task behaviour.
    /// Defaults work fine.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// If there are no files in the path matching the filemask supplied,
        /// throw error to fail the process.
        /// </summary>
        [DefaultValue(true)]
        public Boolean ThrowErrorIfNoMatch { get; set; }

        /// <summary>
        /// If you wish, you can return object keys from S3
        /// ( format: bucket/prefix/prefix/filename.extension )
        /// </summary>
        [DefaultValue(false)]
        public Boolean ReturnListOfObjectKeys { get; set; }

        /// <summary>
        /// You can specify Storage Class for uploaded files.
        /// Standard is default.
        /// Consult AWS S3 Documentation for others.
        /// </summary>
        public StorageClasses StorageClass { get; set; }
    }

    /// <summary>        
    /// Filemask is Windows-style, eg. *.*, *file?.txt.
    /// Bucket Name without s3://-prefix.
    /// </summary>
    public class Upload
    {
        /// <summary>
        /// TASK OVERWRITES FILES WITH SAME PREFIX AND KEY!
        /// Trailing slashes in bucketname or prefix will show as folder.
        /// Filemask is Windows-style, eg. *.*, *file?.txt
        /// Bucketname without s3://-prefix.
        /// <param name="input"/>
        /// <param name="parameters"/>
        /// <param name="options"/>
        /// <param name="cancellationToken"/>
        /// </summary>
        /// <returns>List&lt;string&gt; of filenames transferred. Optionally, return List&lt;string&gt; of object keys in S3.</returns>
        public static List<string> UploadFiles(Input input, Parameters parameters, Options options, CancellationToken cancellationToken)
        {
            // First check to see if this task gets performed at all.
            cancellationToken.ThrowIfCancellationRequested();

            if (!Directory.Exists(input.FilePath))
            {
                throw new ArgumentException(@"Source path not found.", nameof(input.FilePath));
            }

            var filesToCopy = string.IsNullOrWhiteSpace(input.FileMask) ? 
                Directory.GetFiles(input.FilePath) : 
                Directory.GetFiles(input.FilePath, input.FileMask);
            var returnList = new List<string>();

            if (filesToCopy.Length < 1 && options.ThrowErrorIfNoMatch)
            {
                throw new ArgumentException(@"No files match the filemask and path supplied. ", nameof(input.FileMask));
            }

            TransferUtility fileTransferUtility =
                new TransferUtility(
                    new AmazonS3Client(
                        parameters.AWSAccessKeyID,
                        parameters.AWSSecretAccessKey, 
                        Region.RegionSelection(parameters.Region))
                        );

            foreach (var file in filesToCopy)
            {
                // Added check so with each file cancel is checked.
                cancellationToken.ThrowIfCancellationRequested();
                TransferUtilityUploadRequest fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    AutoCloseStream = true,
                    BucketName = parameters.Bucketname,
                    FilePath = file,
                    StorageClass = StorageClassSelection(options.StorageClass),
                    //PartSize = 6291456, // 6 MB.
                    Key = String.IsNullOrWhiteSpace(parameters.Prefix) ? 
                        Path.GetFileName(file) : 
                        String.Join("/", parameters.Prefix, Path.GetFileName(file))
                };

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    fileTransferUtility.Upload(fileTransferUtilityRequest);
                }
                catch (AmazonS3Exception s3Exception)
                {
                    throw new Exception(@"Successful transfers: " + 
                        String.Join(",", returnList) + 
                        " Exception: " + 
                        s3Exception.Message,
                        s3Exception.InnerException);
                }

                if (options.ReturnListOfObjectKeys)
                {
                    returnList.Add(String.Join("/", fileTransferUtilityRequest.BucketName, fileTransferUtilityRequest.Key));
                }
                else
                {
                    returnList.Add(fileTransferUtilityRequest.FilePath);
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
            return returnList;
        }

        /// <summary>
        /// You can select different type of storage redundancy option.
        /// Standard being the default with high redundancy and accessed often, but is the most expensive.
        /// Defaults to Standard.
        /// </summary>
        /// <param name="s3StorageClass"></param>
        /// <returns>S3StorageClass-object for UploadRequest-parameter.</returns>
        private static S3StorageClass StorageClassSelection(StorageClasses s3StorageClass)
        {
            switch (s3StorageClass)
            {
                case StorageClasses.Standard:
                    return S3StorageClass.Standard;
                case StorageClasses.StandardInfrequent:
                    return S3StorageClass.StandardInfrequentAccess;
                case StorageClasses.Reduced:
                    return S3StorageClass.ReducedRedundancy;
                case StorageClasses.Glacier:
                    return S3StorageClass.Glacier;
                default:
                    return S3StorageClass.Standard;
            }

        }
    }
}