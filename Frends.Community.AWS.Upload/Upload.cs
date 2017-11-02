using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.Tasks.Attributes;
using Frends.Community.AWS.Helpers;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace Frends.Community.AWS
{

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
        public string BucketName { get; set; }

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
        public static async Task<List<string>> UploadAsync(Input input, Parameters parameters, Options options, CancellationToken cancellationToken)
        {
            // First check to see if this task gets performed at all.
            cancellationToken.ThrowIfCancellationRequested();

            #region Error checks
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
                       Helpers.Helpers.RegionSelection(parameters.Region))))
            {
                foreach (var file in filesToCopy)
                {
                    var request = new TransferUtilityUploadRequest
                    {
                        AutoCloseStream = true,
                        BucketName = parameters.BucketName,
                        FilePath = file,
                        //StorageClass = StorageClassSelection(options.StorageClass),
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
    }
}