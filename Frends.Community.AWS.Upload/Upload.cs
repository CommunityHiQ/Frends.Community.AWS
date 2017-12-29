using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.Tasks.Attributes;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace Frends.Community.AWS.UL
{
#pragma warning disable 1591
    public enum Regions
    {
        EUWest1, EUWest2, EUCentral1,
        APNortheast1, APNortheast2, APSouth1, APSoutheast1, APSoutheast2,
        CACentral1, CNNorth1, SAEast1,
        USEast1, USEast2, USGovCloudWest1, USWest1, USWest2
    }
    public enum StorageClasses
    {
        Standard, StandardInfrequent, Reduced, Glacier
    }
#pragma warning restore 1591

    /// <summary>
    ///     Input filepath and filemask.
    /// </summary>
    public class Input
    {
        /// <summary>
        ///     Path to folder.
        ///     ( c:\temp\ , \\network\folder )
        /// </summary>
        [DefaultValue(@"c:\temp\")]
        [DefaultDisplayType(DisplayType.Text)]
        public string FilePath { get; set; }

        /// <summary>
        ///     Windows-style filemask, ( *.* , ?_file.*, foo_*.txt ).
        ///     Empty field means whole directory.
        /// </summary>
        [DefaultValue(@"*.*")]
        [DefaultDisplayType(DisplayType.Text)]
        public string FileMask { get; set; }
    }
    /// <summary>
    ///     Parameter class.
    /// </summary>
    public class Parameters
    {
        /// <summary>
        ///     AWS Bucketname, you can add folder path here.
        ///     Prefix will be added after this.
        ///     With Expression-mode, you can add prefixes ( #env.bucket + @"/prefix").
        ///     Do NOT use trailing slash. It will create empty folders.
        /// </summary>
        [DefaultDisplayType(DisplayType.Expression)]
        public string BucketName { get; set; }

        /// <summary>
        ///     Access Key.
        ///     Use #env-variable with secret field for security.
        /// </summary>
        [PasswordPropertyText(true)]
        [DefaultDisplayType(DisplayType.Expression)]
        public string AWSAccessKeyID { get; set; }

        /// <summary>
        ///     Secret Access Key.
        ///     Use #env-variable with secret field for security.         
        /// </summary>
        [PasswordPropertyText(true)]
        [DefaultDisplayType(DisplayType.Expression)]
        public string AWSSecretAccessKey { get; set; }

        /// <summary>
        ///     Region selection, choose nearest.
        ///     Default is EUWest1.
        /// </summary>
        public Regions Region { get; set; }

        /// <summary>
        ///     Object prefix ( folder path ).
        ///     Use this to set prefix for each file.
        /// </summary>
        [DefaultValue(null)]
        [DefaultDisplayType(DisplayType.Text)]
        public string Prefix { get; set; }
    }

    /// <summary>
    ///     Task behaviour.
    ///     Defaults work fine.
    /// </summary>
    public class Options
    {
        /// <summary>
        ///     If there are no files in the path matching the filemask supplied,
        ///     throw error to fail the process.
        /// </summary>
        [DefaultValue(true)]
        public Boolean ThrowErrorIfNoMatch { get; set; }

        /// <summary>
        ///     If you wish, you can return object keys from S3
        ///     ( format: bucket/prefix/prefix/filename.extension )
        /// </summary>
        [DefaultValue(false)]
        public Boolean ReturnListOfObjectKeys { get; set; }

        /// <summary>
        ///     You can specify Storage Class for uploaded files.
        ///     Standard is default.
        ///     Consult AWS S3 Documentation for others.
        /// </summary>
        public StorageClasses StorageClass { get; set; }
    }

    /// <summary>        
    ///     Filemask is Windows-style, eg. *.*, *file?.txt.
    ///     Bucket Name without s3://-prefix.
    /// </summary>
    public class Upload
    {
        /// <summary>
        ///     TASK OVERWRITES FILES WITH SAME PREFIX AND KEY!
        ///     Trailing slashes in bucketname or prefix will show as folder.
        ///     Filemask is Windows-style, eg. *.*, *file?.txt
        ///     Bucketname without s3://-prefix.
        /// </summary>
        /// <param name="input"/>
        /// <param name="parameters"/>
        /// <param name="options"/>
        /// <param name="cancellationToken"/>
        /// <returns>List&lt;string&gt; of filenames transferred. Optionally, return List&lt;string&gt; of object keys in S3.</returns>
        public static async Task<List<string>> UploadAsync(Input input, Parameters parameters, Options options, CancellationToken cancellationToken)
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

            // remove trailing slash to avoid empty folders
            if (parameters.Prefix.EndsWith("/"))
                parameters.Prefix = parameters.Prefix.TrimEnd('/');

            var filesToCopy = string.IsNullOrWhiteSpace(input.FileMask) ?
                Directory.GetFiles(input.FilePath) :
                Directory.GetFiles(input.FilePath, input.FileMask);

            if (options.ThrowErrorIfNoMatch && filesToCopy.Length < 1)
                throw new ArgumentException(@"No files match the filemask within supplied path. {nameof(input.FileMask)}");
            #endregion

            var result = new List<string>();

            using (var fileTransferUtility =
               new TransferUtility(
                   new AmazonS3Client(
                       parameters.AWSAccessKeyID,
                       parameters.AWSSecretAccessKey,
                       RegionSelection(parameters.Region))))
            {
                foreach (var file in filesToCopy)
                {
                    var request = new TransferUtilityUploadRequest
                    {
                        AutoCloseStream = true,
                        BucketName = parameters.BucketName,
                        FilePath = file,
                        StorageClass = StorageClassSelection(options.StorageClass),
                        //PartSize = 6291456, // 6 MB.
                        Key = string.IsNullOrWhiteSpace(parameters.Prefix) ?
                            Path.GetFileName(file) :
                            string.Join("/", parameters.Prefix, Path.GetFileName(file))
                    };

                    //register to event, when done, add to result list
                    request.UploadProgressEvent += (o, e) =>
                    {
                        if (e.PercentDone == 100)
                            if (options.ReturnListOfObjectKeys)
                                result.Add(request.Key);
                            else
                                result.Add(e.FilePath);
                    };

                    try
                    {
                        await fileTransferUtility.UploadAsync(request, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"AWS UploadAsync - Error occured while uploading file: {ex.Message}");
                    }
                }
            }
            return result;
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